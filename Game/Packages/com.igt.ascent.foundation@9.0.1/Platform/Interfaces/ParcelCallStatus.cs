//-----------------------------------------------------------------------
// <copyright file = "ParcelCallStatus.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// The enumeration defines the status of a parcel call.
    /// </summary>
    public enum ParcelCallStatus
    {
        /// <summary>
        /// The parcel call was accepted and a reply was returned from the target.
        /// </summary>
        Success,

        /// <summary>
        /// The parcel call was not accepted because the target's channel was in use.
        /// </summary>
        Busy,

        /// <summary>
        /// The parcel call was not accepted because the target could not be found or was not ready to accept messages.
        /// </summary>
        Unavailable,

        /// <summary>
        /// The parcel call was not accepted because the target rejected the message.
        /// </summary>
        Rejected
    }
}
