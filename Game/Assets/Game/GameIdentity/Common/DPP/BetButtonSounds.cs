using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Stakes;
using UnityEngine;

namespace Game.GameIdentity.Common.DPP
{
	public sealed class BetButtonSounds : MonoBehaviour
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private StakesStatus stakesStatus;

		[SerializeField]
		private SoundPlayerBase[] soundPlayers;

		[SerializeField]
		private SoundPlayerBase maxBetSound;

		private void OnEnable()
		{
			stakesStatus = StatusDatabase.StakesStatus;

			foreach (var buttonFunction in StatusDatabase.StakesStatus.BetButtonFunctions)
				autoUnregisterHelper.RegisterButtonEventListener(buttonFunction, OnBetButtonPressed);
		}

		private void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		private void OnBetButtonPressed(ButtonEventData buttonEventData)
		{
			if (buttonEventData.IsPressed)
			{
				var index = StatusDatabase.StakesStatus.BetButtonFunctions.FindIndex(buttonEventData.ButtonFunction);

				if (index < 0 || index >= stakesStatus.StakeGroups.Count)
					return;

				if (index == stakesStatus.StakeGroups.Count - 1)
					maxBetSound.Play();
				else
					soundPlayers[index].Play();
			}
		}
	}
}