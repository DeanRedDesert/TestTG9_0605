using System;

namespace Midas.LogicToPresentation
{
	/// <summary>
	/// Handles sending messages to a particular thread.
	/// </summary>
	public interface ISender
	{
		/// This event is raised when a message is send (not enqueued)
		event Action MessageToMessageQueueAdded;

		/// <summary>
		/// Send a message immediately to the target thread.
		/// </summary>
		/// <param name="message">The message to send.</param>
		void Send(IMessage message);

		/// <summary>
		/// Place a message on a queue to be sent as a batch using SendEnqueuedMessages.
		/// </summary>
		/// <param name="message">The message to enqueue.</param>
		void EnqueueMessage(IMessage message);

		/// <summary>
		/// Sends enqueued messages to the target thread in a single batch.
		/// </summary>
		void SendEnqueuedMessages();
	}
}