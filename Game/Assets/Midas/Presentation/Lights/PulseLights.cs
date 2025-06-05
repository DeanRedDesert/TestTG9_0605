using System;
using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Pulse Lights")]
	public sealed class PulseLights : RuntimeLights
	{
		private const float FramesPerSecond = 30f;
		private static readonly TimeSpan frameDisplayTime = TimeSpan.FromSeconds(1f / FramesPerSecond);

		[SerializeField]
		[Tooltip("The initial colour.")]
		private Color colour = Color.white;

		[SerializeField]
		[Tooltip("The colour to pulse to.")]
		private Color pulseColour = Color.blue;

		[SerializeField]
		[Tooltip("How long (in seconds) to transition to the pulse colour and back again.")]
		private float pulseTime = 1;

		[SerializeField]
		[Tooltip("How long (in seconds) to wait between pulses.")]
		private float betweenPulseTime = 5;

		public override TimeSpan Duration => TimeSpan.FromSeconds(pulseTime + betweenPulseTime);

		public override void CreateSequence(IRuntimeLightsFactory factory)
		{
			if (betweenPulseTime > 0f)
				factory.AddAllLightsFrame(colour, TimeSpan.FromSeconds(betweenPulseTime));

			if (pulseTime > 0f)
			{
				var pulseFrames = Mathf.RoundToInt(pulseTime * FramesPerSecond);
				var fullPulse = (pulseFrames - 1) / 2f;

				for (var i = 0; i < pulseFrames; i++)
				{
					var lerp = i <= fullPulse ? i / fullPulse : (pulseFrames - i) / fullPulse;
					var col = UnityEngine.Color.Lerp(colour, pulseColour, lerp);
					factory.AddAllLightsFrame(col, frameDisplayTime);
				}
			}
		}
	}
}