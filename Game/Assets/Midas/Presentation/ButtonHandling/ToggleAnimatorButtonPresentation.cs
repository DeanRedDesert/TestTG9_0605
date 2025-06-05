using Midas.Presentation.Data.PropertyReference;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.ButtonHandling
{
	public class ToggleAnimatorButtonPresentation : ButtonPresentation
	{
		#region Static Fields

		private static readonly int enabledParameterId = Animator.StringToHash("Enabled");
		private static readonly int pressedParameterId = Animator.StringToHash("Pressed");

		#endregion

		private ButtonStateData lastButtonStateData;
		private Animator animator;
		private Coroutine releaseCoroutine;

		[SerializeField]
		private PropertyReferenceValueType<bool> propertyReference;

		protected virtual void Awake()
		{
			animator = GetComponent<Animator>();
		}

		protected virtual void OnDestroy()
		{
			releaseCoroutine?.Stop();
		}

		protected virtual void OnEnable()
		{
			if (propertyReference == null)
				return;

			propertyReference.ValueChanged += OnValueChanged;
			Refresh();

			var isButtonEnabled = lastButtonStateData?.ButtonState == ButtonState.Enabled;
			animator.SetBool(enabledParameterId, isButtonEnabled);
		}

		protected virtual void OnDisable()
		{
			animator.SetBool(enabledParameterId, false);
			animator.SetBool(pressedParameterId, false);
			propertyReference.ValueChanged -= OnValueChanged;
			propertyReference.DeInit();
		}

		#region ButtonPresentation overrides

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			lastButtonStateData = buttonStateData;
			var active = button.ButtonState != Button.State.Hidden;
			gameObject.SetActive(active);

			if (active && animator)
			{
				var isEnabled = button.ButtonState == Button.State.EnabledDown || button.ButtonState == Button.State.EnabledUp;
				animator.SetBool(enabledParameterId, isEnabled);
			}
		}

		#endregion

		private void OnValueChanged(PropertyReference arg1, string arg2)
		{
			Refresh();
		}

		private void Refresh()
		{
			var val = propertyReference.Value ?? false;
			animator.SetBool(pressedParameterId, val);
		}
	}
}