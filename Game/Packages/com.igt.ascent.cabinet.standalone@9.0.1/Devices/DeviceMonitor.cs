//-----------------------------------------------------------------------
// <copyright file = "DeviceMonitor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;
    using System.Threading;
    using Interop;

    /// <summary>
    /// A sealed class that provides a high-level wrapper around interop and unmanaged functionality to hook
    /// low level Windows Igt class USB device events.
    /// </summary>
    internal sealed class DeviceMonitor
    {
        #region Private Fields

        /// <summary>
        /// An object wrapping thread and related methods that shims between unmanaged and managed code.
        /// </summary>
        private static InteropWindowWrapperThread interopWindowWrapperThread;

        /// <summary>
        /// A synchronization object to co-ordinate orderly shutdown.
        /// </summary>
        private static AutoResetEvent interopWindowStatusAre = new AutoResetEvent(false);

        #endregion

        #region Events

        /// <summary>
        /// Event raised indicating an applicable Usb device status has changed.
        /// </summary>
        public static event EventHandler<DeviceChangeEventArgs> DeviceChangeEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts a <see cref="InteropWindowWrapperThread"/>thread running.
        /// <param name="igtUsbGuid">The <see cref="Guid"/> device class to monitor.</param>
        /// </summary>
        public static void Start(Guid igtUsbGuid)
        {
            interopWindowWrapperThread = new InteropWindowWrapperThread(IntPtr.Zero,
                                                                        igtUsbGuid,
                                                                        HandleIncomingInteropEvent,
                                                                        interopWindowStatusAre);
            interopWindowWrapperThread.StartThread();
        }

        /// <summary>
        /// Stop all threads, clean up and exit.
        /// </summary>
        public static void Stop()
        {
            interopWindowWrapperThread.StopThread();
            interopWindowStatusAre.WaitOne();
            DeviceChangeEvent = null;
            interopWindowWrapperThread.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Event callback via the unmanaged delegate, raises the event for managed code subscribers.
        /// an event.
        /// </summary>
        /// <param name="deviceRemovalType">The <see cref="DeviceChangeType"/> type of device event.</param>
        /// <param name="deviceName">The string indicating which device raised this event.</param>
        private static void HandleIncomingInteropEvent(DeviceChangeType deviceRemovalType, string deviceName)
        {
            DeviceChangeEvent?.Invoke(null, new DeviceChangeEventArgs(deviceRemovalType, deviceName));
        }

        #endregion
    }
}