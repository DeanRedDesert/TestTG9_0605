//-----------------------------------------------------------------------
// <copyright file = "DeviceControlException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Globalization;
    using Interop;

    /// <summary>
    /// This exception indicates that an error occurred during
    /// a device management function call.
    /// </summary>
    [Serializable]
    public class DeviceControlException : Exception
    {
        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "{0}  Last Error Code: {1}";

        /// <summary>
        /// Initialize a <see cref="DeviceControlException"/> with
        /// a message and a Win32 Error Code.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="errorCode">The Win32 Error Code associated with the exception.</param>
        public DeviceControlException(string message, int errorCode)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat,
                                 message, InterpretErrorCode(errorCode)))
        {
        }

        /// <summary>
        /// Check whether the given error code is an IGT USB status code.
        /// If yes, return the status enumeration value.
        /// Otherwise, return the converted unsigned value in Hex format.
        /// </summary>
        /// <param name="errorCode">The error code to interpret.</param>
        /// <returns>The formatted string of the error code.</returns>
        public static string InterpretErrorCode(int errorCode)
        {
            const uint igtUsbStatusMask = (uint)IgtUsbStatus.IGT_USB_STATUS_MASK;

            // Check if it is an IGT USB status code. Format the string accordingly.
            var result = ((uint)errorCode & igtUsbStatusMask) == igtUsbStatusMask
                            ? ((IgtUsbStatus)errorCode).ToString()
                            : $"0x{Win32Methods.ProcessErrorCode(errorCode):X}";

            return result;
        }
    }
}
