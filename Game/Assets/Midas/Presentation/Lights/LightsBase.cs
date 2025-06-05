using UnityEngine;

namespace Midas.Presentation.Lights
{
	public abstract class LightsBase : ScriptableObject
	{
		[SerializeField]
		private int priority;

		[SerializeField]
		private Color fallbackColor;

		[SerializeField]
		private float lightIntensity = 1f;

		public string Name => name;
		public int Priority => priority;
		public Color FallbackColor => fallbackColor;
		public float LightIntensity => lightIntensity;

		public abstract LightsHandle Register();
	}
}