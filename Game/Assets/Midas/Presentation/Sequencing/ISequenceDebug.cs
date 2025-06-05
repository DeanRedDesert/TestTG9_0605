using System.Collections.Generic;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Provides data to be presented in a Unity window to show the current state of a sequence.
	/// </summary>
	public interface ISequenceDebug
	{
		/// <summary>
		/// Unused right now, originally used in a UTP automation module in GGA.
		/// </summary>
		/// <param name="sequenceId">The sequence ID</param>
		/// <returns>All awaiters for the given sequence ID.</returns>
		IEnumerable<object> GetAwaiters(int sequenceId);
		
		/// <summary>
		/// Gets whether the sequence is currently in the given event.
		/// </summary>
		/// <param name="eventId">The event to check.</param>
		/// <returns>True if the sequence is in the given event, otherwise false.</returns>
		bool IsInEvent(int eventId);
		
		/// <summary>
		/// Gets the list of sequence event callbacks and whether they are waiting or not.
		/// </summary>
		/// <param name="eventId">The event to check.</param>
		/// <returns>The collection of callbacks for the given event ID.</returns>
		IEnumerable<(SequenceEventHandler callBack, bool isWaiting)> GetSequenceEventCallbacks(int eventId);
	}
}