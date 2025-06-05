using System;
using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.Lights
{
	[CreateAssetMenu(menuName = "Midas/Lights/Solid Lights")]
	public sealed class SolidLights : RuntimeLights
	{
		private enum LightPosition
		{
			All,
			Backing,
			Facing
		}

		[Header("Solid Lights")]
		[SerializeField]
		[Tooltip("The colour to use for this solid effect.")]
		private Color color = Color.white;

		[SerializeField]
		[Tooltip("How much of the cabinet do you want to effect?")]
		private LightPosition lightPosition = LightPosition.All;

		public override TimeSpan Duration { get; } = TimeSpan.FromMilliseconds(33);

		public override void CreateSequence(IRuntimeLightsFactory factory)
		{
			switch (lightPosition)
			{
				case LightPosition.All:
					factory.AddAllLightsFrame(color);
					break;
				case LightPosition.Backing:
					factory.AddBackingLightsFrame(color);
					break;
				case LightPosition.Facing:
					factory.AddFacingLightsFrame(color);
					break;
			}
		}
	}
}