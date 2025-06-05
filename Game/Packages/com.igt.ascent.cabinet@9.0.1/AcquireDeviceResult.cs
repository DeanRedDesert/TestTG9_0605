//-----------------------------------------------------------------------
// <copyright file = "AcquireDeviceResult.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Class which contains a response for a request to acquire a device. If the request failed the response will
    /// indicate the reason for the failure.
    /// </summary>
    public class AcquireDeviceResult
    {
        /// <summary>
        /// Flag indicating if the device was acquired.
        /// </summary>
        public bool Acquired { get; }

        /// <summary>
        /// If the device was not acquired, then it was not acquired for this reason. If the device was acquired then
        /// this property will be null.
        /// </summary>
        public DeviceAcquisitionFailureReason? Reason { get; }

        /// <summary>
        /// Construct an instance of the AcquireDeviceResponse.
        /// </summary>
        /// <param name="acquired">Flag indicating if the device was acquired.</param>
        /// <param name="reason">
        /// If the device was not acquired then this will be the reason, if it was then this should be null.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="acquired"/> is true and <paramref name="reason"/> is not null or if
        /// <paramref name="acquired"/> is false and <paramref name="reason"/> is null.
        /// </exception>
        public AcquireDeviceResult(bool acquired, DeviceAcquisitionFailureReason? reason)
        {
            if(acquired && reason != null)
            {
                throw new ArgumentException("Failure reason cannot be set if the device was acquired.");
            }

            if(!acquired && reason == null)
            {
                throw new ArgumentException("Failure reason cannot be null if the device was not acquired.");
            }

            Acquired = acquired;
            Reason = reason;
        }
    }
}
