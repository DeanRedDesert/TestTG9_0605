// -----------------------------------------------------------------------
//  <copyright file = "UtpModuleUtilities.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using UtilityObjects;

    /// <summary>
    /// Static helper class for test modules.
    /// </summary>
    public static class UtpModuleUtilities
    {
        #region Enums

        /// <summary>
        /// An object position type
        /// Screen - The position as viewed by the full game screen
        /// Camera - The position as viewed by a particular camera
        /// </summary>
        public enum ObjectPositionType
        {
            Screen,
            Camera
        }

        #endregion

        #region Fields

        /// <summary>
        /// For UTP Beta release, sending of images will be disabled by default.  This is to avoid generating exceptions caused by calling Texture2D.EncodeToPng()
        /// on an image that is not Read/Write Enabled or has an invalid format.  If images are modified for a game to have the proper import settings, then this
        /// flag can be set to true to enable sending of images.
        /// </summary>
        private static bool imageSendingEnabled = true;

        #endregion Fields

        #region Object helpers

        /// <summary>
        /// Gets a list of all child components, including inactives, from the parent Transform that are the specified type.
        /// This is somewhat of a workaround since Unity doesn't currently have a simpler way to look for inactive objects.
        /// </summary>
        /// <param name="parentTransform">The parent Transform object that the component belongs to.</param>
        /// <returns>A list of components, including inactives, of the matching type.</returns>
        public static List<T> FindObjectsWithInactives<T>(Transform parentTransform)
        {
            /*
             * One example of where this method may be needed is the DPP. The problem is that when UTP initializes, the
             * Help Window (type HelpWindowBehavior) is not displayed, so its .active property is false.  When 'active'
             * is false, FindObjectsOfType and GetComponents don't return the object even if the type matches.  This
             * method does a search of all child components for the specified parentTransform and will find components
             * even if they're inactive.
             * 
             * For example, the following line won't return an object unless the DPP help menu is current being displayed:
             * var sdkHelpWindows = IgniteDppButtonController.Instance.transform.parent.GetComponentsInChildren(typeof(HelpWindowBehavior)).Cast<HelpWindowBehavior>().ToList();
             * 
             * But the following line will find the correct reference:
             * var sdkHelpWindows = FindObjectWithInactives<HelpWindowBehavior>(IgniteDppButtonController.Instance.transform.parent);
             */
            var matchingTypes = new List<T>();

            for(var i = 0; i < parentTransform.childCount; i++)
            {
                matchingTypes.AddRange(parentTransform.transform.GetChild(i).GetComponents(typeof(T)).Cast<T>());
            }

            return matchingTypes;
        }

        /// <summary>
        /// Gets a list of all objects within the hierarchy, including inactives, of the specified type.
        /// </summary>
        /// <param name="onlyRoot">If true, will only return objects in the root of the scene.</param>
        /// <returns>A list of objects, including inactives, of the matching type.</returns>
        public static IEnumerable<T> FindObjectsWithInactives<T>(bool onlyRoot) where T : MonoBehaviour
        {
            var allObjects = Resources.FindObjectsOfTypeAll(typeof(T)).Cast<T>();
            var returnList = new List<T>();

            // ReSharper disable once LoopCanBeConvertedToQuery (too complex for me to understand as a linq query)
            foreach(var pObject in allObjects)
            {
                if(onlyRoot && pObject.transform.parent != null)
                {
                    continue;
                }

                if(pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                returnList.Add(pObject);
            }

            return returnList;
        }

        /// <summary>
        /// Warning: Use this method only when necessary. It loops through game objects in the scene and could
        /// potentially cause performance issues if overused.
        /// 
        /// Accepts a list of object paths and returns all objects from the scene that have the same path.
        /// Typically, the return value is used to ensure there is exactly 1 object for a specified path
        /// for error checking purposes.  The parameter is a collection of object paths to help make
        /// this method more efficient.
        /// </summary>
        /// <typeparam name="T">The type of object to search for.</typeparam>
        /// <param name="objectPaths">A collection of object paths to search for. Use a HashSet to ensure no duplicates are in the collection.</param>
        /// <returns>A Dictionary(string, List(T)) which contains each specified path and a corresponding set of objects.</returns>
        public static Dictionary<string, List<T>> GetObjectsByPath<T>(HashSet<string> objectPaths)
            where T : MonoBehaviour
        {
            var pathGameObjDictionary = objectPaths.ToDictionary(objPath => objPath, objPath => new List<T>());

            foreach(var gameObj in Object.FindObjectsOfType(typeof(T)).Cast<T>())
            {
                var gameObjPath = GetObjectFullPath(gameObj.transform).ToLower().Trim();

                foreach(var path in objectPaths.Where(path => gameObjPath == path.ToLower().Trim()))
                {
                    pathGameObjDictionary[path].Add(gameObj);
                }
            }

            return pathGameObjDictionary;
        }

        /// <summary>
        /// Gets the full path of the object within the hierarchy.  This value can be used with GameObject.Find(path).
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The full path of the object within the hierarchy.</returns>
        public static string GetObjectFullPath(Transform transform)
        {
            if(transform.parent == null)
            {
                return "/" + transform.name;
            }

            return GetObjectFullPath(transform.parent) + "/" + transform.name;
        }

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field != null ? field.GetValue(instance) : null;
        }

        /// <summary>
        /// Uses reflection to get a property value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="propertyName">The properties name which is to be fetched.</param>
        /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
        /// <returns>The property value from the object.</returns>
        public static object GetInstanceProperty(Type type, object instance, string propertyName, object[] index = null)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            PropertyInfo property = type.GetProperty(propertyName, bindFlags);
            return property != null ? property.GetValue(instance, index) : null;
        }

        /// <summary>
        /// Checks if an object is visible in any of the cameras specified
        /// </summary>
        /// <param name="obj">The game object</param>
        /// <param name="ignoreBlockedByObject">If the target object has another object between it and the camera, include as visible</param>
        /// <returns>If the object is visible in any of the cameras</returns>
        public static bool IsObjectVisible(GameObject obj, bool ignoreBlockedByObject = false)
        {
            var visibleIn = GetCamerasVisibleIn(obj, ignoreBlockedByObject);
            if(visibleIn.Any())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of cameras a gameobject is visible in
        /// </summary>
        /// <param name="obj">The object to look for</param>
        /// <param name="includeBlockedByObject">Include cameras that have another object in front of the target</param>
        /// <returns>Cameras the object is visible in</returns>
        public static IEnumerable<Camera> GetCamerasVisibleIn(GameObject obj, bool includeBlockedByObject = false)
        {
            return Camera.allCameras.Where(cam => new CamObjVisibilityContainer(cam, obj, includeBlockedByObject).IsVisible);
        }

        /// <summary>
        /// Gets the bounds of a gameobject
        /// 
        /// This will first check for a renderer, if one isn't present it'll try to get the bounds from a collider
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="enabledCheck">Only get bounds if the renderer or collider is enabled</param>
        /// <returns>The object's bounds</returns>
        public static Bounds? GetObjectBounds(GameObject obj, bool enabledCheck = false)
        {
            if(obj != null)
            {
                var renderer = obj.GetComponents<Renderer>().FirstOrDefault(c => (!enabledCheck || c.enabled));
                if(renderer != null && (!enabledCheck || renderer.enabled))
                {
                    return renderer.bounds;
                }

                var collider = obj.GetComponents<Collider>().FirstOrDefault(c => (!enabledCheck || c.enabled));
                if(collider != null && (!enabledCheck || collider.enabled))
                {
                    return collider.bounds;
                }

                var childRenderer =
                    obj.GetComponentsInChildren<Renderer>().FirstOrDefault(c => (!enabledCheck || c.enabled));
                if(childRenderer != null && (!enabledCheck || childRenderer.enabled))
                {
                    return childRenderer.bounds;
                }

                var childCollider =
                    obj.GetComponentsInChildren<Collider>().FirstOrDefault(c => (!enabledCheck || c.enabled));
                if(childCollider != null && (!enabledCheck || childCollider.enabled))
                {
                    return childCollider.bounds;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the position of an object
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="camera">The camera to locate the object with</param>
        /// <param name="positionReference">What type of position to find</param>
        /// <param name="enabledCheck">Check for the object's renderer and collider when finding bounds</param>
        /// <returns>The position. [0,0] being the top left</returns>
        public static Rect? GetObjectPosition(GameObject obj, Camera camera, ObjectPositionType positionReference,
            bool enabledCheck = false)
        {
            var boundsCheck = GetObjectBounds(obj, enabledCheck);
            if(!boundsCheck.HasValue)
            {
                return null;
            }

            Bounds bounds = boundsCheck.GetValueOrDefault();

            // Get mesh origin and farthest extent (this works best with simple convex meshes)
            Vector3 origin = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
            Vector3 extent = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

            var width = extent.x - origin.x;
            var height = origin.y - extent.y;

            float y;
            if(positionReference == ObjectPositionType.Screen)
            {
                y = Screen.height - origin.y;
            }
            else if(positionReference == ObjectPositionType.Camera)
            {
                y = camera.pixelHeight - (origin.y - camera.pixelRect.y);
            }
            else
            {
                y = origin.y;
            }

            // Create rect in screen space and return - does not account for camera perspective
            return new Rect(origin.x, y, width, height);
        }

        /// <summary>
        /// Gets an image of a game object if visible
        /// </summary>
        /// <param name="gameObject">The game object to look for</param>
        /// <param name="autoScale">Attempt to scale down to fit in the camera</param>
        /// <param name="camera">Pass in a known camera if we already know</param>
        /// <returns>
        /// A PNG data string of a camera's view of that game object.
        /// Returns null if the object can't be found in a camera or if no bounds can be found for it
        /// </returns>
        public static string GetSceneObjectPicture(GameObject gameObject, Camera camera = null, bool autoScale = false)
        {
            if(camera == null)
            {
                camera = GetCamerasVisibleIn(gameObject).FirstOrDefault();
                if(camera == null)
                {
                    return null;
                }
            }

            var cameraPosition = GetObjectPosition(gameObject, camera, ObjectPositionType.Camera, true);
            if(!cameraPosition.HasValue)
            {
                return null;
            }

            var position = cameraPosition.Value;
            var camWidth = (int)camera.pixelWidth;
            var camHeight = (int)camera.pixelHeight;

            // Store original values
            var origTargetTexture = camera.targetTexture;
            var origClearFlags = camera.clearFlags;
            var origActiveTexture = RenderTexture.active;

            // Set a solid clear flag so non-cleared ones will always be cleared
            camera.clearFlags = CameraClearFlags.SolidColor;

            // Scale the image down
            if(autoScale)
            {
                int attempts = 0;
                while(attempts++ < 20 && (position.width > camera.pixelWidth || position.height > camera.pixelHeight))
                {
                    // Scale the camera down
                    camera.orthographicSize += 0.5f;
                    cameraPosition = GetObjectPosition(gameObject, camera, ObjectPositionType.Camera, true);
                    if(cameraPosition.HasValue)
                    {
                        position = cameraPosition.Value;
                    }
                }
            }

            // Crop the image if it goes out of the camera's range
            var texWidth = position.x + position.width <= camera.pixelWidth
                ? position.width
                : camera.pixelWidth - position.x - position.width;
            var texHeight = position.y + position.height <= camera.pixelHeight
                ? position.height
                : position.height + (camera.pixelHeight - position.y - position.height);

            // Render the object within the camera to a texture
            var texture2D = new Texture2D((int)Math.Abs(texWidth), (int)Math.Abs(texHeight));
            var renderTexture = new RenderTexture(camWidth, camHeight, 24);
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;
            
            texture2D.ReadPixels(position, 0, 0);
            texture2D.Apply();

            // Reset back to original values
            camera.targetTexture = origTargetTexture;
            camera.clearFlags = origClearFlags;
            RenderTexture.active = origActiveTexture;

            return GetImageBytesAsString(texture2D);
        }

        #endregion

        #region Module processing

        /// <summary>
        /// Gets a list of available commands in the module
        /// </summary>
        /// <param name="module">The module type</param>
        /// <returns>List of ModuleCommands</returns>
        public static List<ModuleCommand> GetModuleCommands(Type module)
        {
            var commands = new List<ModuleCommand>();

            // get custom attributes on methods
            var moduleList = module.GetMethods().Where(
                m => m.GetCustomAttributes(true).
                    Any(a => a.GetType() == typeof(ModuleCommand)));

            moduleList.SelectMany(
                m => m.GetCustomAttributes(true).Where(
                    a => a.GetType() == typeof(ModuleCommand)))
                .ToList()
                .ForEach(c => commands.Add(c as ModuleCommand));

            return commands;
        }

        /// <summary>
        /// Gets a list of available events in the module
        /// </summary>
        /// <param name="module">The module type</param>
        /// <returns>List of ModuleEvents</returns>
        public static List<ModuleEvent> GetModuleEvents(Type module)
        {
            var events = new List<ModuleEvent>();

            var eventDefs = module.GetCustomAttributes(true).Where(
                a => a.GetType() == typeof(ModuleEvent));

            eventDefs.ToList().ForEach(e => events.Add(e as ModuleEvent));

            return events;
        }

        #endregion

        #region General utilities

        /// <summary>
        /// This method converts the Unity environment coordinates into PAD-friendly coordinates for the specified GameObject.
        /// </summary>
        /// <param name="camera">The camera used by the game object.</param>
        /// <param name="gameObject">The game object to get the coordinates for.</param>
        /// <returns>PadTouchCoordinates for the specified game object.</returns>
        public static PadTouchCoordinates GetPadTouchCoordinates(Camera camera, GameObject gameObject)
        {
            //  The Unity cameras WorldToViewportPoint() method returns the objects coordinates from where the object
            //  appears on a grid where 0,0 is the bottom left corner of the screen.  We need to convert these
            //  coordinates to a grid where 0,0 is in the middle of the screen to be PAD-friendly.

            //  Get the Unity coordinates
            var unityCoordinates = camera.WorldToViewportPoint(gameObject.transform.position);

            //  Transform them to PAD coordinates
            var x = unityCoordinates.x + (unityCoordinates.x - 1);
            var y = unityCoordinates.y + (unityCoordinates.y - 1);

            return new PadTouchCoordinates(x, y);
        }

        /// <summary>
        /// Gets the EncodeToPNG() byte array for the specified texture.  Null is returned if the texture is not readable.
        /// If an exception occurs, this method will disable all future attempts to read images to prevent additional exceptions.
        /// </summary>
        public static string GetImageBytesAsString(Texture2D texture2D)
        {
            //  The following exceptions/errors occur when EncodeToPNG() is called on an image that doesn't have "Read/Write Enabled" checked or has an incompatible format:
            //   - UnityEngine.UnityException: Texture 'FullStackRetriggers' is not readable, the texture memory can not be accessed from scripts. You can make the texture readable in the Texture Import Settings.
            //   - Unsupported image format - the texture needs to be ARGB32 or RGB24

            byte[] bytes = null;

            try
            {
                if(imageSendingEnabled &&
                   (texture2D.format == TextureFormat.ARGB32 || texture2D.format == TextureFormat.RGB24))
                {
                    bytes = texture2D.EncodeToPNG();
                }
            }
            catch(Exception ex)
            {
                Debug.Log("UTP: Exception generated while attempting to get image bytes. Sending of images is now disabled. Error: " + ex);
                imageSendingEnabled = false;
            }

            if(bytes == null)
            {
                return null;
            }

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Gets a Color value from a string
        /// 
        /// Choose between black, blue, cyan, gray, green, grey, magenta, red, white, yellow, orange, or a custom color
        /// 
        /// Custom color strings should be formatted as: (R, B, G, A) where A is optional
        /// RGBA values should range from 0f to 1f
        /// </summary>
        /// <param name="colorValue">The color string</param>
        /// <param name="defaultColor">The default color if no match was found</param>
        /// <returns>The color based on the string</returns>
        public static Color GetColorFromString(string colorValue, Color defaultColor)
        {
            if(!string.IsNullOrEmpty(colorValue))
            {
                switch(colorValue.ToLower())
                {
                    case "black":
                        return Color.black;
                    case "blue":
                        return Color.blue;
                    case "cyan":
                        return Color.cyan;
                    case "gray":
                        return Color.gray;
                    case "green":
                        return Color.green;
                    case "grey":
                        return Color.grey;
                    case "magenta":
                        return Color.magenta;
                    case "red":
                        return Color.red;
                    case "white":
                        return Color.white;
                    case "yellow":
                        return Color.yellow;
                }

                // Also match '(R, G, B, A)' colors
                var rgbRegex = new Regex(@"\((?<R>[\d.]+),\s?(?<G>[\d.]+),\s?(?<B>[\d.]+)(?:,\s?(?<A>[\d.]+))?\)");
                var match = rgbRegex.Match(colorValue);
                if(match.Success)
                {
                    // ReSharper disable InconsistentNaming
                    float R, G, B, A;
                    // ReSharper restore InconsistentNaming
                    if(!float.TryParse(match.Groups["R"].Value, out R))
                    {
                        R = 0;
                    }
                    if(!float.TryParse(match.Groups["G"].Value, out G))
                    {
                        G = 0;
                    }
                    if(!float.TryParse(match.Groups["B"].Value, out B))
                    {
                        B = 0;
                    }
                    if(!float.TryParse(match.Groups["A"].Value, out A))
                    {
                        A = 1;
                    }

                    return new Color(R, G, B, A);
                }
            }
            return defaultColor;
        }

        #endregion
    }
}