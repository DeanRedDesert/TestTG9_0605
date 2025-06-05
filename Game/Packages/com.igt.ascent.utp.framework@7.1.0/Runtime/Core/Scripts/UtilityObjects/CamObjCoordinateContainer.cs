// -----------------------------------------------------------------------
//  <copyright file = "CamObjCoordinateContainer.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Framework.UtilityObjects
{
    using UnityEngine;
    using Framework;


    /// <summary>
    /// Class for containing all the information about a GameObj's XY coordinates within a camera's view.
    /// </summary>
    public class CamObjCoordinateContainer
    {
        /// <summary>
        /// The full path of the camera in the hierarchy.
        /// </summary>
        public string CameraPath;

        /// <summary>
        /// The full path of the GameObject in the hierarchy.
        /// </summary>
        public string GameObjectPath;

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Default empty constructor required for serialization.
        /// </summary>
        public CamObjCoordinateContainer()
        {

        }

        /// <summary>
        /// Constructor to simplify populating new instances.
        /// </summary>
        public CamObjCoordinateContainer(Camera camera, GameObject gameObj)
        {
            CameraPath = camera.transform == null ? camera.name : UtpModuleUtilities.GetObjectFullPath(camera.transform);
            GameObjectPath = gameObj.transform == null ? gameObj.name : UtpModuleUtilities.GetObjectFullPath(gameObj.transform);

            var coordinates = UtpModuleUtilities.GetPadTouchCoordinates(camera, gameObj);
            X = coordinates.X;
            Y = coordinates.Y;
        }
    }

}
