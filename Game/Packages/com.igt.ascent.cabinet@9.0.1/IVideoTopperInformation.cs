//-----------------------------------------------------------------------
// <copyright file = "IVideoTopperInformation.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface for query the video topper information without having to acquire
    /// the video topper device first.
    /// </summary>
    public interface IVideoTopperInformation
    {
        /// <summary>
        /// Gets the video topper device's media capabilities.
        /// </summary>
        /// <returns>Capability response from the foundation.</returns>
        /// <remarks>
        /// The returned value is undefined if the device is not connected at the moment.
        /// The user should re-get the capabilities when a device is connected.
        /// </remarks>
        VideoTopperCapabilities GetDeviceCapabilities();
    }
}
