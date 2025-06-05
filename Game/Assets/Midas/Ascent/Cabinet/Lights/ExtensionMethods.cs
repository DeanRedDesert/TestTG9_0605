using UnityEngine;
using Color = IGT.Game.Core.Presentation.PeripheralLights.Color;
using UnityColor = UnityEngine.Color;

namespace Midas.Ascent.Cabinet.Lights
{
	public static class ExtensionMethods
	{
		public static Color ToPeripheralLightsColor(this UnityColor colour)
		{
			return Color.FromArgb(
				(byte)Mathf.RoundToInt(colour.a * 255f),
				(byte)Mathf.RoundToInt(colour.r * 255f),
				(byte)Mathf.RoundToInt(colour.g * 255f),
				(byte)Mathf.RoundToInt(colour.b * 255f));
		}
	}
}