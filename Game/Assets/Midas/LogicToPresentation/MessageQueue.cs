// Copyright (c) 2021 IGT

using System;
using System.Collections.Generic;

namespace Midas.LogicToPresentation
{
	/// Implements a message queue for a thread.
	internal sealed class MessageQueue<TMessage> where TMessage : class
	{
		private readonly List<TMessage> messageQueue = new List<TMessage>();
		private readonly object mutexObject = new object();

		/// <summary>
		/// Notifies when a message is added to the queue.
		/// </summary>
		public event Action MessageAdded;

		/// <summary>
		/// Number of messages currently in the queue
		/// </summary>
		public int NumMessages
		{
			get
			{
				lock (mutexObject)
				{
					return messageQueue.Count;
				}
			}
		}

		/// <summary>
		/// Post a message to the thread that owns this message queue.
		/// </summary>
		public void PostMessage(TMessage message)
		{
			lock (mutexObject)
			{
				messageQueue.Add(message);
			}

			MessageAdded?.Invoke();
		}

		/// <summary>
		/// Post multiple messages to the thread that owns this message queue.
		/// </summary>
		/// <param name="messages"></param>
		public void PostMessages(IReadOnlyList<TMessage> messages)
		{
			lock (mutexObject)
			{
				messageQueue.AddRange(messages);
			}

			MessageAdded?.Invoke();
		}

		/// <summary>
		/// Get the next message for specified message filter.
		/// If filter is null it will return first message, or if no message is there it will return null.
		/// </summary>
		public TMessage GetMessage(Predicate<TMessage> filter = null)
		{
			TMessage message = null;
			lock (mutexObject)
			{
				if (messageQueue.Count > 0)
				{
					var index = filter != null ? messageQueue.FindIndex(filter) : 0;
					if (index != -1)
					{
						message = messageQueue[index];
						messageQueue.RemoveAt(index);
					}
				}
			}

			return message;
		}

		/// <summary>
		/// Get all messages currently in the queue.
		/// </summary>
		public IReadOnlyList<TMessage> GetMessages(Predicate<TMessage> filter = null)
		{
			lock (mutexObject)
			{
				if (messageQueue.Count > 0)
				{
					var messages = new List<TMessage>();
					TMessage message;
					while ((message = GetMessage(filter)) != null)
					{
						messages.Add(message);
					}

					return messages;
				}

				return Array.Empty<TMessage>();
			}
		}
	}
}