using System.Linq;
using Midas.Core;
using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class LinesAnteButtonText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString playXCreditsString;

		[SerializeField]
		private LocalizedString xLinesString;

		[SerializeField]
		private LocalizedString xCreditsForAnteString;

		[SerializeField]
		private TMP_Text playXCredits;

		[SerializeField]
		private TMP_Text xLines;

		[SerializeField]
		private TMP_Text xCreditsForAnte;

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		private void Awake()
		{
			var args = new object[] { 0L };
			playXCreditsString.Arguments = args;
			xLinesString.Arguments = args;
			xCreditsForAnteString.Arguments = args;
		}

		private void OnEnable()
		{
			playXCreditsString.StringChanged += OnPlayXCreditsStringChanged;
			xLinesString.StringChanged += OnXLinesStringChanged;
			xCreditsForAnteString.StringChanged += OnXCreditsForAnteString;
		}

		private void OnDisable()
		{
			playXCreditsString.StringChanged -= OnPlayXCreditsStringChanged;
			xLinesString.StringChanged -= OnXLinesStringChanged;
			xCreditsForAnteString.StringChanged -= OnXCreditsForAnteString;
		}

		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			if (!(stakeButtonSpecificData is PlayButtonSpecificData playButtonData))
				return false;

			if (playButtonData.IsCostToCover)
				return false;

			var d = playButtonData.StakeCombination.Values;
			var nonBetMultiplierStakes = d.Where(k => k.Key != Stake.BetMultiplier && k.Value != 0).ToArray();

			if (nonBetMultiplierStakes.Length != 2
				|| nonBetMultiplierStakes.All(s => s.Key != Stake.LinesBet)
				|| nonBetMultiplierStakes.All(s => s.Key != Stake.AnteBet))
			{
				return false;
			}

			UpdateFormattedText(playButtonData);

			Dimmer.Initialise(GetComponentsInChildren<TMP_Text>(false));
			Dimmer.SetDim(!isEnabled, true);
			return true;
		}

		private void UpdateFormattedText(PlayButtonSpecificData playButtonData)
		{
			var stakeCombination = playButtonData.StakeCombination;
			var linesBet = stakeCombination.Values[Stake.LinesBet];
			var ante = stakeCombination.Values[Stake.AnteBet];

			playXCreditsString.Arguments = new object[] { stakeCombination.TotalBet.Credits };
			playXCreditsString.RefreshString();
			xLinesString.Arguments = new object[] { linesBet };
			xLinesString.RefreshString();
			xCreditsForAnteString.Arguments = new object[] { ante };
			xCreditsForAnteString.RefreshString();
		}

		public override void UpdateEnabledState(bool enabledState)
		{
			Dimmer.SetDim(!enabledState);
		}

		private void OnPlayXCreditsStringChanged(string s)
		{
			playXCredits.text = s;
		}

		private void OnXLinesStringChanged(string s)
		{
			xLines.text = s;
		}

		private void OnXCreditsForAnteString(string s)
		{
			xCreditsForAnte.text = s;
		}
	}
}