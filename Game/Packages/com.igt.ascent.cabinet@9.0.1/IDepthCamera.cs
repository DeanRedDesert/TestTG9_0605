//-----------------------------------------------------------------------
// <copyright file = "IDepthCamera.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Interface for the depth camera category of the CSI.
    /// </summary>
    public interface IDepthCamera
    {
        /// <summary>
        /// Get the list of depth cameras available to be acquired through the CSI.
        /// </summary>
        /// <returns>
        /// Configuration information including all present depth cameras.
        /// </returns>
        IEnumerable<DepthCameraDevice> GetAvailableDepthCameras();
    }
}
