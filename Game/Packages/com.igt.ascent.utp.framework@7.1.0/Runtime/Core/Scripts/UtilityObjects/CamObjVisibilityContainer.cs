// -----------------------------------------------------------------------
//  <copyright file = "CamObjVisibilityContainer.cs" company = "IGT">
//      Copyright (c) 2017-2020 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Framework.UtilityObjects
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Framework;


    /// <summary>
    /// Class for containing all the information about a GameObj's visibility within a camera's view.
    /// </summary>
    public class CamObjVisibilityContainer
    {
        /// <summary>
        /// The full path of the camera in the hierarchy.
        /// </summary>
        public readonly string CameraPath;

        /// <summary>
        /// The full path of the GameObject in the hierarchy.
        /// </summary>
        public readonly string GameObjectPath;

        /// <summary>
        /// Simplified bool variance of VisibilityType used to represent whether the object is visible or not in the specified camera.
        /// </summary>
        public readonly bool IsVisible;

        /// <summary>
        /// A descriptive string about the visibilty state.  Mostly meant to be used for debugging.
        /// </summary>
        public readonly string VisibilityType;


        /// <summary>
        /// Various ways an object may appear (or not appear) in a cameras view.
        /// These are mostly meant to be used for debugging purposes.
        /// </summary>
        private enum ObjCamVisibilityType
        {
            Visible,
            VisibleNoBounds,
            HiddenNotInCameraView,
            HiddenCanvasRenderer,
            HiddenBlockedBySomething,
            //HiddenSpriteRendererDisabled,
            ErrorNoBoundsNoCanvasRenderer,
            ErrorNoTransform
        }

        /// <summary>
        /// This is a list of 'visible' visibility types.
        /// </summary>
        private readonly List<ObjCamVisibilityType> visibleTypes = new List<ObjCamVisibilityType> {ObjCamVisibilityType.Visible, ObjCamVisibilityType.VisibleNoBounds};


        /// <summary>
        /// Default empty constructor required for serialization.
        /// </summary>
        public CamObjVisibilityContainer()
        {

        }

        /// <summary>
        /// Constructor to simplify populating new instances.
        /// </summary>
        public CamObjVisibilityContainer(Camera camera, GameObject gameObj, bool includeBlockedByObject = false)
        {
            CameraPath = camera.transform == null ? camera.name : UtpModuleUtilities.GetObjectFullPath(camera.transform);
            GameObjectPath = gameObj.transform == null ? gameObj.name : UtpModuleUtilities.GetObjectFullPath(gameObj.transform);

            var visType = GetVisibilityType(camera, gameObj);
            VisibilityType = visType.ToString();
            IsVisible = visibleTypes.Contains(visType) || (includeBlockedByObject && visType.Equals(ObjCamVisibilityType.HiddenBlockedBySomething));
        }

        /// <summary>
        /// Gets the ObjCamVisibilityType for a given Camera/GameObject combo.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="gameObj">The game object</param>
        /// <returns>The ObjCamVisibilityType for the given Camera/GameObject combo.</returns>
        private static ObjCamVisibilityType GetVisibilityType(Camera camera, GameObject gameObj)
        {
            //  http://answers.unity3d.com/questions/8003/how-can-i-know-if-a-gameobject-is-seen-by-a-partic.html
            var bounds = UtpModuleUtilities.GetObjectBounds(gameObj, true);
            if (bounds == null)
            {
                //  Handle special case for CanvasRenderer gameobjects, which don't have bounds
                var canvasRenderer = gameObj.GetComponents<CanvasRenderer>().FirstOrDefault(cr => cr.gameObject.activeInHierarchy);
                if (canvasRenderer == null)
                {
                    return ObjCamVisibilityType.ErrorNoBoundsNoCanvasRenderer;
                }

                var point = camera.WorldToViewportPoint(gameObj.transform.position);
                if (point.x >= 0 && point.x <= 1.01F && point.y >= 0 && point.y <= 1.01F && point.z >= 0)
                {
                    return ObjCamVisibilityType.VisibleNoBounds;
                }

                return ObjCamVisibilityType.HiddenCanvasRenderer;
            }

            if (gameObj.transform == null)
                return ObjCamVisibilityType.ErrorNoTransform;

            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!GeometryUtility.TestPlanesAABB(planes, bounds.Value))
                return ObjCamVisibilityType.HiddenNotInCameraView;

            var fwd = gameObj.transform.TransformDirection(Vector3.forward);
            var ray = camera.ScreenPointToRay(fwd);
            if (Physics.Raycast(ray))
                return ObjCamVisibilityType.HiddenBlockedBySomething;

            //Commenting this out b/c it's probably not needed if 'true' is passed in to the above call to GetObjectBounds
            //if(gameObj.GetComponents<SpriteRenderer>().All(sr => !sr.enabled))
            //    return VisTypes.HiddenSpriteRendererDisabled;

            return ObjCamVisibilityType.Visible;
        }
    }
}