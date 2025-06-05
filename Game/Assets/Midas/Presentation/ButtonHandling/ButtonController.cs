using System;
using System.Collections.Generic;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation.ButtonHandling
{
	public abstract class ButtonController : IButtonController
	{
		#region Fields

		private AutoUnregisterHelper autoUnregisterHelper;
		private readonly List<ButtonStateData> buttonStates = new List<ButtonStateData>();

		#endregion

		#region Properties

		protected ButtonQueueController ButtonQueueController { get; private set; }
		protected ButtonEventDataQueueStatus ButtonEventDataQueueStatus { get; private set; }

		#endregion

		#region Public Methods

		public virtual void Init()
		{
			ButtonQueueController = GameBase.GameInstance.GetPresentationController<ButtonQueueController>();
			ButtonEventDataQueueStatus = StatusDatabase.ButtonEventDataQueueStatus;

			RegisterEvents();
			ButtonManager.AddGetButtonStateHandler(GetButtonStates);
		}

		public virtual void DeInit()
		{
			ButtonManager.RemoveGetButtonStateHandler(GetButtonStates);
			autoUnregisterHelper?.UnRegisterAll();
			UnregisterEvents();

			ButtonQueueController = null;
			ButtonEventDataQueueStatus = null;
		}

		public virtual void Destroy()
		{
		}

		#endregion

		#region Protected Methods

		protected void RequestButtonUpdate()
		{
			buttonStates.Clear();
			ButtonManager.RequestButtonUpdate();
		}

		protected void RegisterButtonEventListener(ButtonFunction buttonFunction, Action<ButtonEventData> handler)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterButtonEventListener(buttonFunction, handler);
		}

		protected void RegisterButtonEventListener(Action<ButtonEventData> handler)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterButtonEventListener(handler);
		}

		protected void RegisterButtonPressListener(ButtonFunction buttonFunction, Action<ButtonEventData> handler)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterButtonPressListener(buttonFunction, handler);
		}

		protected void AddButtonConditionPropertyChanged(StatusBlock statusBlock, string propertyName)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterPropertyChangedHandler(statusBlock, propertyName, OnButtonsConditionPropertyChanged);
		}

		protected void AddButtonConditionAnyPropertyChanged(StatusBlock statusBlock)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterAnyPropertyChangedHandler(statusBlock, OnButtonsConditionPropertyChanged);
		}

		protected void RegisterPropertyChangedHandler<T>(StatusBlock statusBlock, string propertyName, PropertyChangedHandler<T> handler)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterPropertyChangedHandler(statusBlock, propertyName, handler);
		}

		protected void AddButtonConditionExpressionChanged(Type type, string propertyName)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterExpressionChangedHandler(type, propertyName, RequestButtonUpdate);
		}

		protected void RegisterExpressionChangedHandler(Type type, string propertyName, Action handler)
		{
			autoUnregisterHelper ??= new AutoUnregisterHelper();
			autoUnregisterHelper.RegisterExpressionChangedHandler(type, propertyName, handler);
		}

		protected void AddButtonState(ButtonFunction id, bool enabled, object data = null)
		{
			AddButtonState(id, enabled ? ButtonState.Enabled : ButtonState.DisabledShow, enabled ? LightState.On : LightState.Off, data);
		}

		protected void AddButtonState(ButtonFunction id, ButtonState buttonState, object data = null)
		{
			AddButtonState(id, buttonState, buttonState == ButtonState.Enabled ? LightState.On : LightState.Off, data);
		}

		protected void AddButtonState(ButtonFunction id, bool enabled, LightState enabledLightState, object data = null)
		{
			AddButtonState(id, enabled, ButtonState.DisabledShow, enabledLightState, data);
		}

		protected void AddButtonState(ButtonFunction id, bool enabled, ButtonState disabledButtonState, object data = null)
		{
			AddButtonState(id, enabled, disabledButtonState, enabled ? LightState.On : LightState.Off, data);
		}

		protected void AddButtonState(ButtonFunction id, bool enabled, ButtonState disabledButtonState, LightState enabledLightState, object data = null)
		{
			var enabledState = enabled ? ButtonState.Enabled : disabledButtonState;
			var lightState = enabledState == ButtonState.Enabled ? enabledLightState : LightState.Off;

			AddButtonState(id, enabledState, lightState, data);
		}

		protected void AddButtonState(ButtonFunction id, ButtonState enabledState, LightState lightState, object data = null)
		{
			buttonStates.Add(new ButtonStateData(id, enabledState, lightState, data));
		}

		/// <summary>
		/// Override this method if you plan on registering events.
		/// </summary>
		/// <remarks>
		/// Use AddButtonConditionPropertyChanged methods to register status database change handlers. These will be unregistered automatically on shutdown.
		/// </remarks>
		protected virtual void RegisterEvents()
		{
		}

		/// <summary>
		/// Override this method to unregister events.
		/// </summary>
		/// <remarks>
		/// Any events registered using AddButtonConditionPropertyChanged methods do not require unregistering as this is automatic.
		/// </remarks>
		protected virtual void UnregisterEvents()
		{
		}

		protected abstract void UpdateButtonStates();

		#endregion

		#region Private Methods

		private IReadOnlyList<ButtonStateData> GetButtonStates()
		{
			if (buttonStates.Count == 0)
				UpdateButtonStates();

			return buttonStates;
		}

		private void OnButtonsConditionPropertyChanged(StatusBlock sender, string propertyName)
		{
			RequestButtonUpdate();
		}

		#endregion
	}
}