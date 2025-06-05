using System;
using System.Collections.Generic;
using Midas.Presentation.Cabinet;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Sparkle Lights")]
	public sealed class SparkleLights : RuntimeLights
	{
		[FormerlySerializedAs("Duration")]
		[SerializeField]
		[Tooltip("How long (in seconds) to play this stage light when it is activated (negative value for infinite duration).")]
		private float duration = 5;

		[FormerlySerializedAs("FramesPerSecond")]
		[SerializeField]
		[Tooltip("How many frames to display per second.")]
		[Range(1, 30)]
		private float framesPerSecond = 10;

		[FormerlySerializedAs("Colour")]
		[SerializeField]
		[Tooltip("The colour to use for a the background.")]
		private Color colour = Color.white;

		[FormerlySerializedAs("ColourWeights")]
		[Header("Sparkles Stage Light")]
		[SerializeField]
		[Tooltip("The colours to use for the sparkles.")]
		private WeightedColour[] colourWeights;

		/// <summary>
		/// Get the duration of the choreography, in seconds.
		/// </summary>
		public override TimeSpan Duration => TimeSpan.FromSeconds(duration);

		/// <summary>
		/// Create and return the LightSequence for the given cabinet and device.
		/// </summary>
		public override void CreateSequence(IRuntimeLightsFactory factory)
		{
			if (framesPerSecond > 30)
				framesPerSecond = 30;
			else if (framesPerSecond < 1)
				framesPerSecond = 1;

			var colourWeights = new List<Tuple<Color, float>>();
			var sum = 0f;

			foreach (var wc in this.colourWeights)
			{
				sum += wc.Weight;
				colourWeights.Add(new Tuple<Color, float>(wc.Colour, sum));
			}

			var defaultColour = colour;
			var frameCount = Mathf.RoundToInt(duration * framesPerSecond);
			var frameDisplayTime = TimeSpan.FromSeconds(1f / framesPerSecond);

			var random = new Random();

			for (var i = 0; i < frameCount; i++)
			{
				var colours = new List<Color?>();

				for (var j = 0; j < factory.LightCount; j++)
				{
					var weight = (float)random.NextDouble();
					var colour = defaultColour;

					foreach (var cw in colourWeights)
					{
						if (weight >= cw.Item2)
							continue;

						colour = cw.Item1;
						break;
					}

					colours.Add(colour);
				}

				factory.AddFrame(colours, frameDisplayTime);
			}
		}

		[Serializable]
		private struct WeightedColour
		{
			public float Weight;
			public Color Colour;
		}
	}
}