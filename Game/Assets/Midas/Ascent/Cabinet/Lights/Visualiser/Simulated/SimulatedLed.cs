using UnityEngine;
using Color = IGT.Game.Core.Presentation.PeripheralLights.Color;

namespace Midas.Ascent.Cabinet.Lights.Visualiser.Simulated
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class SimulatedLed : MonoBehaviour
	{
		#region Fields

		private SpriteRenderer spriteRenderer;

		#endregion

		#region Unity Methods

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		#endregion

		public void SetColor(Color color, byte intensity)
		{
			if ((color.R & color.G & color.B) != 1)
			{
				var r = color.R / 255f;
				var g = color.G / 255f;
				var b = color.B / 255f;

				var colour = new UnityEngine.Color(r, g, b, 1);
				var intensityValue = intensity / 255f;

				var colourIntensified = new UnityEngine.Color
				{
					r = colour.r * intensityValue,
					g = colour.g * intensityValue,
					b = colour.b * intensityValue,
					a = spriteRenderer.sharedMaterial.color.a
				};

				spriteRenderer.color = colourIntensified;
			}
		}
	}
}