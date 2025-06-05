using System;
using Midas.Core;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Sequence to allow waiting for a specified time in addition to the features of <see cref="SequenceSkippable"/>.
	/// </summary>
	public sealed class SequenceWaitWithTimeout : SequenceSkippable
	{
		#region Fields

		private TimeSpan stopTime = TimeSpan.Zero;

		#endregion

		#region Inspector Fields

		[Tooltip("Time in seconds to wait")]
		[SerializeField]
		private float waitingDuration = 0f;

		#endregion

		#region SequenceSkippable Implementation

		protected override bool IsInterruptibleSequencePlaying => stopTime >= FrameTime.CurrentTime;
		protected override void PlaySkippableSequence(SequenceEventArgs sequenceEventArgs) => stopTime = FrameTime.CurrentTime + TimeSpan.FromSeconds(waitingDuration);
		protected override void StopSkippableSequence(SequenceEventArgs sequenceEventArgs) => stopTime = TimeSpan.Zero;

		#endregion
	}
}