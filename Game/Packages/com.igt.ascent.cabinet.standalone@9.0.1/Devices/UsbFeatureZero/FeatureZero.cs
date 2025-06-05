//-----------------------------------------------------------------------
// <copyright file = "FeatureZero.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.UsbFeatureZero
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using IgtUsbDevice;
    using Interop;
    using Logging;

    /// <summary>
    /// The class represents the common feature present in
    /// every IGT Class peripheral.
    /// It handles the peripheral hardware statuses, and
    /// device verification requests etc..
    /// </summary>
    [Serializable]
    internal class FeatureZero : DeviceBase
    {
        #region Constants

        /// <summary>
        /// Size of the buffer receiving interrupt in messages.
        /// </summary>
        /// <devdoc>
        /// Empirical value copied from AVP code.
        /// </devdoc>
        private const int InterruptBufferSize = 255;

        /// <summary>
        /// Size of the feature descriptor header.
        /// 
        /// The header of a Feature Zero's feature specific descriptor
        /// consists of the following fields:
        ///     <list type="number">
        ///         <item>Total length of the feature specific data (1 byte).</item>
        ///         <item>Descriptor type (1 byte).</item>
        ///     </list>
        /// </summary>
        private const int FeatureDescriptorHeaderSize = 2;

        /// <summary>
        /// Get status every 2 seconds to keep the communication alive.
        /// </summary>
        private const int KeepAliveInterval = 2000;

        #endregion

        #region Events

        /// <summary>
        /// Event indicating an interrupt in message has been received from the device driver.
        /// </summary>
        public event EventHandler<UsbMessageEventArgs> UsbMessageEvent;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string FeatureDescriptors => featureZeroDescriptor.ToString();

        #endregion

        #region Fields

        /// <summary>
        /// The descriptor for the feature zero.
        /// </summary>
        private readonly FeatureZeroDescriptor featureZeroDescriptor;

        /// <summary>
        /// The end point address used by the feature zero.
        /// </summary>
        private readonly byte endPointAddress;

        /// <summary>
        /// The flag indicating if the feature zero is running.
        /// </summary>
        private volatile bool isRunning;

        /// <summary>
        /// The worker thread used for receiving interrupt in
        /// messages from devices.
        /// </summary>
        private Thread workerThread;

        /// <summary>
        /// The timer to track when to query the device in order to
        /// keep the communication alive.
        /// </summary>
        private Timer keepAliveTimer;

        /// <summary>
        /// The flag indicating if the keep alive timer has fired.
        /// </summary>
        private volatile bool tickKeepAlive;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="FeatureZero"/> class,
        /// given the required device data.
        /// </summary>
        /// <param name="deviceData">
        /// The device data for the reel device.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="deviceData"/> contains invalid data.
        /// </exception>
        internal FeatureZero(UsbDeviceData deviceData, bool bypassHardware = false)
            : base(deviceData, bypassHardware)
        {
            if(deviceData == null)
            {
                throw new ArgumentNullException(nameof(deviceData));
            }

            if(!deviceData.EndPointDescriptor.IsInterruptIn())
            {
                throw new InvalidUsbDeviceDataException(
                    "The end point for Feature Zero does not support Interrupt In messages.");
            }

            // There should be only 1 feature specific descriptor to parse.
            if(deviceData.FunctionalDescriptor.NumberAdditionalDescriptors != 1)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect number of feature descriptors {deviceData.FunctionalDescriptor.NumberAdditionalDescriptors} for the Feature Zero.  Should be 1.");
            }

            featureZeroDescriptor = new FeatureZeroDescriptor();
            featureZeroDescriptor.Unpack(deviceData.FeatureDescriptorData, FeatureDescriptorHeaderSize);

            endPointAddress = deviceData.EndPointDescriptor.EndPointAddress;

            // Create the keep alive timer.
            // Make it disabled until the feature zero is started.
            keepAliveTimer = new Timer(arg => { tickKeepAlive = true; },
                                       null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start running Feature Zero.
        /// </summary>
        public void Start()
        {
            if(BypassHardware)
                return;

            if(!isRunning && workerThread == null)
            {
                workerThread = new Thread(Run);
                workerThread.Start();

                // Enable the keep alive timer.
                keepAliveTimer.Change(0, KeepAliveInterval);
            }
        }

        /// <summary>
        /// Stop running Feature Zero.
        /// </summary>
        public void Stop()
        {
            if(BypassHardware)
                return;

            isRunning = false;

            // Dispose the keep alive timer.
            if(keepAliveTimer != null)
            {
                keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

                using(var timerDisposed = new ManualResetEvent(false))
                {
                    keepAliveTimer.Dispose(timerDisposed);

                    // Wait till all timer's callbacks have completed.
                    timerDisposed.WaitOne(500);
                }

                keepAliveTimer = null;
            }

            // Terminate the worker thread.
            if(workerThread != null)
            {
                if(workerThread.IsAlive)
                {
                    ResetUsb();
                }

                // If it does not terminate within 2 seconds, abort.
                var terminated = workerThread.Join(2000);
                if(!terminated)
                {
                    workerThread.Abort();
                }

                workerThread = null;
            }
        }

        /// <summary>
        /// Poll the feature Zero to keep the communication alive.
        /// </summary>
        public void KeepAlive()
        {
            if(BypassHardware)
                return;

            // Do the polling only if the timer has fired,
            // and the feature zero is still running.
            if(tickKeepAlive && isRunning)
            {
                tickKeepAlive = false;
                PollDevice();
            }
        }

        #endregion

        #region Main Thread

        /// <summary>
        /// Reset the USB device.
        /// </summary>
        private void ResetUsb()
        {
            using(var deviceHandle = DeviceManager.CreateDeviceHandle(DriverName))
            {
                // In this function call, cannot use IntPtr.Zero for lpInBuffer.
                // VS is fine with it, but Mono will issue the following runtime error:
                // "No PInvoke conversion exists for value passed to Object-typed parameter."
                Win32Methods.DeviceIoControl(deviceHandle,
                                             IgtUsbIoControlCode.ResetDevice,
                                             0,
                                             0,
                                             IntPtr.Zero,
                                             0,
                                             out _,
                                             IntPtr.Zero);
            }
        }

        /// <inheritdoc />
        internal override void HandleStatus(UsbStatusMessage statusMessage, StringBuilder infoBuilder)
        {
            if(statusMessage == null)
            {
                throw new ArgumentNullException(nameof(statusMessage));
            }

            base.HandleStatus(statusMessage, infoBuilder);

            foreach(var statusRecord in statusMessage.StatusRecords)
            {
                // Handle global statuses.
                if(statusRecord.IsGlobalStatus)
                {
                    var status = (GlobalStatusCode)statusRecord.Status;

                    switch(status)
                    {
                        case GlobalStatusCode.CommunicationTimedOut:
                            ClearStatus(statusRecord.Status);
                            break;
                    }
                }

                // TODO: Post tilt when feature zero tilts.
                // Something to consider when standalone tilt manager is in place.
            }
        }

        #endregion

        #region Worker Thread

        /// <summary>
        /// The ThreadStart delegate to be invoked when the worker thread begins executing.
        /// </summary>
        private void Run()
        {
            try
            {
                var path = $"{DriverName}\\{endPointAddress}";

                using(var deviceHandle = DeviceManager.CreateDeviceHandle(path))
                {
                    // Create a buffer for output.
                    var buffer = new byte[sizeof(ulong)];

                    // Pin the object first in order to receive data.
                    var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                    int bytesReturned;
                    try
                    {
                        // Get the pipe handle.
                        // In this function call, cannot use IntPtr.Zero for lpInBuffer.
                        // VS is fine with it, but Mono will issue the following runtime error:
                        // "No PInvoke conversion exists for value passed to Object-typed parameter."
                        isRunning = Win32Methods.DeviceIoControl(deviceHandle,
                                                                 IgtUsbIoControlCode.GetHandle,
                                                                 0,
                                                                 0,
                                                                 gcHandle.AddrOfPinnedObject(),
                                                                 sizeof(ulong),
                                                                 out bytesReturned,
                                                                 IntPtr.Zero);

                        // Convert the byte array to an integer.
                        var pipeHandle = BitConverter.ToInt64(buffer, 0);

                        // Link the pipe handle to the device handle.
                        if(isRunning && pipeHandle != 0)
                        {
                            isRunning = Win32Methods.DeviceIoControl(deviceHandle,
                                                                     IgtUsbIoControlCode.LinkPipe,
                                                                     pipeHandle,
                                                                     sizeof(ulong),
                                                                     IntPtr.Zero,
                                                                     0,
                                                                     out bytesReturned,
                                                                     IntPtr.Zero);
                        }
                    }
                    finally
                    {
                        gcHandle.Free();
                    }

                    // Wait for interruption.
                    while(isRunning)
                    {
                        var interruptBuffer = new byte[InterruptBufferSize];

                        // Block until read complete.
                        var success = Win32Methods.ReadFile(deviceHandle,
                                                            interruptBuffer,
                                                            InterruptBufferSize,
                                                            out bytesReturned,
                                                            IntPtr.Zero);

                        if(!success)
                        {
                            isRunning = false;
                            break;
                        }

                        // Discard if buffer overflow.
                        if(bytesReturned > InterruptBufferSize)
                        {
                            Log.WriteWarning("Discard an USB message due to buffer overrun.");
                            continue;
                        }

                        // Trim the array.
                        var returnedBuffer = new byte[bytesReturned];
                        Array.Copy(interruptBuffer, returnedBuffer, bytesReturned);

                        var eventArgs = ParseUsbMessage(returnedBuffer);

                        // Discard the data if a message can not be retrieved.
                        if(eventArgs == null)
                        {
                            Log.WriteWarning("Discard an USB message due to parsing errors.");
                            continue;
                        }

                        // Raise the event.
                        UsbMessageEvent?.Invoke(this, eventArgs);
                    }
                }
            }
            catch(Exception exception)
            {
                isRunning = false;
                Log.WriteError(exception.ToString());
                throw;
            }
        }

        /// <summary>
        /// Process an incoming message contained in a byte array.
        /// </summary>
        /// <param name="buffer">
        /// The buffer containing the data to process.
        /// </param>
        /// <returns>
        /// An instance of <see cref="UsbMessageEventArgs"/> that contains the
        /// information on the USB message.
        /// </returns>
        private static UsbMessageEventArgs ParseUsbMessage(byte[] buffer)
        {
            UsbMessageEventArgs result = null;
            var offset = 0;

            // Retrieve the standard header.
            var standardHeader = new IgtClassStandardHeader();
            standardHeader.Unpack(buffer, offset);

            offset += standardHeader.DataSize;

            // Validate the buffer size against the payload length.
            if(buffer.Length - offset >= standardHeader.PayloadLength)
            {
                // Retrieve the report header.
                var messageHeader = new UsbMessageHeader();
                messageHeader.Unpack(buffer, offset);

                offset += messageHeader.DataSize;

                // Validate the payload length.
                if(standardHeader.PayloadLength > messageHeader.DataSize)
                {
                    var dataLength = buffer.Length - offset;
                    var messageData = new byte[dataLength];
                    Array.Copy(buffer, offset, messageData, 0, dataLength);

                    result = new UsbMessageEventArgs(messageHeader.InterfaceNumber,
                                                     messageHeader.ReportType,
                                                     messageData);
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}
