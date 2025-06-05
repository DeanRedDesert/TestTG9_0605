using System;
using System.Collections.Generic;

namespace Midas.LogicToPresentation
{
	/// <summary>
	/// Implements a combined sender and dispatcher.
	/// </summary>
	internal sealed class MessageDispatcher : IDispatcher, ISender
	{
		#region Types

		private interface IMessageSubscriber
		{
			void Dispatch(IMessage message);
		}

		private sealed class MessageSubscriber<T> : IMessageSubscriber where T : IMessage
		{
			public event Action<T> Handler;

			public void Dispatch(IMessage message)
			{
				if (message is T tMessage)
					Handler?.Invoke(tMessage);
			}
		}

		#endregion

		#region Fields

		private readonly string name;
		private readonly MessageQueue<IMessage> messageQueue = new MessageQueue<IMessage>();
		private readonly List<IMessage> queuedMessages = new List<IMessage>();
		private readonly Dictionary<Type, IMessageSubscriber> messageHandlers = new Dictionary<Type, IMessageSubscriber>();

		#endregion

		public MessageDispatcher(string name)
		{
			this.name = name;
		}
		
		#region ISender Implementation

		public event Action MessageToMessageQueueAdded
		{
			add => messageQueue.MessageAdded += value;
			remove => messageQueue.MessageAdded -= value;
		}

		public void Send(IMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			Log.Instance.InfoFormat("{0} sending {1}", name, message.ToString());
			messageQueue.PostMessage(message);
		}

		public void EnqueueMessage(IMessage message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			Log.Instance.InfoFormat("{0} enqueueing {1}", name, message.ToString());
			queuedMessages.Add(message);
		}

		public void SendEnqueuedMessages()
		{
			Log.Instance.InfoFormat("{0} posting enqueued messages", name);
			messageQueue.PostMessages(queuedMessages);
			queuedMessages.Clear();
		}

		#endregion

		#region IDispatcher implementation

		public bool HasMessagesToDispatch()
		{
			return messageQueue.NumMessages > 0;
		}

		public bool DispatchAll(Predicate<IMessage> filter = null)
		{
			var dispatched = false;
			var messages = messageQueue.GetMessages(filter);
			foreach (var message in messages)
			{
				Dispatch(message);
				dispatched = true;
			}

			return dispatched;
		}

		public bool DispatchOne(Predicate<IMessage> filter = null)
		{
			var dispatched = false;
			var message = messageQueue.GetMessage(filter);
			if (message != null)
			{
				Dispatch(message);
				dispatched = true;
			}

			return dispatched;
		}

		public void AddHandler<T>(Action<T> handler) where T : IMessage
		{
			var key = typeof(T);

			if (!messageHandlers.TryGetValue(key, out var typeMessageHandler))
			{
				typeMessageHandler = new MessageSubscriber<T>();
				messageHandlers.Add(key, typeMessageHandler);
			}

			((MessageSubscriber<T>)typeMessageHandler).Handler += handler;
		}

		public void RemoveHandler<T>(Action<T> handler) where T : IMessage
		{
			var key = typeof(T);

			if (messageHandlers.ContainsKey(key))
			{
				if (messageHandlers[key] is MessageSubscriber<T> subscriber)
				{
					subscriber.Handler -= handler;
				}
			}
		}

		public void ForceClearMessages()
		{
			if (messageQueue.NumMessages > 0)
				messageQueue.GetMessages();

			if (queuedMessages.Count > 0)
				queuedMessages.Clear();
		}

		private void Dispatch(IMessage message)
		{
			Log.Instance.InfoFormat("{0} dispatching {1}", name, message.ToString());
			var type = message.GetType();
			while (type != null && type != typeof(object))
			{
				DispatchType(message, type);

				type = type.BaseType;
			}

			DispatchType(message, typeof(IMessage));
		}

		private void DispatchType(IMessage message, Type type)
		{
			if (messageHandlers.TryGetValue(type, out var handler))
				handler.Dispatch(message);
		}

		#endregion
	}
}