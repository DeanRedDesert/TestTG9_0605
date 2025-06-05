//-----------------------------------------------------------------------
// <copyright file = "DeviceBase.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using Interop;

    /// <summary>
    /// A base class that defines that functionalities that
    /// are common to all physical devices.
    /// </summary>
    [Serializable]
    public abstract class DeviceBase
    {
        #region Nested Classes

        /// <summary>
        /// Enumeration which indicates the type of the recipient
        /// of a USB command.
        /// </summary>
        [Serializable]
        protected enum UsbRecipient
        {
            /// <summary>
            /// Command is to a device.
            /// </summary>
            Device,

            /// <summary>
            /// Command is to an interface.
            /// </summary>
            Interface,

            /// <summary>
            /// Command is to an end point.
            /// </summary>
            EndPoint,

            /// <summary>
            /// Other recipient.
            /// </summary>
            Other
        }

        #endregion

        #region Constants

        /// <summary>
        /// Size of the buffer used for receiving a query response.
        /// </summary>
        private const int QueryResponseSize = 512;

        #endregion

        #region Events

        /// <summary>
        /// Event posted when there is an update of the information on the device.
        /// </summary>
        public event EventHandler<DeviceInformationUpdateEventArgs> DeviceInformationUpdateEvent;

        /// <summary>
        /// Event posted when a device requested an active polling.
        /// </summary>
        internal event EventHandler<DeviceRequestPollingEventArgs> DeviceRequestPollingEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Get the address of the device driver.
        /// </summary>
        /// <remarks>
        /// This string can be viewed as the identifier of a communication
        /// channel, which could be shared by multiple devices, each using
        /// a unique <see cref="InterfaceNumber"/>.
        /// 
        /// The address and the interface number consist of the unique
        /// <see cref="DriverName"/> for each device.  In other words, the
        /// driver address is the driver name with interface number cleared.
        /// 
        /// At each address, there should be one Feature Zero present, which is
        /// responsible for status reading for all devices at the same address.
        /// </remarks>
        public string DriverAddress { get; private set; }

        /// <summary>
        /// Get the sub feature name of the device.
        /// </summary>
        public string SubFeatureName { get; private set; }

        /// <summary>
        /// Zero-based value identifying the number of the interface
        /// used by the device.
        /// </summary>
        public byte InterfaceNumber { get; private set; }

        /// <summary>
        /// Get the common descriptors of the device.
        /// </summary>
        public string CommonDescriptors { get; private set; }

        /// <summary>
        /// Get a string describing the feature descriptors of the device.
        /// </summary>
        public abstract string FeatureDescriptors { get; }

        #endregion

        #region Fields

        /// <summary>
        /// The name of the device driver.
        /// </summary>
        protected readonly string DriverName;

        /// <summary>
        /// The target to which a feature command is sent.
        /// </summary>
        protected readonly ushort InterfaceTarget;

        /// <summary>
        /// The string builder used to store the information update
        /// returned by message handling.
        /// </summary>
        /// <devdoc>
        /// Make it a class member for better performance.
        /// </devdoc>
        protected readonly StringBuilder InformationBuilder = new StringBuilder();

        /// <summary>
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </summary>
        protected readonly bool BypassHardware;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="DeviceBase"/> class for
        /// a specific device type, given the required device data.
        /// </summary>
        /// <param name="deviceData">
        /// The data for the device.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceData"/> is null.
        /// </exception>
        protected DeviceBase(UsbDeviceData deviceData)
            : this(deviceData, false)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="DeviceBase"/> class for
        /// a specific device type, given the required device data.
        /// </summary>
        /// <param name="deviceData">
        /// The data for the device.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceData"/> is null.
        /// </exception>
        protected DeviceBase(UsbDeviceData deviceData, bool bypassHardware)
        {
            if(deviceData == null)
            {
                throw new ArgumentNullException(nameof(deviceData));
            }

            BypassHardware = bypassHardware;

            DriverName = deviceData.DriverName;
            DriverAddress = ComputeDriverAddress(DriverName);

            SubFeatureName = deviceData.InterfaceName;

            InterfaceNumber = deviceData.InterfaceDescriptor.InterfaceNumber;
            InterfaceTarget = FormatTarget(UsbRecipient.Interface, InterfaceNumber);
 
            CommonDescriptors = WriteCommonDescriptorsToString(deviceData);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset the device.
        /// </summary>
        /// <returns>True if succeed, false otherwise.</returns>
        public bool Reset()
        {
            // Reset ignores device id.
            var payload = new UsbCommandPayload((byte)GlobalCommandCode.Reset, 0);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Do self test on the device.
        /// </summary>
        /// <returns>True if succeed, false otherwise.</returns>
        public bool SelfTest()
        {
            // Self Test ignores device id.
            var payload = new UsbCommandPayload((byte)GlobalCommandCode.SelfTest, 0);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Get and process the status of the device.
        /// </summary>
        /// <returns>A string describing the device status.</returns>
        public string PollDevice()
        {
            var result = string.Empty;

            var statusMessage = QueryStatus();

            if(statusMessage != null)
            {
                // Reset the information output.
                InformationBuilder.Length = 0;

                HandleStatus(statusMessage, InformationBuilder);

                // If there is information update, raise the event.
                if(InformationBuilder.Length > 0)
                {
                    result = InformationBuilder.ToString();
                    OnInformationUpdate(result);
                }
            }

            return result;
        }

        /// <summary>
        /// Remove the interface number from a driver name
        /// to obtain the driver address of a device.
        /// </summary>
        /// <param name="driverName">The driver name used as the base for manipulation.</param>
        /// <returns>The driver address as the manipulation result.</returns>
        internal static string ComputeDriverAddress(string driverName)
        {
            // A driver name looks like:
            // "\\?\usb#vid_0a70&pid_0310&mi_01#6&1945f92f&0&0001#{c638df45-2530-498d-9d1a-5bc979b1ffb0}"
            // where mi_01 and &0001# indicate interface number of 1.

            var result = driverName;

            // Replace interface number with 0.
            const string targetText = "00";

            // Process "mi_01" in the example driver name.
            var targetPos = result.IndexOf("mi_", StringComparison.Ordinal) + 3;
            if(targetPos >= 0 && targetPos + 1 < result.Length)
            {
                result = result.Remove(targetPos, 2);
                result = result.Insert(targetPos, targetText);
            }

            // Process "&0001#" in the example driver name.
            targetPos = result.LastIndexOf('#') - 2;
            if(targetPos >= 0 && targetPos + 1 < result.Length)
            {
                result = result.Remove(targetPos, 2);
                result = result.Insert(targetPos, targetText);
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Format a request target, which combines recipient type in the low byte,
        /// and the recipient index in the high byte.
        /// </summary>
        /// <param name="recipient">Type of the recipient.</param>
        /// <param name="index">Index of the recipient.</param>
        /// <returns>Formatted target value.</returns>
        protected static ushort FormatTarget(UsbRecipient recipient, byte index)
        {
            return (ushort)((int)recipient | ((index & 0xFF) << 8));
        }

        /// <summary>
        /// Write the common descriptors of a device to a string.
        /// </summary>
        /// <param name="deviceData">The device data that provides the common descriptors.</param>
        /// <returns>The string describing the descriptors.</returns>
        protected static string WriteCommonDescriptorsToString(UsbDeviceData deviceData)
        {
            string result;

            if(deviceData != null)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("String Descriptors -");
                stringBuilder.AppendLine("\t Vender = " + deviceData.VendorName);
                stringBuilder.AppendLine("\t Product = " + deviceData.ProductName);
                stringBuilder.AppendLine("\t Interface = " + deviceData.InterfaceName);
                stringBuilder.AppendLine("\t Serial = " + deviceData.SerialName);
                stringBuilder.AppendLine("\t File Name and Date = " + deviceData.FileName);
                stringBuilder.AppendLine();

                stringBuilder.Append(deviceData.DeviceDescriptor);
                stringBuilder.AppendLine();
                stringBuilder.Append(deviceData.ConfigurationDescriptor);
                stringBuilder.AppendLine();
                stringBuilder.Append(deviceData.InterfaceDescriptor);
                stringBuilder.AppendLine();
                stringBuilder.Append(deviceData.FunctionalDescriptor);
                stringBuilder.AppendLine();
                stringBuilder.Append(deviceData.EndPointDescriptor);

                result = stringBuilder.ToString();
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Send a command to the device without additional data.
        /// </summary>
        /// <param name="payload">Payload of the command.</param>
        /// <returns>
        /// True if the command succeeds, false otherwise.
        /// </returns>
        protected virtual bool SendFeatureCommand(UsbCommandPayload payload)
        {
            return SendFeatureCommand(payload, null);
        }

        /// <summary>
        /// Send a command to the device.
        /// </summary>
        /// <param name="payload">Payload of the command.</param>
        /// <param name="data">Additional data required by the command.</param>
        /// <returns>
        /// True if the command succeeds, false otherwise.
        /// </returns>
        protected virtual bool SendFeatureCommand(UsbCommandPayload payload, byte[] data)
        {
            if(BypassHardware)
                return true;

            using(var deviceHandle = DeviceManager.CreateDeviceHandle(DriverName))
            {
                var vendorRequest = new UsbVendorRequest
                {
                    Target = InterfaceTarget,
                    Type = UsbVendorRequest.Description.RequestOut,
                    Request = UsbVendorRequest.RequestCode.Command,
                    ReservedBits = 0,
                    Value = 0
                };

                var buffer = new DeviceControlCommand(payload, data).GetBytes();

                var result = Win32Methods.DeviceIoControl(deviceHandle,
                                                          IgtUsbIoControlCode.ClassOrVendorRequest,
                                                          vendorRequest,
                                                          Marshal.SizeOf(vendorRequest),
                                                          buffer,
                                                          buffer.Length,
                                                          out _,
                                                          IntPtr.Zero);

                return result;
            }
        }

        /// <summary>
        /// Query a feature of the device.
        /// </summary>
        /// <param name="queryCode">The code indicating what to query for.</param>
        /// <returns>
        /// A byte array containing the response data to the query.
        /// Null if the command fails.
        /// </returns>
        protected virtual byte[] QueryFeature(byte queryCode)
        {
            if(BypassHardware)
                return null;

            byte[] result = null;

            using(var deviceHandle = DeviceManager.CreateDeviceHandle(DriverName))
            {
                var vendorRequest = new UsbVendorRequest
                {
                    Type = UsbVendorRequest.Description.RequestIn,
                    Target = InterfaceTarget,
                    Request = UsbVendorRequest.RequestCode.Query,
                    ReservedBits = 0,
                    Value = queryCode
                };

                var buffer = new byte[QueryResponseSize];

                var success = Win32Methods.DeviceIoControl(deviceHandle,
                                                           IgtUsbIoControlCode.ClassOrVendorRequest,
                                                           vendorRequest,
                                                           Marshal.SizeOf(vendorRequest),
                                                           buffer,
                                                           buffer.Length,
                                                           out var bytesReturned,
                                                           IntPtr.Zero);

                if(success && bytesReturned <= QueryResponseSize)
                {
                    // Retrieve the response data excluding the header.
                    var header = new IgtClassStandardHeader();
                    header.Unpack(buffer, 0);

                    // Validate the size of received data.
                    if(bytesReturned >= header.DataSize + header.PayloadLength)
                    {
                        var payloadLength = header.PayloadLength;

                        result = new byte[payloadLength];
                        Array.Copy(buffer, header.DataSize, result, 0, payloadLength);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Clear a specific tilt status for the device.
        /// </summary>
        /// <param name="statusCode">The status to clear.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        protected bool ClearStatus(ushort statusCode)
        {
            // Clear Status ignores device id.
            var payload = new UsbSingleUshortCommandPayload((byte)GlobalCommandCode.ClearStatus,
                                                            0,
                                                            statusCode);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Clear all clearable statuses.
        /// </summary>
        /// <returns>True if succeed, false otherwise.</returns>
        protected bool ClearAllStatuses()
        {
            return ClearStatuses(null);
        }

        /// <summary>
        /// Clear a list of tilt statuses for the device.
        /// </summary>
        /// <param name="statusCodes">
        /// The list statuses to clear.
        /// If null or empty, it clears all clearable statues.
        /// </param>
        /// <returns>True if succeed, false otherwise.</returns>
        protected bool ClearStatuses(IList<ushort> statusCodes)
        {
            // Clear Status ignores device id.
            var payload = new UsbCommandPayload((byte)GlobalCommandCode.ClearStatus, 0);

            byte[] data = null;

            if(statusCodes?.Count > 0)
            {
                using(var stream = new MemoryStream())
                {
                    foreach(var statusCode in statusCodes)
                    {
                        stream.Write(BitConverter.GetBytes(statusCode), 0, sizeof(ushort));
                    }

                    data = stream.ToArray();
                }
            }

            return SendFeatureCommand(payload, data);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Query the statuses of the device.
        /// </summary>
        /// <returns>
        /// A <see cref="UsbStatusMessage"/> containing a list of status records.
        /// </returns>
        internal UsbStatusMessage QueryStatus()
        {
            UsbStatusMessage result = null;

            var data = QueryFeature((byte)GlobalQueryCode.GetStatus);

            if(data != null)
            {
                result = new UsbStatusMessage();
                result.Unpack(data, 0);
            }

            return result;
        }

        /// <summary>
        /// Handle an USB message received from the device driver.
        /// </summary>
        /// <param name="usbMessage">
        /// The message to handle.
        /// </param>
        internal virtual void HandleMessage(UsbMessageEventArgs usbMessage)
        {
            // Only handle events related this device.
            if(usbMessage.InterfaceNumber != InterfaceNumber)
                return;

            // Reset the information output.
            InformationBuilder.Length = 0;

            switch(usbMessage.Type)
            {
                case UsbReportType.Status:
                    {
                        var statusMessage = new UsbStatusMessage();
                        statusMessage.Unpack(usbMessage.MessageData, 0);

                        HandleStatus(statusMessage, InformationBuilder);
                        break;
                    }

                case UsbReportType.ControlMessageRejected:
                    {
                        var rejectionMessage = new UsbMessageRejectedMessage();
                        rejectionMessage.Unpack(usbMessage.MessageData, 0);

                        HandleControlMessageRejected(rejectionMessage, InformationBuilder);
                        break;
                    }

                // No need to handling the following message types for now.
                case UsbReportType.Event:
                case UsbReportType.SequenceMessageProcessed:
                case UsbReportType.BulkMessageRejected:
                    break;
            }

            // If there is information update, raise the event.
            if(InformationBuilder.Length > 0)
            {
                OnInformationUpdate(InformationBuilder.ToString());
            }
        }

        /// <summary>
        /// Handle an USB Status Message received from the device driver.
        /// </summary>
        /// <param name="statusMessage">
        /// The status message to handle.
        /// </param>
        /// <param name="infoBuilder">
        /// The builder of strings describing the statuses contained in the message.
        /// If null, no information needs to be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="statusMessage"/> is null.
        /// </exception>
        internal virtual void HandleStatus(UsbStatusMessage statusMessage,
                                           StringBuilder infoBuilder)
        {
            if(statusMessage == null)
            {
                throw new ArgumentNullException(nameof(statusMessage));
            }

            if(infoBuilder != null)
            {
                foreach(var statusRecord in statusMessage.StatusRecords)
                {
                    // Add the descriptive text in the status message to information string.
                    if(statusRecord.TextData != null)
                    {
                        infoBuilder.AppendLine("Status Description: " + statusRecord.TextData);
                    }

                    // Add the global status to the information string.
                    if(statusRecord.IsGlobalStatus)
                    {
                        infoBuilder.AppendFormat("Device Update: {0}", (GlobalStatusCode)statusRecord.Status);
                        infoBuilder.AppendLine();
                    }
                }
            }
        }

        /// <summary>
        /// Handle an USB Message Rejected Message received from the device driver.
        /// </summary>
        /// <param name="rejectionMessage">
        /// The rejection message to handle.
        /// </param>
        /// <param name="infoBuilder">
        /// The builder of strings describing the rejections contained in the message.
        /// If null, no information needs to be returned.
        /// </param>
        internal virtual void HandleControlMessageRejected(UsbMessageRejectedMessage rejectionMessage,
                                                           StringBuilder infoBuilder)
        {
            if(rejectionMessage == null)
            {
                throw new ArgumentNullException(nameof(rejectionMessage));
            }

            if(infoBuilder != null)
            {
                // We only care about the reason.
                // Reason-specific data is ignored for now.
                infoBuilder.AppendFormat("Last command was rejected due to the reason of {0}", rejectionMessage.Reason);
                infoBuilder.AppendLine();
            }

            // TODO: Post tilt when a command is rejected.
            // Something to consider when standalone tilt manager is in place.
        }

        /// <summary>
        /// Raise <see cref="DeviceInformationUpdateEvent"/> event for the device.
        /// </summary>
        /// <param name="deviceInformation">
        /// The latest update of the information on a device.
        /// </param>
        internal virtual void OnInformationUpdate(string deviceInformation)
        {
            DeviceInformationUpdateEvent?.Invoke(this, new DeviceInformationUpdateEventArgs(deviceInformation));
        }

        /// <summary>
        /// Raise <see cref="DeviceRequestPollingEvent"/> event for the device.
        /// </summary>
        internal virtual void OnRequestPolling()
        {
            DeviceRequestPollingEvent?.Invoke(this, new DeviceRequestPollingEventArgs(InterfaceNumber));
        }

        #endregion

        #endregion
    }
}
