// Copyright (C) , IGT Australia Pty. Ltd.

using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IGT.Game.SDKAssets.AscentBuildSettings;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights.Visualiser
{
	/// <summary>
	/// A GUI window for visualising the EGM lights.
	/// </summary>
	public sealed class LightsVisualiserWindow : MonoBehaviour
	{
		#region Fields

		private static LightsVisualiserWindow instance;
		private bool isQuitting;
		private Rect clientRect = Rect.zero;
		private LightsVisualiserController lightsController;

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets or creates the lights visualiser instance.
		/// </summary>
		/// <returns>The lights visualiser window instance.</returns>
		public static LightsVisualiserWindow GetInstance()
		{
			// First check if we already found the instance or there is one in the scene somewhere.

			if (instance || (instance = GetAllComponentsInScene<LightsVisualiserWindow>().FirstOrDefault()))
				return instance;

			// Try creating one.

			var go = new GameObject("LightsVisualiser");
			DontDestroyOnLoad(go);
			return instance = go.AddComponent<LightsVisualiserWindow>();
		}

		#endregion

		#region Unity Hooks

		private void OnGUI()
		{
			clientRect = GUI.Window(0, clientRect, DrawLightsWindow, "Lights Visualiser");
		}

		private void OnEnable()
		{
			if (clientRect == Rect.zero)
			{
				var machineBuild = IsMachineBuild();

				clientRect = machineBuild
					? new Rect(15, 15, 240, 320)
					: new Rect(15, 15, 500, 550);
			}

			if (lightsController)
				lightsController.Enable(true);
		}

		private void OnDisable()
		{
			if (isQuitting)
				return;
			if (lightsController)
				lightsController.Enable(false);
		}

		private void OnApplicationQuit()
		{
			isQuitting = true;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets all components in the scene including inactive, excluding prefabs.
		/// </summary>
		private static IReadOnlyList<T> GetAllComponentsInScene<T>() where T : Component
		{
			var components = new List<T>();
			if (!(Resources.FindObjectsOfTypeAll(typeof(T)) is T[] objects))
				return null;

			components.AddRange(objects);

#if UNITY_EDITOR
			components = components.Where(component => !UnityEditor.AssetDatabase.Contains(component)).ToList();
#endif

			return components;
		}

		private void DrawLightsWindow(int id)
		{
			var machineBuild = IsMachineBuild();

			if (!lightsController)
			{
				if (!(lightsController = FindObjectOfType<LightsVisualiserController>()))
					return;

				lightsController.Enable(true);
			}

			using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(5);

				using (new GUILayout.VerticalScope(GUILayout.Width(150), GUILayout.ExpandHeight(true)))
				{
					GUILayout.Space(5);

					if (!machineBuild)
					{
						using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(50)))
						{
							if (GUILayout.Button("Prev Cabinet", GUILayout.Height(40)))
								lightsController.SimulatePrevCabinet();
							if (GUILayout.Button("Next Cabinet", GUILayout.Height(40)))
								lightsController.SimulateNextCabinet();
						}

						GUILayout.Space(5);

						using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(50)))
						{
							if (GUILayout.Button("Prev Sequence", GUILayout.Height(40)))
								lightsController.SimulatePrevSequence();
							if (GUILayout.Button("Next Sequence", GUILayout.Height(40)))
								lightsController.SimulateNextSequence();
							if (GUILayout.Button("Reset Sequence", GUILayout.Height(40)))
								lightsController.StopSimulatingSequence();
						}

						GUILayout.Space(5);
						GUILayout.Label(lightsController.CurrentData.GetDescription());
						GUILayout.Space(5);
						GUILayout.Label("Sequence: " + (LightsVisualiserController.CurrentLights != null ? LightsVisualiserController.CurrentLights.Name : "none"));
						GUILayout.Space(5);
					}
					else
					{
						var cabinetData = LightsVisualiserController.CabinetData.FirstOrDefault(d => d.CabinetType == CabinetTypeIdentifier.GetCabinetType());

						if (cabinetData == null)
						{
							GUILayout.Label("<b>Unable to detect cabinet.</b>");
						}

						using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(50)))
						{
							if (GUILayout.Button("Prev Sequence", GUILayout.Height(40)))
								lightsController.SimulatePrevSequence();
							if (GUILayout.Button("Next Sequence", GUILayout.Height(40)))
								lightsController.SimulateNextSequence();
							if (GUILayout.Button("Reset Sequence", GUILayout.Height(40)))
								lightsController.StopSimulatingSequence();
						}

						GUILayout.Space(5);
						GUILayout.Label(cabinetData?.GetDescription() ?? "Unknown Cabinet");
						GUILayout.Space(5);
						GUILayout.Label("Sequence: " + (LightsVisualiserController.CurrentLights != null ? LightsVisualiserController.CurrentLights.Name : "none"));
						GUILayout.Space(5);
					}

					if (GUILayout.Button("Close", GUILayout.Height(40), GUILayout.ExpandWidth(true)))
						enabled = false;
				}

				if (!machineBuild)
				{
					GUILayout.Space(5);

					using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
					{
						using (new GUILayout.VerticalScope(lightsController.GetRenderTexture(), GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
						{
						}
					}
				}

				GUILayout.Space(5);
			}

			GUI.DragWindow();
		}

		private static bool IsMachineBuild()
		{
			return AscentFoundation.GameParameters.Type == IgtGameParameters.GameType.Standard ||
				AscentFoundation.GameParameters.Type == IgtGameParameters.GameType.UniversalController;
		}

		#endregion
	}
}