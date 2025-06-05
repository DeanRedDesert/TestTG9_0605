using System;
using System.Collections.Generic;
using System.Diagnostics;
using Midas.Core.General;
using Midas.Core.LogicServices;

namespace Midas.Presentation.ExtensionMethods
{
	public static class GameServiceConsumerExtensions
	{
		private interface IActionBasedListener
		{
			bool IsEmpty { get; }
			void ReportHangingHandlers(string consumerServiceName);
		}

		private sealed class ActionBasedListener<T> : IGameServiceEventListener<T>, IActionBasedListener
		{
			public bool IsEmpty => OnChangeEvent == null;
			public event Action<T> OnChangeEvent;

			public void OnEventRaised(T newValue)
			{
				OnChangeEvent?.Invoke(newValue);
			}

			public void ReportHangingHandlers(string consumerServiceName)
			{
				if (OnChangeEvent == null)
					return;

				foreach (var handler in OnChangeEvent.GetInvocationList())
					Log.Instance.Fatal($"Property changed handlers is not unregistered from '{consumerServiceName} '{handler.Target?.GetType().FullName}.{handler.Method.Name}'");
			}
		}

		private static readonly Dictionary<object, object> registeredListeners = new Dictionary<object, object>();

		public static void RegisterChangeListenerAction<T>(this IGameServiceConsumer<T> consumer, Action<T> onChange)
		{
			ActionBasedListener<T> listener;
			if (!registeredListeners.TryGetValue(consumer, out var listenerObj))
			{
				listener = new ActionBasedListener<T>();
				registeredListeners.Add(consumer, listener);
			}
			else
			{
				listener = (ActionBasedListener<T>)listenerObj;
			}

			if (listener.IsEmpty)
				consumer.RegisterChangeListener(listener);

			listener.OnChangeEvent += onChange;
		}

		public static void UnregisterChangeListenerAction<T>(this IGameServiceConsumer<T> consumer, Action<T> onChange)
		{
			if (!registeredListeners.TryGetValue(consumer, out var listenerObj))
				return;

			var listener = (ActionBasedListener<T>)listenerObj;
			listener.OnChangeEvent -= onChange;

			if (listener.IsEmpty)
				consumer.UnregisterChangeListener(listener);
		}

		[Conditional("DEBUG")]
		public static void CheckForHangingChangeListeners()
		{
			foreach (var kvp in registeredListeners)
			{
				var listener = (IActionBasedListener)kvp.Value;

				if (listener.IsEmpty)
					continue;

				var consumerServiceName = ((IGameService)kvp.Key).Name;
				listener.ReportHangingHandlers(consumerServiceName);
			}
		}
		
		/// <summary>
		/// Registers for a game service change.
		/// </summary>
		/// <typeparam name="T">The game service type.</typeparam>
		private sealed class RegisterClassGameService<T> : IRegisterClass
		{
			private IGameServiceConsumer<T> gameServiceConsumer;
			private Action<T> handler;

			public RegisterClassGameService(IGameServiceConsumer<T> gameServiceConsumer, Action<T> handler)
			{
				this.gameServiceConsumer = gameServiceConsumer;
				this.handler = handler;
			}

			public void Register() => gameServiceConsumer.RegisterChangeListenerAction(handler);
			public void UnRegister() => gameServiceConsumer.UnregisterChangeListenerAction(handler);
		}

		public static void RegisterGameServiceChangedHandler<T>(this AutoUnregisterHelper autoUnregisterHelper, IGameServiceConsumer<T> gameServiceConsumer, Action<T> handler)
		{
			autoUnregisterHelper.Register(new RegisterClassGameService<T>(gameServiceConsumer, handler));
		}
	}
}