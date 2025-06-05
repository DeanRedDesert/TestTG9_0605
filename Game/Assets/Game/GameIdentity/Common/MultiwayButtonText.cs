using System.Linq;
using Midas.Core;
using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class MultiwayButtonText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString playString;

		[SerializeField]
		private LocalizedString waysString;

		[SerializeField]
		private LocalizedString creditsString;

		[SerializeField]
		private TMP_Text play;

		[SerializeField]
		private TMP_Text ways;

		[SerializeField]
		private TMP_Text credits;

		[SerializeField]
		private MultiwayWaysMapping waysMapping;

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		private void Awake()
		{
			waysString.Arguments = new object[] { 0L };
			creditsString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			playString.StringChanged += OnPlayStringChanged;
			waysString.StringChanged += OnWaysStringChanged;
			creditsString.StringChanged += OnCreditsStringChanged;
		}

		private void OnDisable()
		{
			playString.StringChanged -= OnPlayStringChanged;
			waysString.StringChanged -= OnWaysStringChanged;
			creditsString.StringChanged -= OnCreditsStringChanged;
		}

		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			if (!(stakeButtonSpecificData is PlayButtonSpecificData playButtonData))
				return false;

			if (playButtonData.IsCostToCover)
				return false;

			var d = playButtonData.StakeCombination.Values;
			var nonBetMultiplierStakes = d.Where(k => k.Key != Stake.BetMultiplier && k.Value != 0).ToArray();

			if (nonBetMultiplierStakes.Length != 1 || nonBetMultiplierStakes[0].Key != Stake.Multiway)
				return false;

			UpdateFormattedText(playButtonData);

			Dimmer.Initialise(GetComponentsInChildren<TMP_Text>(false));
			Dimmer.SetDim(!isEnabled, true);
			return true;
		}

		private void UpdateFormattedText(PlayButtonSpecificData playButtonData)
		{
			var stakeCombination = playButtonData.StakeCombination;
			var multiwayBet = stakeCombination.Values[Stake.Multiway];

			if (!waysMapping || !waysMapping.GetMultiwayMapping().TryGetValue(multiwayBet, out var waysCount))
			{
				GameLog.Instance.Warn($"No ways mapping for multiway bet {multiwayBet}");
				waysCount = 0;
			}

			var cost = stakeCombination.TotalBet.Credits;
			if (stakeCombination.Values.TryGetValue(Stake.BetMultiplier, out var stakeMultiplier))
				cost /= stakeMultiplier;

			waysString.Arguments = new object[] { waysCount };
			waysString.RefreshString();
			creditsString.Arguments = new object[] { cost };
			creditsString.RefreshString();
		}

		public override void UpdateEnabledState(bool enabledState)
		{
			Dimmer.SetDim(!enabledState);
		}

		private void OnPlayStringChanged(string s)
		{
			play.text = s;
		}

		private void OnWaysStringChanged(string s)
		{
			ways.text = s;
		}

		private void OnCreditsStringChanged(string s)
		{
			credits.text = s;
		}
	}
}