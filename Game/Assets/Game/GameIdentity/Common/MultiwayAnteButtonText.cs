using System.Linq;
using Midas.Core;
using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class MultiwayAnteButtonText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString playXCreditsString;

		[SerializeField]
		private LocalizedString xWaysString;

		[SerializeField]
		private LocalizedString xCreditsForAnteString;

		[SerializeField]
		private TMP_Text playXCredits;

		[SerializeField]
		private TMP_Text xWays;

		[SerializeField]
		private TMP_Text xCreditsForAnte;

		[SerializeField]
		private MultiwayWaysMapping waysMapping;

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		private void Awake()
		{
			var args = new object[] { 0L };
			playXCreditsString.Arguments = args;
			xWaysString.Arguments = args;
			xCreditsForAnteString.Arguments = args;
		}

		private void OnEnable()
		{
			playXCreditsString.StringChanged += OnPlayXCreditsStringChanged;
			xWaysString.StringChanged += OnXWaysStringChanged;
			xCreditsForAnteString.StringChanged += OnXCreditsForAnteString;
		}

		private void OnDisable()
		{
			playXCreditsString.StringChanged -= OnPlayXCreditsStringChanged;
			xWaysString.StringChanged -= OnXWaysStringChanged;
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
				|| nonBetMultiplierStakes.All(s => s.Key != Stake.Multiway)
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
			var multiwayBet = stakeCombination.Values[Stake.Multiway];
			var ante = stakeCombination.Values[Stake.AnteBet];

			if (!waysMapping || !waysMapping.GetMultiwayMapping().TryGetValue(multiwayBet, out var waysCount))
			{
				GameLog.Instance.Warn($"No ways mapping for multiway bet {multiwayBet}");
				waysCount = 0;
			}

			playXCreditsString.Arguments = new object[] { stakeCombination.TotalBet.Credits };
			playXCreditsString.RefreshString();
			xWaysString.Arguments = new object[] { waysCount };
			xWaysString.RefreshString();
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

		private void OnXWaysStringChanged(string s)
		{
			xWays.text = s;
		}

		private void OnXCreditsForAnteString(string s)
		{
			xCreditsForAnte.text = s;
		}
	}
}