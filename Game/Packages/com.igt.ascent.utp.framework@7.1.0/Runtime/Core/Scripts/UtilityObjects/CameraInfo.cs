// -----------------------------------------------------------------------
//  <copyright file = "CameraInfo.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable NotAccessedField.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Framework.UtilityObjects
{
    using IgtUnityEngine;
    using UnityEngine;

    /// <summary>
    /// Class for wrapping up information about cameras to be serialized and sent to UTP clients.
    /// </summary>
    public class CameraInfo
    {
        /// <summary>
        /// Full path of the camera in the hierarchy.
        /// </summary>
        public string FullPath;

        /// <summary>
        /// Which monitor the camera is associated to.
        /// </summary>
        public MonitorRole MonitorAssociation;

        /// <summary>
        /// Whether or not the camera is enabled.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Default constructor required for serialization.
        /// </summary>
        public CameraInfo()
        {

        }

        /// <summary>
        /// Constructor to simplify populating new instances.
        /// </summary>
        /// <param name="camera"></param>
        public CameraInfo(Camera camera)
        {
            FullPath = UtpModuleUtilities.GetObjectFullPath(camera.transform);
            MonitorAssociation = (MonitorRole)camera.targetDisplay;
            Enabled = camera.enabled;
        }
    }
}