using System;

namespace Midas.LogicToPresentation
{
	/// <summary>
	/// A dispatcher implementation sends messages to its handlers.
	/// </summary>
	public interface IDispatcher
	{
		/// <summary>
		/// Adds a handler for a particular message type.
		/// </summary>
		void AddHandler<T>(Action<T> handler) where T : IMessage;

		/// <summary>
		/// Removes a handler for a particular message type.
		/// </summary>
		void RemoveHandler<T>(Action<T> handler) where T : IMessage;

		/// <summary>
		/// Returns true if there are messages available to be dispatched.
		/// </summary>
		bool HasMessagesToDispatch();

		/// <summary>
		/// Clears the message queue.
		/// </summary>
		void ForceClearMessages();

		/// <summary>
		/// Dispatch all messages
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		bool DispatchAll(Predicate<IMessage> filter = null);

		bool DispatchOne(Predicate<IMessage> filter = null);
	}
}