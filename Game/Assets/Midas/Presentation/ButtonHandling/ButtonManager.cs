using System;
using System.Collections.Generic;

namespace Midas.Presentation.ButtonHandling
{
	public static class ButtonManager
	{
		#region Fields

		private static readonly List<Action<IReadOnlyCollection<ButtonStateData>>> buttonStateChangedActions = new List<Action<IReadOnlyCollection<ButtonStateData>>>();
		private static readonly Dictionary<ButtonFunction, List<Action<ButtonStateData>>> buttonStateChangedActionsById = new Dictionary<ButtonFunction, List<Action<ButtonStateData>>>();
		private static readonly List<Func<IReadOnlyList<ButtonStateData>>> getButtonStateFunctions = new List<Func<IReadOnlyList<ButtonStateData>>>();
		private static readonly List<ButtonStateData> buttonStatesCachedList = new List<ButtonStateData>();
		private static readonly List<ButtonEventData> buttonEvents = new List<ButtonEventData>();
		private static readonly List<Action<ButtonEventData>> buttonEventListeners = new List<Action<ButtonEventData>>();
		private static readonly Dictionary<ButtonFunction, List<Action<ButtonEventData>>> buttonEventListenersPerId = new Dictionary<ButtonFunction, List<Action<ButtonEventData>>>();
		private static readonly Dictionary<ButtonFunction, List<Action<ButtonEventData>>> buttonPressListenersPerId = new Dictionary<ButtonFunction, List<Action<ButtonEventData>>>();
		private static bool buttonUpdateRequested;

		#endregion

		#region Properties

		public static IReadOnlyList<ButtonStateData> ButtonStates { get; private set; } = new List<ButtonStateData>();

		#endregion

		#region Public Methods

		public static void Init()
		{
		}

		public static void DeInit()
		{
			ErrorLogEventSubscribers();
			buttonStateChangedActions.Clear();
			buttonStateChangedActionsById.Clear();
			getButtonStateFunctions.Clear();
			buttonStatesCachedList.Clear();
			buttonEvents.Clear();
			buttonEventListeners.Clear();
			buttonEventListenersPerId.Clear();
			buttonUpdateRequested = false;
		}

		public static void AddButtonsStateChangedListener(Action<IReadOnlyCollection<ButtonStateData>> action)
		{
			buttonStateChangedActions.Add(action);
		}

		public static void AddButtonStateChangedListener(ButtonFunction buttonFunction, Action<ButtonStateData> action)
		{
			if (!buttonStateChangedActionsById.TryGetValue(buttonFunction, out var actions))
			{
				actions = new List<Action<ButtonStateData>>();
				buttonStateChangedActionsById.Add(buttonFunction, actions);
			}

			actions.Add(action);
		}

		public static void RemoveButtonsStateChangedListener(Action<IReadOnlyCollection<ButtonStateData>> action)
		{
			buttonStateChangedActions.Remove(action);
		}

		public static void RemoveButtonStateChangedListener(ButtonFunction buttonFunction, Action<ButtonStateData> action)
		{
			if (buttonStateChangedActionsById.TryGetValue(buttonFunction, out var list))
				list.Remove(action);
		}

		public static void AddGetButtonStateHandler(Func<IReadOnlyList<ButtonStateData>> getButtonStates)
		{
			getButtonStateFunctions.Add(getButtonStates);
		}

		public static void AddButtonEventListener(Action<ButtonEventData> buttonEventListener)
		{
			buttonEventListeners.Add(buttonEventListener);
		}

		public static void AddButtonEventListener(ButtonFunction buttonFunction, Action<ButtonEventData> buttonEventListener)
		{
			if (!buttonEventListenersPerId.TryGetValue(buttonFunction, out var listeners))
			{
				listeners = new List<Action<ButtonEventData>>();
				buttonEventListenersPerId.Add(buttonFunction, listeners);
			}

			listeners.Add(buttonEventListener);
		}

		public static void AddButtonPressListener(ButtonFunction buttonFunction, Action<ButtonEventData> buttonEventListener)
		{
			if (!buttonPressListenersPerId.TryGetValue(buttonFunction, out var listeners))
			{
				listeners = new List<Action<ButtonEventData>>();
				buttonPressListenersPerId.Add(buttonFunction, listeners);
			}

			listeners.Add(buttonEventListener);
		}

		public static void PostButtonEvent(ButtonEventData buttonEvent)
		{
			Log.Instance.Debug($"PostButtonEvent {buttonEvent}");
			buttonEvents.Add(buttonEvent);
		}

		public static void EmulateButtonPress(ButtonFunction buttonFunction)
		{
			PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Down));
			PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Pressed));
			PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Up));
		}

		public static void RemoveButtonEventListener(Action<ButtonEventData> buttonEventAction)
		{
			if (!buttonEventListeners.Remove(buttonEventAction))
			{
				Log.Instance.Error($"{buttonEventAction.Target?.GetType().FullName}.{buttonEventAction.Method.Name} not registered");
			}
		}

		public static void RemoveButtonEventListener(ButtonFunction buttonFunction, Action<ButtonEventData> buttonEventAction)
		{
			if (!buttonEventListenersPerId[buttonFunction].Remove(buttonEventAction))
			{
				Log.Instance.Error($"{buttonEventAction.Target?.GetType().FullName}.{buttonEventAction.Method.Name} for {buttonFunction} not registered");
			}
		}

		public static void RemoveButtonPressListener(ButtonFunction buttonFunction, Action<ButtonEventData> buttonEventAction)
		{
			if (!buttonPressListenersPerId[buttonFunction].Remove(buttonEventAction))
			{
				Log.Instance.Error($"{buttonEventAction.Target?.GetType().FullName}.{buttonEventAction.Method.Name} for {buttonFunction} not registered");
			}
		}

		public static void RemoveGetButtonStateHandler(Func<IReadOnlyList<ButtonStateData>> getButtonStates)
		{
			getButtonStateFunctions.Remove(getButtonStates);
		}

		public static void RequestButtonUpdate()
		{
			buttonUpdateRequested = true;
		}

		public static void Update()
		{
			SendButtonEvents();

			if (buttonUpdateRequested)
			{
				CollectButtonStates();
				SendButtonStates();
				buttonUpdateRequested = false;
			}
		}

		#endregion

		#region Private Methods

		private static void ErrorLogEventSubscribers()
		{
			foreach (var buttonEventListener in buttonEventListeners)
			{
				Log.Instance.Error($"Button event listener {buttonEventListener.Target?.GetType().FullName}.{buttonEventListener.Method.Name} not unregistered");
			}

			foreach (var keyValuePair in buttonEventListenersPerId)
			{
				foreach (var buttonEventListener in keyValuePair.Value)
				{
					Log.Instance.Error($"Button event listener {buttonEventListener.Target?.GetType().FullName}.{buttonEventListener.Method.Name} for {keyValuePair.Key} not unregistered");
				}
			}
		}

		private static void CollectButtonStates()
		{
			buttonStatesCachedList.Clear();
			foreach (var buttonStateFunction in getButtonStateFunctions)
			{
				buttonStatesCachedList.AddRange(buttonStateFunction());
			}

			ButtonStates = buttonStatesCachedList;
		}

		private static void SendButtonStates()
		{
			foreach (var buttonStateData in ButtonStates)
			{
				if (buttonStateChangedActionsById.TryGetValue(buttonStateData.ButtonFunction, out var stateActions))
				{
					foreach (var stateAction in stateActions)
					{
						stateAction(buttonStateData);
					}
				}
			}

			foreach (var buttonStateChangedAction in buttonStateChangedActions)
			{
				buttonStateChangedAction(ButtonStates);
			}
		}

		private static void SendButtonEvents()
		{
			if (buttonEvents.Count > 0)
			{
				foreach (var buttonEventData in buttonEvents)
					SendButtonEvent(buttonEventData);

				buttonEvents.Clear();
			}
		}

		private static void SendButtonEvent(ButtonEventData buttonEventData)
		{
			Log.Instance.Debug($"Sending Button Event: {buttonEventData}");

			var buttonFunction = buttonEventData.ButtonFunction;
			if (buttonEventListenersPerId.TryGetValue(buttonFunction, out var eventListeners))
			{
				SendButtonEvent(eventListeners, buttonEventData);
			}

			if (buttonEventData.IsPressed && buttonPressListenersPerId.TryGetValue(buttonFunction, out var pressListeners))
			{
				SendButtonEvent(pressListeners, buttonEventData);
			}

			SendButtonEvent(buttonEventListeners, buttonEventData);
		}

		private static void SendButtonEvent(List<Action<ButtonEventData>> listeners, ButtonEventData buttonEventData)
		{
			foreach (var buttonEventListener in listeners)
			{
				buttonEventListener(buttonEventData);
			}
		}

		#endregion
	}
}