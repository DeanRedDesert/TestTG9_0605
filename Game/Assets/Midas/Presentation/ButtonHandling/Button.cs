using System;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Midas.Presentation.ButtonHandling
{
	public sealed class Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		#region Nested Types

		public enum TriggerStyle
		{
			Pressed,
			Released
		}

		public enum State
		{
			Hidden,
			EnabledUp,
			EnabledDown,
			DisabledUp,
			DisabledDown
		}

		private enum TouchInterface
		{
			Pointer,
			Mouse
		}

		#endregion

		#region Fields

		private Action<bool> setColliderEnable;
		private ButtonPresentation buttonPresentation;
		private TouchInterface? touchInterface;
		private bool buttonFunctionRegistered;
		private ButtonStateData lastButtonStateData;
		private State buttonState;
		private bool isMouseDown;
		private bool isMouseInside;
		private bool stateRefreshRequired;
		private bool visualRefreshRequired;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private ButtonFunction buttonFunction = new ButtonFunction(ButtonFunction.Undefined);

		[SerializeField]
		private TriggerStyle triggerStyle;

		#endregion

		#region Properties

		public State ButtonState
		{
			get => buttonState;
			private set
			{
				if (buttonState == value) return;
				buttonState = value;
				visualRefreshRequired = true;
			}
		}

		public bool IsMouseDown
		{
			get => isMouseDown;
			private set
			{
				if (isMouseDown == value) return;
				isMouseDown = value;
				stateRefreshRequired = true;
			}
		}

		public bool IsMouseInside
		{
			get => isMouseInside;
			private set
			{
				if (isMouseInside == value) return;
				isMouseInside = value;
				stateRefreshRequired = true;
			}
		}

		#endregion

		#region Public Methods

		public void ChangeButtonFunction(ButtonFunction newFunction)
		{
			UnRegisterButtonFunction();
			buttonFunction = newFunction;
			RegisterButtonFunction();
		}

		#endregion

		#region Private Methods

		private void OnFrameUpdate()
		{
			if (stateRefreshRequired)
			{
				stateRefreshRequired = false;
				RunStateMachine();
			}

			if (visualRefreshRequired && lastButtonStateData != null)
			{
				visualRefreshRequired = false;
				if (buttonPresentation)
					buttonPresentation.RefreshVisualState(this, lastButtonStateData);
			}
		}

		private void OnMouseOrPointerDown()
		{
			if (ButtonState == State.EnabledUp || ButtonState == State.EnabledDown)
			{
				IsMouseInside = true;
				IsMouseDown = true;
				PostButtonEvent(ButtonEvent.Down);

				if (triggerStyle == TriggerStyle.Pressed)
					PostButtonEvent(ButtonEvent.Pressed);
			}
		}

		private void OnMouseOrPointerUp()
		{
			if (!IsMouseDown)
			{
				return;
			}

			IsMouseDown = false;
			PostButtonEvent(ButtonEvent.Up);
			if (IsMouseInside && triggerStyle == TriggerStyle.Released && ButtonState == State.EnabledDown)
				PostButtonEvent(ButtonEvent.Pressed);
		}

		private void OnMouseOrPointerEnter()
		{
			IsMouseInside = true;
		}

		private void OnMouseOrPointerExit()
		{
			IsMouseInside = false;
		}

		private void RunStateMachine()
		{
			if (lastButtonStateData == null)
				return;

			switch (lastButtonStateData.ButtonState)
			{
				case ButtonHandling.ButtonState.DisabledHide:
					ButtonState = State.Hidden;
					break;
				case ButtonHandling.ButtonState.DisabledShow:
					ButtonState = IsMouseDown ? State.DisabledDown : State.DisabledUp;
					break;
				case ButtonHandling.ButtonState.Enabled:
					ButtonState = IsMouseDown ? State.EnabledDown : State.EnabledUp;
					break;
				default:
					Log.Instance.Error($"Unknown button state {lastButtonStateData.ButtonState}");
					break;
			}

			setColliderEnable?.Invoke(lastButtonStateData.ButtonState == ButtonHandling.ButtonState.Enabled);
		}

		private void RegisterButtonFunction()
		{
			if (buttonFunction != null && !buttonFunctionRegistered)
			{
				ButtonManager.AddButtonStateChangedListener(buttonFunction, UpdateButtonState);
				buttonFunctionRegistered = true;

				var newButtonState = ButtonManager.ButtonStates.FirstOrDefault(bs => bs.ButtonFunction.Equals(buttonFunction));
				if (newButtonState != null)
					UpdateButtonState(newButtonState);
			}
		}

		private void UnRegisterButtonFunction()
		{
			if (buttonFunctionRegistered)
			{
				ButtonManager.RemoveButtonStateChangedListener(buttonFunction, UpdateButtonState);
				buttonFunctionRegistered = false;
			}
		}

		private void UpdateButtonState(ButtonStateData buttonStateData)
		{
			if (!buttonStateData.Equals(lastButtonStateData))
			{
				stateRefreshRequired = true;

				// See if we need to force a visual refresh due to specific data changes.

				visualRefreshRequired |= lastButtonStateData == null ||
					buttonPresentation && buttonPresentation.OnSpecificButtonDataChanged(lastButtonStateData.SpecificData, buttonStateData.SpecificData);
				lastButtonStateData = buttonStateData;
			}
		}

		private void PostButtonEvent(ButtonEvent buttonEvent)
		{
			ButtonManager.PostButtonEvent(new ButtonEventData(buttonFunction, buttonEvent));
		}

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			buttonPresentation = GetComponent<ButtonPresentation>();
			FrameUpdateService.Update.OnFrameUpdate += OnFrameUpdate;
			RegisterButtonFunction();

			stateRefreshRequired = true;
			visualRefreshRequired = true;

			// Collider and Collider2D have different inheritance hierarchies, hence this strange implementation.

			var c = GetComponent<Collider>();
			if (c)
				setColliderEnable = en => c.enabled = en;
			else
			{
				var c2d = GetComponent<Collider2D>();
				if (c2d)
					setColliderEnable = en => c2d.enabled = en;
			}

			setColliderEnable?.Invoke(false);
		}

		private void OnEnable()
		{
			stateRefreshRequired = true;
			visualRefreshRequired = true;
			IsMouseDown = false;
			IsMouseInside = false;
		}

		private void OnDisable()
		{
			IsMouseInside = false;
			if (IsMouseDown)
			{
				OnMouseOrPointerUp();
				IsMouseDown = false;
			}
		}

		private void OnDestroy()
		{
			UnRegisterButtonFunction();
			if (FrameUpdateService.Update != null)
				FrameUpdateService.Update.OnFrameUpdate -= OnFrameUpdate;
		}

		#region Mouse Handling

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			Log.Instance.Debug($"OnPointerDown of {this}");
			EnsureTouchInterface(TouchInterface.Pointer);
			EventSystem.current.SetSelectedGameObject(gameObject, eventData);
			OnMouseOrPointerDown();
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			Log.Instance.Debug($"OnPointerUp of {this}");
			OnMouseOrPointerUp();
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			Log.Instance.Debug($"OnPointerEnter of {this}");
			OnMouseOrPointerEnter();
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			Log.Instance.Debug($"OnPointerExit of {this}");
			OnMouseOrPointerExit();
		}

		void ISelectHandler.OnSelect(BaseEventData eventData)
		{
			Log.Instance.Debug($"OnSelect of {this}");
		}

		void IDeselectHandler.OnDeselect(BaseEventData eventData)
		{
			Log.Instance.Debug($"OnDeselect of {this}");
			OnMouseOrPointerUp();
		}

		private void OnMouseDown()
		{
			Log.Instance.Debug($"OnMouseDown of {this}");
			EnsureTouchInterface(TouchInterface.Mouse);

			OnMouseOrPointerDown();
		}

		private void OnMouseUp()
		{
			Log.Instance.Debug($"OnMouseUp of {this}");
			OnMouseOrPointerUp();
		}

		private void OnMouseEnter()
		{
			Log.Instance.Debug($"OnMouseEnter of {this}");
			OnMouseOrPointerEnter();
		}

		private void OnMouseExit()
		{
			Log.Instance.Debug($"OnMouseExit of {this}");
			OnMouseOrPointerExit();
		}

		private void EnsureTouchInterface(TouchInterface @interface)
		{
			if (touchInterface == null)
				touchInterface = @interface;
			else if (touchInterface != @interface)
				Log.Instance.Error($"Button {this.GetPath()} can not use Pointer and Mouse interface at the same time. Check Raycast setting");
		}

		#endregion

		#endregion

		#region Object overrides

		public override string ToString() => $"{name}, function: {buttonFunction}";

		#endregion
	}
}