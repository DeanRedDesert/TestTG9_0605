using System;
using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Marching Ant Lights")]
	public sealed class MarchingAntLights : RuntimeLights
	{
		[SerializeField]
		[Tooltip("How long (in seconds) to play this stage light when it is activated.")]
		private float duration = 5;

		[SerializeField]
		[Tooltip("Do you want to have a solid background?")]
		private bool useBackgroundColour;

		[SerializeField]
		[Tooltip("The colour to use for a solid background (if any).")]
		private Color backgroundColour = Color.white;

		[SerializeField]
		[Tooltip("The colour to use for the ant.")]
		private Color antColour = Color.white;

		[SerializeField]
		[Tooltip("The number of ants.")]
		private uint antCount = 4;

		[SerializeField]
		[Tooltip("The size of the ants.")]
		private float antScale = 0.137f;

		public override TimeSpan Duration => TimeSpan.FromSeconds(duration);

		public override void CreateSequence(IRuntimeLightsFactory factory)
		{
			var bgColour = useBackgroundColour ? (Color?)backgroundColour : null;
			var ledCount = factory.LightCount;
			var ledStep = (float)ledCount / antCount;
			var frameCount = Mathf.RoundToInt(ledStep);
			var antLength = Mathf.Max(Mathf.RoundToInt(ledStep * antScale), 1);
			var frameDisplayTime = TimeSpan.FromSeconds(duration / frameCount);
			if (frameDisplayTime <= TimeSpan.Zero || frameDisplayTime > TimeSpan.FromMilliseconds(33))
				frameDisplayTime = TimeSpan.FromMilliseconds(33);

			// Detect if the index of an LED at the index of a frame is an ant.
			bool IsAnt(int frameIndex, int ledIndex)
			{
				var framePos = ledIndex % frameCount - frameIndex;
				if (framePos < 0)
					framePos += frameCount;
				return framePos >= 0 && framePos < antLength;
			}

			for (var i = 0; i < frameCount; i++)
			{
				var colours = new Color?[ledCount];

				// Generate marching ants
				for (var j = 0; j < ledCount; j++)
					colours[j] = IsAnt(i, j) ? antColour : bgColour;

				factory.AddFrame(colours, frameDisplayTime);
			}
		}
	}
}