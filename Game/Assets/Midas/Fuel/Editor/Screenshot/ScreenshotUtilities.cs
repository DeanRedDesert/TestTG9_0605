using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Midas.Core;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Midas.Fuel.Editor.Screenshot
{
	using Object = UnityEngine.Object;

	/// <summary>
	/// Helper methods for taking translated data screen shots.
	/// </summary>
	public static class ScreenshotUtilities
	{
		#region Constants

		/// <summary>
		/// Name of the folder to save screenshots to
		/// </summary>
		private const string ScreenshotFolder = "FuelTranslations";

		/// <summary>
		/// Format string to get the database storage path for an object.
		/// </summary>
		private const string SavePathFormat = "{0}\\{1}";

		/// <summary>
		/// Default distance to move the camera away from the object for the screen shot.
		/// </summary>
		public const float DefaultCameraDistance = 1.0f;

		/// <summary>
		/// The forced monitor height for screenshots.
		/// </summary>
		public const int ScreenshotMonitorHeight = 1080;

		/// <summary>
		/// The forced monitor width for screenshots.
		/// </summary>
		public const int ScreenshotMonitorWidth = 1920;

		/// <summary>
		/// The forced monitor aspect ration for screenshots.
		/// </summary>
		public const float ScreenshotMonitorAspect = (float)ScreenshotMonitorWidth / ScreenshotMonitorHeight;

		#endregion

		#region Public Properties

		/// <summary>
		/// Camera used to take screenshots.
		/// </summary>
		public static Camera ScreenshotCamera { get; set; }

		/// <summary>
		/// Name of the game currently being screenshot.
		/// </summary>
		public static string GameName { get; set; }

		/// <summary>
		/// The project path for the currently open project.
		/// </summary>
		private static string ProjectPath { get; set; }

		/// <summary>
		/// The directory where screen shots are stored for this project.
		/// </summary>
		private static string ScreenshotDirectory { get; set; }

		#endregion

		#region Construction

		/// <summary>
		/// Initialize this utility class.
		/// </summary>
		static ScreenshotUtilities()
		{
			var applicationPath = Application.dataPath;
			ProjectPath = applicationPath.Substring(0, applicationPath.IndexOf("Assets", StringComparison.Ordinal));

			ScreenshotDirectory = Path.Combine(ProjectPath, ScreenshotFolder);

			//Create the place where screenshots will be saved
			Directory.CreateDirectory(ScreenshotDirectory);
		}

		#endregion

		#region Render Screen Shot Methods

		/// <summary>
		/// Takes a screenshot of the passed game object and saves the image to disk.
		/// </summary>
		/// <param name="translationId">Translation ID for the translation being screenshot.</param>
		/// <returns>Database path for the saved image.</returns>
		public static string RenderGameObject(string translationId)
		{
			var imageName = GetImageName(translationId);

			var databasePath = GetDatabasePath(RenderCameraToImage(imageName));

			return databasePath;
		}

		/// <summary>
		/// Centers the camera on a game object and ensures it is within the viewing window
		/// </summary>
		/// <param name="objectToPositionOn">GameObject to center on</param>
		/// <param name="rotation">
		/// Specify a custom rotation for the screenshot camera to capture the translation.
		/// </param>
		/// <param name="distanceFromCenter">Distance to move the camera back from the object being screenshot.</param>
		public static void PositionCamera(GameObject objectToPositionOn, Quaternion rotation = new Quaternion(), float distanceFromCenter = DefaultCameraDistance)
		{
#if UNITY_EDITOR

			var camTransform = ScreenshotCamera.transform;

			// Find the canvas this is part of.  The canvas will be on one of the object's parents.
			var canvas = objectToPositionOn.GetComponentInParent<Canvas>();

			// Check if this is a Unity UI element.
			var objectIsInCanvas = canvas != null;

			if (objectIsInCanvas)
			{
				// Set the render mode to world space so that the canvas element can be zoomed in on.
				canvas.renderMode = RenderMode.WorldSpace;

				// Rotate the camera to match the canvas.  This makes sure the camera is aligned with it rather than the world
				// axis or whichever game object last used the camera.
				camTransform.rotation = canvas.transform.rotation;
			}
			else
			{
				// NOTE: Removing this for canvas rendering makes it easier to trust the RectTransform which appears to be relative to
				// the canvas.  This seems like a good choice as long as the canvas is meant to be used in RenderMode.ScreenSpaceCamera
				// or RenderMode.ScreenSpaceOverlay.  If the people choose to use RenderMode.WorldSpace (useful for diegetic interfaces, i.e.
				// interfaces that float in 3D space within the scene) we may need to revisit this.

				// Parent the camera to the object and adjust its settings to match.
				camTransform.parent = objectToPositionOn.transform;
				camTransform.localPosition = Vector3.zero;
				camTransform.localRotation = Quaternion.identity;
				camTransform.localScale = Vector3.one;
			}

			// Apply custom rotation.
			camTransform.localRotation *= rotation;

			// For now we assume that all screen shots should be taken at the main monitor size, which may not
			// work for games that have a portrait top box.
			var monitorSize = GetMonitorSize();
			var monitorHeight = monitorSize.y;

			// Default the size to half of the vertical resolution.
			ScreenshotCamera.orthographicSize = (int)(monitorHeight / 2);

			var contextObjects = GetContextGameObjects(objectToPositionOn).ToList();

			// Calculate the size of the objects that need to be in the screen shot.
			var bounds = GetScreenShotBounds(contextObjects);

			// Always re-position the camera for bounds because for a single text field with alignment
			// parenting and centering above will not fit the object to camera bounds properly
			PositionCameraForBounds(ScreenshotCamera, bounds);

			// Make the camera size fits that calculated size
			SizeCamerForBounds(ScreenshotCamera, bounds);

			// Move the camera back from the object.
			camTransform.Translate(0, 0, distanceFromCenter * -1, Space.Self);

#endif
		}

		/// <summary>
		/// Takes what is currently being displayed by a camera and creates an image from it.
		/// </summary>
		/// <param name="filename">Name of the file this PNG will be called</param>
		/// <returns>File name of the PNG image</returns>
		private static string RenderCameraToImage(string filename)
		{
			return RenderCameraToImage(ScreenshotCamera, filename);
		}

		/// <summary>
		/// Takes what is currently being displayed by a camera and creates a PNG from it
		/// </summary>
		/// <param name="cam">Camera to use to take screenshot with</param>
		/// <param name="filename">Name of the file this PNG will be called</param>
		/// <returns>File name of the PNG image</returns>
		private static string RenderCameraToImage(Camera cam, string filename)
		{
			var screenShot = CameraToTexture(cam);

			filename = RenderTextureToImage(screenShot, filename);

			Object.DestroyImmediate(screenShot);

			return filename;
		}

		/// <summary>
		/// Take what is currently being displayed by a camera and create an in memory texture from it.
		/// </summary>
		/// <param name="cam">Camera to use to take screenshot with</param>
		/// <returns>Text created from the camera</returns>
		private static Texture2D CameraToTexture(Camera cam)
		{
#if UNITY_EDITOR
			var monitorSize = GetMonitorSize();

			// Use full resolution screen captures.
			var textureHeight = (int)monitorSize.y;
			var textureWidth = (int)monitorSize.x;

			var texture = new RenderTexture(textureWidth, textureHeight, 24);
			cam.targetTexture = texture;

			var screenShot = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

			cam.Render();

			RenderTexture.active = texture;
			screenShot.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);

			cam.targetTexture = null;
			RenderTexture.active = null;

			Object.DestroyImmediate(texture);

			return screenShot;
#else
            return null;
#endif
		}

		/// <summary>
		/// Save the passed texture out to disk as an image file.
		/// </summary>
		/// <param name="texture">Texture to render</param>
		/// <param name="filename">Path to save to.</param>
		/// <returns>Final path to the rendered texture.</returns>
		private static string RenderTextureToImage(Texture2D texture, string filename)
		{
			var directory = Path.GetDirectoryName(filename);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var bytes = texture.EncodeToJPG();
			filename += ".jpg";
			File.WriteAllBytes(filename, bytes);

			return filename;
		}

		#endregion

		#region Asset Helpers

		/// <summary>
		/// Saves a copy of the given object to the translation directory
		/// </summary>
		/// <param name="obj">Object to copy/move</param>
		/// <param name="translationId">Unique ID to save this object as</param>
		/// <returns>File name of the saved file</returns>
		public static string SaveAsset(Object obj, string translationId)
		{
			// ReSharper disable once RedundantAssignment
			var newFileName = string.Empty;

#if UNITY_EDITOR

			var assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
			var fileName = Path.GetFileName(assetPath);

			newFileName = GetScreenshotFilePath(fileName, translationId);
			File.Copy(ProjectPath + assetPath, newFileName, true);
#endif

			return newFileName;
		}

		/// <summary>
		/// Generates the file path where a screenshot will be saved for this item
		/// </summary>
		/// <param name="fileName">The file name of the item.</param>
		/// <param name="translationId">The game client Id of the item.</param>
		/// <returns>File path for this items screenshot.</returns>
		private static string GetScreenshotFilePath(string fileName, string translationId)
		{
			// ReSharper disable once RedundantAssignment
			var newFilePath = string.Empty;

#if UNITY_EDITOR

			var newFileName = $"{translationId}_{LocalizationSettings.SelectedLocale.Identifier.Code}{Path.GetExtension(fileName)}";

			newFilePath = Path.Combine(ScreenshotDirectory, newFileName);

#endif

			return newFilePath;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Get the game objects that make up the context for the passed object.
		/// </summary>
		/// <param name="objectToRender">Object to find context objects for.</param>
		/// <returns>Collection of game objects found including the one passed.</returns>
		private static IEnumerable<GameObject> GetContextGameObjects(GameObject objectToRender)
		{
			var objects = new List<GameObject> { objectToRender };
			objects.AddRange(from component in objectToRender.GetComponents<ScreenshotContext>()
				where component != null
				from gameObject in component.ContextObjects
				where gameObject != null
				select gameObject);

			return objects.Distinct();
		}

		/// <summary>
		/// Enable a game object that should get a screenshot and any context game objects if needed.
		/// </summary>
		/// <param name="objectToRender">Game object to enable for screenshot.</param>
		public static void EnableScreenShotObject(GameObject objectToRender)
		{
			var contextObjects = GetContextGameObjects(objectToRender);

			// Enable the target object separately to ensure it is enabled in case it is not one of its own context objects.
			EnableScreenshotGameObject(objectToRender);

			// Enable all the all the objects within the context.  Omit the target object because it was directly handled.
			foreach (var contextObject in contextObjects.Where(obj => obj != objectToRender))
			{
				// Enable just the objects in the context, not their children.
				EnableScreenshotGameObject(contextObject);
			}
		}

		/// <summary>
		/// Backing Field for <see cref="ScreenshotContentEnablers"/>.
		/// </summary>
		private static List<IScreenshotContentEnabler> screenshotContentEnablers;

		/// <summary>
		/// Cache of objects that implement <see cref="IScreenshotContentEnabler"/>.
		/// </summary>
		/// <remarks>Cached because finding them is expensive.</remarks>
		private static IEnumerable<IScreenshotContentEnabler> ScreenshotContentEnablers
		{
			get
			{
				if (screenshotContentEnablers == null)
				{
					var foundTypes =
						from assemblyType in ReflectionUtil.GetAllTypes()
						// Get all the types that inherit form ContentTranslatorOld and can be instantiated (not abstract)
						where typeof(IScreenshotContentEnabler).IsAssignableFrom(assemblyType) && !assemblyType.IsAbstract
						// Get the types.
						select (IScreenshotContentEnabler)Activator.CreateInstance(assemblyType);

					screenshotContentEnablers = new List<IScreenshotContentEnabler>(foundTypes);
				}

				return screenshotContentEnablers;
			}
		}

		/// <summary>
		/// Disables an object and its components that should be shown in a screenshot.
		/// </summary>
		/// <param name="contentObj">Object to disable.</param>
		private static void DisableScreenshotContentObject(GameObject contentObj)
		{
			// Disable the object.
			contentObj.SetActive(false);

			// Disable all components that have an IScreenshotContentEnabler implemented.
			foreach (var contentEnabler in ScreenshotContentEnablers)
			{
				contentEnabler.Enable(contentObj, false);
			}
		}

		/// <summary>
		/// Disable a game object that should not appear in a screenshot.
		/// </summary>
		/// <param name="objectToDisable">Game object to disable for screenshot.</param>
		public static void DisableScreenShotObject(GameObject objectToDisable)
		{
			var root = objectToDisable.transform.root.gameObject;
			DisableScreenshotContentObject(root);
			root.PerformActionOnAllChildrenRecursive(DisableScreenshotContentObject);

			foreach (var contextObject in GetContextGameObjects(objectToDisable))
			{
				root = contextObject.transform.root.gameObject;
				DisableScreenshotContentObject(root);
				root.PerformActionOnAllChildrenRecursive(DisableScreenshotContentObject);
			}
		}

		/// <summary>
		/// Get the standard Fuel image name for rendered textures
		/// </summary>
		/// <param name="translationId">Translation Id for the object being rendered.</param>
		/// <returns>Standard Fuel image name path</returns>
		private static string GetImageName(string translationId)
		{
			return Path.Combine(ScreenshotFolder, $"{translationId}_{LocalizationSettings.SelectedLocale.Identifier.Code}");
		}

		/// <summary>
		/// Get the standard Fuel image path for the translation database.
		/// </summary>
		/// <param name="imagePath">Path to the image that was rendered to disk.</param>
		/// <returns>Standard Fuel database path.</returns>
		private static string GetDatabasePath(string imagePath)
		{
			// Handle Cases where the image is nested within many folders
			var databasePath = string.Empty;

			if (!string.IsNullOrEmpty(imagePath))
			{
				var imageName = Path.GetFileName(imagePath);
				databasePath = string.Format(SavePathFormat, GameName, imageName);
			}

			return databasePath;
		}

		/// <summary>
		/// Get the path for screenshot images stored local to the game based on their database path.
		/// </summary>
		/// <param name="databasePath">Standard fuel database path for the file to convert.</param>
		/// <returns>Path local to the game project for the image.</returns>
		public static string GetLocalPathFromDatabasePath(string databasePath)
		{
			var imageName = Path.GetFileName(databasePath);

			return Path.Combine(ScreenshotFolder, imageName);
		}

		/// <summary>
		/// Enables an object and its components that should be shown in a screenshot.
		/// </summary>
		/// <param name="contentObj">Object to enable.</param>
		private static void EnableScreenshotContentObject(GameObject contentObj)
		{
			// Enable the object.
			contentObj.SetActive(true);
			// Make sure this object and all of its children are not faded or invisible.
			contentObj.SetGameObjectAlpha(1.0f);

			// Enable all components that have an IScreenshotContentEnabler implemented.
			foreach (var screenshotContentEnabler in ScreenshotContentEnablers)
			{
				screenshotContentEnabler.Enable(contentObj, true);
			}
		}

		/// <summary>
		/// Enable a single game object for screenshot.
		/// </summary>
		/// <param name="gameObject">Game object to enable for screenshot.</param>
		private static void EnableScreenshotGameObject(GameObject gameObject)
		{
			// Enable this object.
			EnableScreenshotContentObject(gameObject);

			// Enable this object and all of its parents.
			var currObject = gameObject.transform.parent == null ? null : gameObject.transform.parent.gameObject;
			while (currObject != null)
			{
				// Make sure the object scale is not very small or zero.  If it is, the screenshot will be blank.
				if (currObject.transform.localScale.magnitude < .01)
				{
					currObject.transform.localScale = Vector3.one;
				}

				// Active the parent.
				currObject.SetActive(true);

				// Move up to the next parent.
				currObject = currObject.transform.parent == null ? null : currObject.transform.parent.gameObject;
			}
		}

		/// <summary>
		/// Recursively perform an action on all of this game object's children.
		/// </summary>
		/// <param name="gameObject">GameObject to perform the action on all children.</param>
		/// <param name="action">The action to invoke on each child.</param>
		private static void PerformActionOnAllChildrenRecursive(this GameObject gameObject, Action<GameObject> action)
		{
			foreach (Transform child in gameObject.transform)
			{
				action(child.gameObject);
				PerformActionOnAllChildrenRecursive(child.gameObject, action);
			}
		}

		/// <summary>
		/// Action delegate used by the PerformActionOnAllChildrenRecursive function.
		/// Set the alpha of the GameObject's sharedMaterial.
		/// </summary>
		/// <param name="gameObject">GameObject to set the alpha.</param>
		/// <param name="data">The alpha value.</param>
		private static void SetGameObjectAlpha(this GameObject gameObject, object data)
		{
			if (gameObject.GetComponent<Renderer>() != null &&
				gameObject.GetComponent<Renderer>().sharedMaterial != null)
			{
				if (gameObject.GetComponent<Renderer>().sharedMaterial.HasProperty("_Color"))
				{
					var alpha = (float)data;
					var color = gameObject.GetComponent<Renderer>().sharedMaterial.color;
					color.a = alpha;
					gameObject.GetComponent<Renderer>().sharedMaterial.color = color;
				}
			}
		}

		/// <summary>
		/// Converts the local extents to world space by applying the scale of the object an all of all its parents.
		/// </summary>
		/// <param name="screenshotObject">The object associated with the passed extents.</param>
		/// <param name="localExtents">The extents in object space.</param>
		private static void ApplyScale(GameObject screenshotObject, ref Vector3 localExtents)
		{
			// Apply the scales of the object and all parents to the extents so that the final value will be relative to the world.
			var transformToScale = screenshotObject.transform;
			while (transformToScale != null)
			{
				localExtents.Scale(transformToScale.transform.localScale);

				// Get the parent's transform.
				transformToScale = transformToScale.transform.parent;
			}
		}

		/// <summary>
		/// Return the bounds of a single game object for a screenshot.
		/// </summary>
		/// <param name="screenshotObject">Object to get bounds from.</param>
		/// <returns>Bounds to use for screenshot of passed object.</returns>
		private static Bounds GetScreenShotBounds(GameObject screenshotObject)
		{
			// Create new bounds that will be calculated for passed object.
			var currentObjectBounds = new Bounds();

			// Need to determine the extents. This can be pulled from different components.
			var currentObjectExtents = new Vector3();

			// Try to get a rect transform from the object.  This is the first place that will be checked for the bounds.
			var rectTransform = screenshotObject.GetComponent<RectTransform>();

			// Try to get a renderer from the object.  This will be the second place that will be checked for the bounds.
			var renderer = screenshotObject.GetComponent<Renderer>();

			// Try getting the bounds a RectTransform on the object.
			if (rectTransform != null)
			{
				// Copy the extents (distance from the edge to the center) from the RectTransform.  This does not get the center in world space.
				currentObjectExtents = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform).extents;

				// Calculate the center of the rect and use that as the center for the bounds.  This is needed because the center copied from the object
				// or reported by Transform.position will be the anchor point which may not be the center of the image.
				var centeredAnchor = new Vector2 { x = 0.5f, y = 0.5f };
				var anchorAdjust = centeredAnchor - rectTransform.pivot;
				var rectSizeVector = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
				var worldSpaceCenter = rectTransform.TransformPoint(Vector2.Scale(anchorAdjust, rectSizeVector));
				currentObjectBounds.center = worldSpaceCenter;

				// Apply scale to get the extents relative to the world.
				ApplyScale(screenshotObject, ref currentObjectExtents);
			}
			// Try getting the bounds from a renderer on the game object.
			else if (renderer != null)
			{
				// Copy the entire bounds (extents and center) from the renderer.
				currentObjectBounds = renderer.bounds;

				// Try to get the extents of the bounding box from the mesh of the object.  This size should be more reliable than
				// the one from the renderer for the screenshot because it will be object aligned rather than axis aligned.
				var aMeshFilter = screenshotObject.GetComponent<MeshFilter>();
				if (aMeshFilter != null && aMeshFilter.sharedMesh != null)
				{
					// Use the object aligned extents instead of the axis aligned ones which come from the renderer.
					currentObjectExtents = aMeshFilter.sharedMesh.bounds.extents;

					// Apply scale to get the extents relative to the world.
					ApplyScale(screenshotObject, ref currentObjectExtents);
				}
				else
				{
					// Copy the extents from the renderer if another option is not found.
					currentObjectExtents = renderer.bounds.extents;
				}
			}

			// Copy the object aligned extents.
			currentObjectBounds.extents = currentObjectExtents;

			if (!IsValidVector(currentObjectBounds))
			{
				Debug.LogError($"{screenshotObject.name} does not contain a valid Bounds.", screenshotObject);
			}

			return currentObjectBounds;
		}

		/// <summary>
		/// Given a collection of game object. Determine how big the group of them are and return it.
		/// </summary>
		/// <param name="objects">Collection of objects to size.</param>
		/// <returns>Bounds object that encapsulates all objects passed.</returns>
		private static Bounds GetScreenShotBounds(IEnumerable<GameObject> objects)
		{
			var typesToInclude = new List<Type> { typeof(Renderer), typeof(RectTransform) };
			var objectsToInclude = new List<GameObject>();
			var objectsList = objects.ToList();

			// Create a list of objects that need to be included in the bounds calculation from the passed collection.
			foreach (var type in typesToInclude)
			{
				objectsToInclude.AddRange(
					// ReSharper disable once AccessToForEachVariableInClosure
					objectsList.SelectMany(gameObject => gameObject.GetComponentsInChildren(type).Select(component => component.gameObject)));
			}

			// Get the bounds of all objects to include in the screenshot.
			var positionBounds = objectsToInclude.Distinct().Select(GetScreenShotBounds).ToList();

			// Create a bounds object that contains all bounds found. For things such as audio translators, there is no renderer--
			// and thus no bounds, so make sure to get a default in case positionBounds is empty.
			var bounds = positionBounds.FirstOrDefault();
			positionBounds.ForEach(objectBounds => bounds.Encapsulate(objectBounds));

			return bounds;
		}

		/// <summary>
		/// Determine whether the center and size Vectors contains invalid values (float.Nan)
		/// </summary>
		/// <param name="bound">a Bound object that we wish to test</param>
		/// <returns>True if there are no float.Nan in the center or size vectors</returns>
		private static bool IsValidVector(Bounds bound)
		{
			if (float.IsNaN(bound.center.x) || float.IsNaN(bound.center.y) || float.IsNaN(bound.center.z))
			{
				return false;
			}

			return !float.IsNaN(bound.size.x) && !float.IsNaN(bound.size.y) && !float.IsNaN(bound.size.z);
		}

		/// <summary>
		/// Position the camera to be at the center of a group of objects based on the bounding box passed.
		/// </summary>
		/// <param name="camera">Camera that should be sized.</param>
		/// <param name="bounds">Bounds object that will be used to determine position.</param>
		private static void PositionCameraForBounds(Component camera, Bounds bounds)
		{
			// There are cases where the bounds value we get is not valid. Just don't use it.
			if (float.IsNaN(bounds.center.x) || float.IsNaN(bounds.center.y) || float.IsNaN(bounds.center.z))
			{
				return;
			}

			// Move the camera to the center of the bounds.  X and Y are based on the passed bounds center because the center of the camera
			// corresponds to the center of the objects to render.  Z is based on the min because the camera needs to be positioned so that
			// all the objects to screenshot are inside the camera's frustum.  Min Z will mean the camera is far enough back to be looking
			// at all objects.
			camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z);
		}

		/// <summary>
		/// Get the size of the design monitor for this game. Accounts for MLD.
		/// </summary>
		/// <remarks>
		/// If the design monitor is portrait, return landscape.
		/// It turns out the most content on a portrait screen should be captured with a wide screen screenshot
		/// for best results. The content that doesn't require a wide screen capture still looks good enough for
		/// the translators to work in the wide screen capture.
		/// </remarks>
		/// <returns>Vector with width as x and height as y.</returns>
		private static Vector2 GetMonitorSize()
		{
			float monitorHeight = ScreenshotMonitorHeight;
			float monitorWidth = ScreenshotMonitorWidth;

			return monitorHeight > monitorWidth
				? new Vector2(monitorHeight, monitorWidth)
				: new Vector2(monitorWidth, monitorHeight);
		}

		/// <summary>
		/// Resize a camera to fit a group of objects based on the bounding box passed.
		/// </summary>
		/// <param name="camera">Camera that should be sized.</param>
		/// <param name="bounds">Bounds object that will be used to determine size.</param>
		private static void SizeCamerForBounds(Camera camera, Bounds bounds)
		{
			var monitorSize = GetMonitorSize();

			var monitorHeight = monitorSize.y;
			var monitorWidth = monitorSize.x;

			var gameAspect = monitorWidth / monitorHeight;

			// Find the camera's size
			var height = bounds.size.y;
			var width = bounds.size.x;

			float vertSize;
			float horizSize;

			if (monitorHeight > monitorWidth)
			{
				// If this is a portrait monitor, the width is more important.
				// The width is more important because it is in short supply.
				// Set the width and calculate vertical size based on aspect.
				horizSize = width;
				// Divide aspect here because in portrait the aspect is less than 1, and we want the number to grow.
				vertSize = horizSize / gameAspect;
			}
			else
			{
				// If this is a landscape monitor, the height is more important.
				// Start by setting the size to the object's height and calculate width based on aspect.
				vertSize = height / 2;
				horizSize = vertSize * gameAspect;
			}

			// With that setting, is there enough horizontal space to display the whole object?
			var halfScreenWidth = width / 2;
			if (horizSize < halfScreenWidth)
			{
				// If not adjust the vertical size to meet the horizontal requirements.
				vertSize = halfScreenWidth / gameAspect;
			}

			// Size the camera to match what was calculated.
			// Increase the size by 10% so that the objects aren't touching the edge of the screenshot.
			camera.orthographicSize = vertSize + (float)(vertSize * 0.10);
		}

		#endregion
	}
}