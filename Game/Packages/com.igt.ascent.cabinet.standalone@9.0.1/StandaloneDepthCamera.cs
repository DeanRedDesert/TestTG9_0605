//-----------------------------------------------------------------------
// <copyright file = "StandaloneDepthCamera.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Provide a virtual implementation of the depth camera category.
    /// </summary>
    internal class StandaloneDepthCamera : IDepthCamera
    {
        /// <summary>
        /// List of depth cameras provided for standalone.
        /// </summary>
        private readonly List<DepthCameraDevice> currentCameras = new List<DepthCameraDevice>
        {
            new DepthCameraDevice
            {
                DepthCameraType = CameraTechType.TimeOfFlight,
                DepthCameraDeviceIndex = DepthCameraDeviceIndex.First,
                DepthCameraModel = DepthCameraModel.PMDPicoFlex,
                DepthCameraUsbLocationPath = "connection",
                DepthCameraDeviceId = DepthCameraModel.PMDPicoFlex + " " + DepthCameraDeviceIndex.First
            },

            new DepthCameraDevice
            {
                DepthCameraType = CameraTechType.TimeOfFlight,
                DepthCameraDeviceIndex = DepthCameraDeviceIndex.Second,
                DepthCameraModel = DepthCameraModel.PMDPicoFlex,
                DepthCameraUsbLocationPath = "connection",
                DepthCameraDeviceId = DepthCameraModel.PMDPicoFlex + " " + DepthCameraDeviceIndex.Second
            }
        };

        #region IDepthCamera Implementation

        /// <inheritdoc/>
        public IEnumerable<DepthCameraDevice> GetAvailableDepthCameras()
        {
            return currentCameras;
        }

        #endregion
    }
}
