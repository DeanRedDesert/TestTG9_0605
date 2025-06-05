using System;
using Midas.Presentation.DevHelpers;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	[Serializable]
	public struct SequenceComponent
	{
		#region Inspector Fields

		[SerializeField]
		private int intensity;

		[SerializeField, ComponentSelector]
		private Component component;

		[SerializeField]
		private bool durationOverride;

		[SerializeField]
		private float duration;

		[SerializeField]
		private int animationComponentClipIndex;

		[SerializeField]
		private string animationComponentClipName;

		#endregion

		public SequenceComponent(int intensity, Component component, bool durationOverride, float duration)
		{
			this.component = component;
			this.intensity = intensity;
			this.durationOverride = durationOverride;
			this.duration = duration;
			animationComponentClipIndex = 0;
			animationComponentClipName = "";
		}

		/// <summary>
		/// The component that the sequence
		/// </summary>
		public Component Comp => component;

		/// <summary>
		/// Appears to be used after win counting completes to delay presentation a little longer. TODO: Could be named better
		/// </summary>
		public bool DurationOverride => durationOverride;

		/// <summary>
		/// How long to delay after the completion of the sequence event.
		/// </summary>
		public float Duration => duration;

		/// <summary>
		/// Used if the sequence component refers to an animation. TODO: I think this should be refactored away
		/// </summary>
		public int AnimationComponentClipIndex => animationComponentClipIndex;

		/// <summary>
		/// Used if the sequence component refers to an animation. TODO: I think this should be refactored away
		/// </summary>
		public string AnimationComponentClipName => animationComponentClipName;

		/// <summary>
		/// Checks if the component should run at the given intensity.
		/// </summary>
		/// <param name="currentIntensity">The intensity to run at.</param>
		/// <returns>True if the component should run, otherwise false.</returns>
		public bool ActivatesAtIntensity(int currentIntensity)
		{
			return intensity == currentIntensity || intensity == -1;
		}
	}
}