using System.Linq;
using Midas.Core;
using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class LinesButtonText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString playString;

		[SerializeField]
		private LocalizedString linesString;

		[SerializeField]
		private TMP_Text play;

		[SerializeField]
		private TMP_Text value;

		[SerializeField]
		private TMP_Text lines;

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		private void Awake()
		{
			linesString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			playString.StringChanged += OnPlayStringChanged;
			linesString.StringChanged += OnLinesStringChanged;
		}

		private void OnDisable()
		{
			playString.StringChanged -= OnPlayStringChanged;
			linesString.StringChanged -= OnLinesStringChanged;
		}

		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			if (!(stakeButtonSpecificData is PlayButtonSpecificData playButtonData))
				return false;

			if (playButtonData.IsCostToCover)
				return false;

			var d = playButtonData.StakeCombination.Values;
			var nonBetMultiplierStakes = d.Where(k => k.Key != Stake.BetMultiplier && k.Value != 0).ToArray();

			if (nonBetMultiplierStakes.Length != 1 || nonBetMultiplierStakes[0].Key != Stake.LinesBet)
				return false;

			UpdateFormattedText(playButtonData);

			Dimmer.Initialise(GetComponentsInChildren<TMP_Text>(false));
			Dimmer.SetDim(!isEnabled, true);
			return true;
		}

		private void UpdateFormattedText(PlayButtonSpecificData playButtonData)
		{
			var linesBet = playButtonData.StakeCombination.Values[Stake.LinesBet];

			value.text = linesBet.ToString();
			linesString.Arguments = new object[] { linesBet };
			linesString.RefreshString();
		}

		public override void UpdateEnabledState(bool enabledState)
		{
			Dimmer.SetDim(!enabledState);
		}

		private void OnPlayStringChanged(string s)
		{
			play.text = s;
		}

		private void OnLinesStringChanged(string s)
		{
			lines.text = s;
		}
	}
}