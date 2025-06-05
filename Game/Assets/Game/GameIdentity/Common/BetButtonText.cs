using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class BetButtonText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString betString;

		[SerializeField]
		private LocalizedString creditsString;

		[SerializeField]
		private TMP_Text bet;

		[SerializeField]
		private TMP_Text value;

		[SerializeField]
		private TMP_Text credits;

		#region Properties

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		#endregion

		private void Awake()
		{
			creditsString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			betString.StringChanged += OnBetStringChanged;
			creditsString.StringChanged += OnCreditsStringChanged;
		}

		private void OnDisable()
		{
			betString.StringChanged -= OnBetStringChanged;
			creditsString.StringChanged -= OnCreditsStringChanged;
		}

		private void OnBetStringChanged(string s)
		{
			bet.text = s;
		}

		private void OnCreditsStringChanged(string s)
		{
			credits.text = s;
		}

		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			if (!(stakeButtonSpecificData is BetButtonSpecificData betButtonSpecificData))
				return false;

			UpdateFormattedText(betButtonSpecificData);

			Dimmer.Initialise(GetComponentsInChildren<TMP_Text>(true));
			Dimmer.SetDim(!isEnabled, true);
			return true;
		}

		private void UpdateFormattedText(BetButtonSpecificData betButtonSpecificData)
		{
			if (betButtonSpecificData.HasAnteBet)
			{
				value.text = betButtonSpecificData.StakeMultiplier + "x";
				credits.gameObject.SetActive(false);
			}
			else
			{
				value.text = betButtonSpecificData.StakeMultiplier.ToString();
				credits.gameObject.SetActive(true);
				creditsString.Arguments = new object[] { betButtonSpecificData.StakeMultiplier };
				creditsString.RefreshString();
			}
		}

		public override void UpdateEnabledState(bool enabled)
		{
			Dimmer.SetDim(!enabled);
		}
	}
}