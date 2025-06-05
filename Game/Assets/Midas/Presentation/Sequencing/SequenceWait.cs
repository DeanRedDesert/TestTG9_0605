namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Sequence to allow waiting for a skippable input to occur. eg the play button being pressed./>.
	/// </summary>
	public sealed class SequenceWait : SequenceSkippable
	{
		#region Fields

		private bool isPlaying;

		#endregion

		#region SequenceSkippable Implementation

		protected override bool IsInterruptibleSequencePlaying => isPlaying;
		protected override void PlaySkippableSequence(SequenceEventArgs sequenceEventArgs) => isPlaying = true;

		protected override void StopSkippableSequence(SequenceEventArgs sequenceEventArgs) => isPlaying = false;

		#endregion
	}
}