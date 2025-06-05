//-----------------------------------------------------------------------
// <copyright file = "GroupAcquisitionStatus.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration which indicates the group acquisition status.
    /// </summary>
    public enum GroupAcquisitionStatus
    {
        /// <summary>
        /// A higher priority client has control of the device. The request has been queued and when no higher priority
        /// clients desire the group control will be given to this client.
        /// </summary>
        RequestQueued,

        /// <summary>
        /// The group was acquired.
        /// </summary>
        Acquired
    }
}