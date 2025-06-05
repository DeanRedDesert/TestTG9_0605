namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// This interface allows you to make any component playable in the sequence system.
	/// </summary>
	public interface ISequencePlayable
	{
		/// <summary>
		/// Start playing the playable.
		/// </summary>
		void StartPlay();

		/// <summary>
		/// Stop playing the playable.
		/// </summary>
		void StopPlay(bool reset);

		/// <summary>
		/// Gets whether the playable is playing.
		/// </summary>
		/// <returns>True if playing, otherwise false.</returns>
		bool IsPlaying();
	}
}