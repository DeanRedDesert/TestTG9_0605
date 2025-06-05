using System;
using System.Collections.Generic;
using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Wave Lights")]
	public sealed class WaveLights : RuntimeLights
	{
		private const float FramesPerSecond = 30f;

		[SerializeField]
		[Tooltip("The initial colour.")]
		private Color colour = Color.white;

		[SerializeField]
		[Tooltip("The wave colour.")]
		private Color waveColour = Color.blue;

		[SerializeField]
		[Tooltip("How long (in seconds) to transition to do the wave and return to the start colour.")]
		private float waveTime = 1;

		[SerializeField]
		[Tooltip("How long (in seconds) to wait between pulses.")]
		private float betweenWaveTime = 5;

		[SerializeField]
		[Tooltip("The size of the wave as a percentage of the light device.")]
		private float waveSize = 0.2f;

		public override TimeSpan Duration => TimeSpan.FromSeconds(waveTime + betweenWaveTime);

		public override void CreateSequence(IRuntimeLightsFactory factory)
		{
			if (betweenWaveTime > 0f)
				factory.AddAllLightsFrame(colour, TimeSpan.FromSeconds(betweenWaveTime));

			if (waveTime > 0f)
			{
				var waveFrames = Mathf.RoundToInt(waveTime * FramesPerSecond);
				var ledFrameLength = waveFrames * waveSize;

				for (var frame = 0; frame < waveFrames; frame++)
				{
					var ledCount = factory.LightCount;
					var waveColours = new List<Color?>(ledCount);

					for (var led = 0; led < ledCount; led++)
					{
						var ledStartFrame1 = led * ((waveFrames - ledFrameLength) / ledCount);
						var ledPercent1 = (frame - ledStartFrame1) / ledFrameLength;

						var ledStartFrame2 = (ledCount - led) * ((waveFrames - ledFrameLength) / ledCount);
						var ledPercent2 = (frame - ledStartFrame2) / ledFrameLength;

						var lerp1 = ledPercent1 < 0f || ledPercent1 > 1f
							? 0
							: ledPercent1 < 0.5f
								? ledPercent1 * 2f
								: (1f - ledPercent1) * 2f;

						var lerp2 = ledPercent2 < 0f || ledPercent2 > 1f
							? 0
							: ledPercent2 < 0.5f
								? ledPercent2 * 2f
								: (1f - ledPercent2) * 2f;

						var lerp = Mathf.Max(lerp1, lerp2);
						waveColours.Add(lerp > 0
							? Color.Lerp(colour, waveColour, lerp)
							: colour);
					}

					factory.AddFrame(waveColours);
				}
			}
		}
	}
}