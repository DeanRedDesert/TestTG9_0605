//-----------------------------------------------------------------------
// <copyright file = "AcquireGroupsResult.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class which contains a response for a request to acquire groups. If the request failed the response will
    /// indicate the reason for the failure.
    /// </summary>
    public class AcquireGroupsResult : AcquireDeviceResult
    {
        /// <summary>
        /// If all the groups were not acquired, this dictionary lists the status of the groups that were and were not acquired.
        /// This collection will be null if the requested groups were acquired or disconnected.
        /// </summary>
        public IDictionary<uint, GroupAcquisitionStatus> GroupAcquisitionStatuses { get; }

        /// <summary>
        /// Construct an instance of the AcquireGroupsResult.
        /// </summary>
        /// <param name="acquired">Flag indicating if all the groups were acquired.</param>
        /// <param name="reason">
        /// If the device was not acquired then this will be the reason, if it was then this should be null.
        /// </param>
        /// <param name="statuses">
        /// If any group was not acquired then this will be the list of status for each group.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="statuses"/> is null or empty if
        /// <paramref name="reason"/> is <see cref="DeviceAcquisitionFailureReason.RequestQueued"/>.
        /// </exception>
        public AcquireGroupsResult(bool acquired,
                                   DeviceAcquisitionFailureReason? reason,
                                   IDictionary<uint, GroupAcquisitionStatus> statuses) : base(acquired, reason)
        {
            if(reason.HasValue && reason.Value == DeviceAcquisitionFailureReason.RequestQueued)
            {
                if(statuses == null || statuses.Count == 0)
                {
                    throw new ArgumentException("Group statuses list cannot be empty if request groups were queued.");
                }

                GroupAcquisitionStatuses = statuses;
            }
        }
    }
}