//-----------------------------------------------------------------------
// <copyright file = "InteropWindowWrapperThread.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// This class provides a wedge between Win 32/64 Wnd Proc message handling and C#, without using the .NET "Windows.Forms" namespace,
    /// which is not compatible with all versions of Unity's Mono .Net support.
    /// </summary>
    internal class InteropWindowWrapperThread : IDisposable
    {
        #region Interop delegate definitions

        /// <summary>
        /// Delegate definition to be called from .NET into an unmanaged DLL; loads at runtime the needed DLL, constructs and initializes
        /// the unmanaged window and runs the WndProc loop to capture and route windows messages as needed.
        /// </summary>
        /// <param name="thisHandle">
        /// A handle from the managed parent process that creates the window.
        /// </param>
        /// <param name="deviceClassGuid">
        /// The Guid class id for devices of interest.
        /// </param>
        /// <param name="clParms">
        /// Command line parameters to pass to the initialization window.
        /// </param>
        /// <param name="callbackDelegatePointer">
        /// A managed pointer that the unmanaged DLL will use to call back with device events into the managed
        /// calling code.
        /// </param>
        /// <param name="cmdShow">
        /// An int value that tells how to display the window. A normal display is the value of "10", hidden is the value of "0."
        /// </param>
        /// <returns>
        /// The return code from the unmanaged window as returned when the Wnd Proc loop exits and the window is closed.
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int UnmanagedDeviceEventWindowInitializationDelegate(IntPtr thisHandle, Guid deviceClassGuid, string clParms, IntPtr callbackDelegatePointer, int cmdShow);

        /// <summary>
        /// Delegate definition to be called from .NET into an unmanaged DLL; de-initializes the previously created window
        /// by ending the Wnd Proc loop and cleaning up necessary resources.
        /// </summary>
        /// <param name="thisHandle">
        /// The handle from the managed parent process that created the window.
        /// </param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UnmanagedDeviceEventWindowDeinitializationDelegate(IntPtr thisHandle);

        /// <summary>
        /// Delegate definition to be called from an unmanaged DLL into a managed .NET DLL; bubbles the appropriate windows message for
        /// a USB device insertion/removal event.
        /// </summary>
        /// <param name="deviceEventType">
        /// 0 for a device insertion event; 1 for a device removal event.
        /// </param>
        /// <param name="deviceNameBuffer">
        /// The full USB path name of the IGT device.
        /// </param>
        /// <param name="deviceNameBufferLength">
        /// The length of the actual characters returned in <paramref name="deviceNameBuffer"/>.
        /// </param>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        delegate void CallbackDelegate(int deviceEventType, StringBuilder deviceNameBuffer, int deviceNameBufferLength);

        #endregion

        #region Private members

        /// <summary>
        /// The unmanaged Dll library name to load.
        /// </summary>
        private const string LibName = @"Standalone.Devices.UnmanagedHelper.dll";

        /// <summary>
        /// A <see cref="CallbackDelegate"/> used to reference the supplied managed callback method reference. 
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly CallbackDelegate callbackDelegate;

        /// <summary>
        /// Managed pointer to unmanaged delegate.
        /// </summary>
        private readonly IntPtr callbackDelegatePointer;

        /// <summary>
        /// Managed pointer to unmanaged DLL library.
        /// </summary>
        private readonly IntPtr pUnmanagedWindowDllPointer;

        /// <summary>
        /// Managed pointer to the parent owner of the created unmanaged window.
        /// </summary>
        private readonly IntPtr parentWindowHandle;

        /// <summary>
        /// Delegate used to invoke the unmanaged window initialization code.
        /// </summary>
        private readonly UnmanagedDeviceEventWindowInitializationDelegate deviceWindowInitDelegate;

        /// <summary>
        /// Delegate used to invoke the unmanaged window de-initialization code.
        /// </summary>
        private readonly UnmanagedDeviceEventWindowDeinitializationDelegate deviceWindowDeinitDelegate;

        /// <summary>
        /// The Guid for Usb devices to be monitored.
        /// </summary>
        private readonly Guid guidForIgtUsbDevices;

        /// <summary>
        /// The <see cref="Action"/> definition to perform when a Usb device event is handled.
        /// </summary>
        private readonly Action<DeviceChangeType, string> deviceEventAction;

        /// <summary>
        /// A handle assigned and kept alive throughout the application to prevent garbage collection issues.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private readonly GCHandle gchCallbackDelegate;

        /// <summary>
        /// A synchronization object to co-ordinate orderly shutdown.
        /// </summary>
        private readonly AutoResetEvent interopWindowStatusAre;

        /// <summary>
        /// A thread to help marshall events from the unmanaged window device events to the managed caller.
        /// </summary>
        private Thread interopWindowWrapperThread;

        #endregion

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="parentWindowHandle">
        /// The underlying window handle of the parent owner window.
        /// </param>
        /// <param name="guidForIgtUsbDevices">
        /// A valid IGT Usb device Guid.
        /// </param>
        /// <param name="handleEvent">
        /// An <see cref="Action"/> designating a higher level managed callback event handler and parameters to invoke when
        /// the desired Usb device event is fired.
        /// </param>
        /// <param name="interopWindowStatusAre">
        /// An <see cref="AutoResetEvent"/> synchronization object to co-ordinate orderly shutdown between different threads.
        /// </param>
        /// <exception cref="Exception">
		/// Thrown if there is any problem finding, loading or initializing the unmanaged Dll. The inner exception will
        /// contain the original exception that was thrown.
		/// </exception>
        public InteropWindowWrapperThread(IntPtr parentWindowHandle,
                                         Guid guidForIgtUsbDevices,
                                         Action<DeviceChangeType, string> handleEvent,
                                         AutoResetEvent interopWindowStatusAre)
        {
            this.interopWindowStatusAre = interopWindowStatusAre;
            this.parentWindowHandle = parentWindowHandle;
            this.guidForIgtUsbDevices = guidForIgtUsbDevices;
            deviceEventAction = handleEvent;

            try
            {
                // Gets an IntPtr pointer to the unmanaged library.
                var fullPathToLibrary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LibName);
                pUnmanagedWindowDllPointer = Win32Methods.LoadLibrary(fullPathToLibrary);

                // Gets the function addressed of any exported methods.
                var pAddressOfInitFunctionToCall =
                    Win32Methods.GetProcAddress(pUnmanagedWindowDllPointer, "InitializeDeviceEventWindow");
                var pAddressOfDeinitFunctionToCall =
                    Win32Methods.GetProcAddress(pUnmanagedWindowDllPointer, "DeinitializeDeviceEventWindow");

                // Attach .NET delegates to the function pointers.
                deviceWindowInitDelegate =
                    (UnmanagedDeviceEventWindowInitializationDelegate)Marshal.GetDelegateForFunctionPointer(
                        pAddressOfInitFunctionToCall,
                        typeof(UnmanagedDeviceEventWindowInitializationDelegate));

                deviceWindowDeinitDelegate =
                    (UnmanagedDeviceEventWindowDeinitializationDelegate)Marshal.GetDelegateForFunctionPointer(
                        pAddressOfDeinitFunctionToCall,
                        typeof(UnmanagedDeviceEventWindowDeinitializationDelegate));

                // Set the .NET delegate to be passed into the unmanaged DLL to allow it to callback into managed code.
                callbackDelegate = DeviceEventCallback;
                callbackDelegatePointer = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

                // Recommended to keep all delegates alive until this thread exits.
                gchCallbackDelegate = GCHandle.Alloc(callbackDelegatePointer);
                GC.Collect();
            }
            catch(Exception ex)
            {
                throw new Exception($@"Unmanaged interop code initialization failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates and starts the thread that the unmanaged window will run in.
        /// </summary>
        public void StartThread()
        {
            interopWindowWrapperThread = new Thread(ShowInteropWindow)
            {
                Name = @"InteropWindowWrapperThread",
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

            interopWindowWrapperThread.Start();
        }

        /// <summary>
        /// Tells the unmanaged window to first exit and clean-up, and then stops the wrapper thread.
        /// </summary>
        public void StopThread()
        {
            deviceWindowDeinitDelegate(parentWindowHandle);
            // Unmanaged window has exited, managed cleanup and shutdown can go ahead.
            interopWindowStatusAre.Set();
        }

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Win32Methods.FreeLibrary(pUnmanagedWindowDllPointer);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// The Thread Run method; creates and shows the window. This call will block until the unmanaged window exits (via a manual window
        /// close or a call to the DeInitialize delegate above); this wrapper thread will exit afterwards.
        /// </summary>
        private void ShowInteropWindow()
        {
            // Use '10' for displaying window in regular mode.
            // Use '0' for hiding window.
            var windowDisplayState = 0;
            // Blocks until the window exits.
            deviceWindowInitDelegate(parentWindowHandle, guidForIgtUsbDevices,"", callbackDelegatePointer, windowDisplayState);
        }

        /// <summary>
        /// Callback that is called from the unmanaged DLL when a USB device insertion/removal event is detected. This will bubble
        /// up to the unmanaged code as an event.
        /// </summary>
        /// <param name="deviceEvent">
        /// The device event type; '0' for device insertion, '1' for device removal.</param>
        /// <param name="deviceNameBuffer">
        /// The StringBuilder buffer that will be written by the unmanaged code and indicates the full Usb IGT pathname of the device.
        /// </param>
        /// <param name="deviceNameBufferLength">The actual length of the text written to the <paramref name="deviceNameBuffer"/>.</param>
        private void DeviceEventCallback(int deviceEvent, StringBuilder deviceNameBuffer, int deviceNameBufferLength)
        {
            deviceEventAction((DeviceChangeType)deviceEvent, deviceNameBuffer.ToString());
        }

        #endregion
    }
}