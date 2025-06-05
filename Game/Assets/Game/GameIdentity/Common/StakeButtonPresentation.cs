using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Presentation;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Stakes;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Game.GameIdentity.Common
{
	[RequireComponent(typeof(Animator))]
	public sealed class StakeButtonPresentation : ButtonPresentation
	{
		#region Static Fields

		private static readonly int enabledParameterId = Animator.StringToHash("Enabled");
		private static readonly int pressedParameterId = Animator.StringToHash("Pressed");
		private static readonly int activeParameterId = Animator.StringToHash("Active");
		private static readonly int hiddenParameterId = Animator.StringToHash("Hidden");

		#endregion

		private ButtonStateData lastButtonStateData;
		private Animator animator;
		private Coroutine releaseCoroutine;
		private bool textConfigured;
		private bool textRefreshRequired;

		[SerializeField]
		public StakeButtonTextBase buttonText;

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void OnEnable()
		{
			var betButtonData = lastButtonStateData?.SpecificData as IStakeButtonSpecificData;
			var isButtonEnabled = lastButtonStateData?.ButtonState == ButtonState.Enabled;
			var isButtonSelected = betButtonData?.IsSelected == true;
			animator.SetBool(enabledParameterId, isButtonEnabled);
			animator.SetBool(activeParameterId, isButtonSelected);
			var animState = $"{(isButtonSelected ? "Active" : "Inactive")}{(isButtonEnabled ? "Enabled" : "Disabled")}";
			animator.Play(animState);
			animState = $"Idle{(isButtonSelected ? "Active" : "Inactive")}";
			animator.Play(animState);
			animator.Update(0);
			textConfigured = false;
			textRefreshRequired = true;
		}

		private void OnDisable()
		{
			animator.SetBool(enabledParameterId, false);
			animator.SetBool(pressedParameterId, false);
			animator.SetBool(activeParameterId, false);
		}

		#region ButtonPresentation overrides

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			if (!(newSpecificData is IStakeButtonSpecificData))
			{
				textRefreshRequired = true;
				textConfigured = false;
				return true;
			}

			if (oldSpecificData != null && !oldSpecificData.Equals(newSpecificData))
			{
				textRefreshRequired = true;
				return true;
			}

			return false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			bool isHidden = button.ButtonState == Button.State.Hidden;
			animator.SetBool(hiddenParameterId, isHidden);

			lastButtonStateData = buttonStateData;

			var isEnabled = button.ButtonState == Button.State.EnabledDown || button.ButtonState == Button.State.EnabledUp;
			animator.SetBool(enabledParameterId, isEnabled);
			if (button.ButtonState == Button.State.EnabledDown)
			{
				animator.SetBool(pressedParameterId, true);
				releaseCoroutine?.Stop();
				releaseCoroutine = FrameUpdateService.Update.StartCoroutine(Release());
			}

			var betButtonData = lastButtonStateData?.SpecificData as IStakeButtonSpecificData;
			animator.SetBool(activeParameterId, betButtonData?.IsSelected ?? false);

			if (buttonText)
			{
				if (isHidden)
				{
					buttonText.gameObject.SetActive(false);
				}
				else if (textConfigured && !textRefreshRequired)
				{
					buttonText.gameObject.SetActive(true);
					buttonText.UpdateEnabledState(isEnabled);
				}
				else
				{
					buttonText.gameObject.SetActive(true);
					buttonText.gameObject.SetActive(betButtonData != null);
					textConfigured = buttonText.Configure(betButtonData, isEnabled);
					textRefreshRequired = false;
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