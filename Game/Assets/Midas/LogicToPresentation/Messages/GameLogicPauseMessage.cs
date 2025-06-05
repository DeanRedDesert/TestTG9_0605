using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Message to Pause/UnPause the Game (not interaction should be possible)
	/// <remarks>
	/// Logic -> Pres: When the Display State of the Game Changes (e.g.: Door Open)
	/// <see cref="Pause" /> Normal -> false, Suspended -> true, Hidden -> true
	/// </remarks>
	/// </summary>
	public sealed class GameLogicPauseMessage : IMessage
	{
		#region Public

		public GameLogicPauseMessage(bool pause)
		{
			Pause = pause;
		}

		public override string ToString()
		{
			return $"Pause:{Pause}";
		}

		public bool Pause { get; }

		#endregion
	}

	/// <summary>
	/// Used for messages that are used for debugging (display state simulator, sim operator menu, etc)
	/// </summary>
	public abstract class DebugMessage : IMessage
	{
	}

	public sealed class DebugDisplayStateMessage : DebugMessage
	{
		public DisplayState NewDisplayState { get; }

		public DebugDisplayStateMessage(DisplayState newDisplayState)
		{
			NewDisplayState = newDisplayState;
		}
	}
}