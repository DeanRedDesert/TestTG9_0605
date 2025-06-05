using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	/// This button presentation uses an animator with two bool parameters: and has a bool flag for Enabled state and a bool flag for Pressed state
	/// Enabled - Show the enabled or disabled state.
	/// Pressed - Show the button in pressed or idle state. This parameter is set to true for 0.2 seconds.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class SimpleAnimatorButtonPresentation : ButtonPresentation
	{
		#region Static Fields

		private static readonly int enabledParameterId = Animator.StringToHash("Enabled");
		private static readonly int pressedParameterId = Animator.StringToHash("Pressed");

		#endregion

		private ButtonStateData lastButtonStateData;
		private Animator animator;
		private Coroutine releaseCoroutine;

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
			var isButtonEnabled = lastButtonStateData?.ButtonState == ButtonState.Enabled;
			animator.SetBool(enabledParameterId, isButtonEnabled);
		}

		protected virtual void OnDisable()
		{
			animator.SetBool(enabledParameterId, false);
			animator.SetBool(pressedParameterId, false);
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

				if (button.ButtonState == Button.State.EnabledDown)
				{
					animator.SetBool(pressedParameterId, true);
					releaseCoroutine?.Stop();
					releaseCoroutine = FrameUpdateService.Update.StartCoroutine(Release());
				}
			}
		}

		#endregion

		private IEnumerator<CoroutineInstruction> Release()
		{
			yield return new CoroutineDelay(TimeSpan.FromSeconds(0.2));
			animator.SetBool(pressedParameterId, false);
			releaseCoroutine = null;
		}
	}
}