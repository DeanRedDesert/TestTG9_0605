using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices;
using Midas.Presentation.DevHelpers.DebugWindows;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights.Visualiser
{
	public sealed class StandaloneStreamingLights : StandaloneDeviceBase<IStreamingLights>
	{
		private DebugWindowsEnabler debugWindowsEnabler;
		private LightsVisualiserWindow lightsVisualiserWindow;
		private int customButtonId;

		[SerializeField]
		private Sprite ledSprite;

		[SerializeField]
		private Vector2 ledSize;

		protected override object CreateVirtualImplementation()
		{
			var lightsGo = new GameObject("LightsVisualiser");
			lightsGo.transform.parent = gameObject.transform;
			lightsGo.transform.localPosition = new Vector3(-50f, -15, -1000);
			lightsGo.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

			debugWindowsEnabler = FindObjectOfType<DebugWindowsEnabler>(true);
			if (debugWindowsEnabler)
			{
				customButtonId = debugWindowsEnabler.AddCustomButtonFunction("Lights Visualiser", () =>
				{
					ShowLightsVisualiser();
					return "Lights Visualiser";
				});
			}

			var lightsVis = lightsGo.AddComponent<LightsVisualiserController>();
			lightsVis.Configure(ledSprite, ledSize);
			return lightsVis;
		}

		private void ShowLightsVisualiser()
		{
			if (!lightsVisualiserWindow)
				lightsVisualiserWindow = LightsVisualiserWindow.GetInstance();

			if (lightsVisualiserWindow)
			{
				lightsVisualiserWindow.gameObject.SetActive(true);
				lightsVisualiserWindow.enabled = true;
			}
		}

		private void OnDestroy()
		{
			if (debugWindowsEnabler)
				debugWindowsEnabler.RemoveCustomButtonFunction(customButtonId);
		}
	}
}