using Midas.Presentation.Stakes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Common
{
	public sealed class CostToCoverText : StakeButtonTextBase
	{
		private TextAreaDimmer dimmer;

		[SerializeField]
		private LocalizedString playString;

		[SerializeField]
		private LocalizedString creditsString;

		[SerializeField]
		private TMP_Text play;

		[SerializeField]
		private TMP_Text value;

		[SerializeField]
		private TMP_Text credits;

		private TextAreaDimmer Dimmer => dimmer ? dimmer : dimmer = gameObject.AddComponent<TextAreaDimmer>();

		private void Awake()
		{
			creditsString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			playString.StringChanged += OnPlayStringChanged;
			creditsString.StringChanged += OnCreditsStringChanged;
		}

		private void OnDisable()
		{
			playString.StringChanged -= OnPlayStringChanged;
			creditsString.StringChanged -= OnCreditsStringChanged;
		}

		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			if (!(stakeButtonSpecificData is PlayButtonSpecificData playButtonData))
				return false;

			if (!playButtonData.IsCostToCover)
				return false;

			UpdateFormattedText(playButtonData);

			Dimmer.Initialise(GetComponentsInChildren<TMP_Text>(false));
			Dimmer.SetDim(!isEnabled, true);
			return true;
		}

		private void UpdateFormattedText(PlayButtonSpecificData playButtonData)
		{
			var cost = playButtonData.StakeCombination.TotalBet.Credits;

			value.text = cost.ToString();
			creditsString.Arguments = new object[] { cost };
			creditsString.RefreshString();
		}

		public override void UpdateEnabledState(bool enabledValue)
		{
			Dimmer.SetDim(!enabledValue);
		}

		private void OnPlayStringChanged(string s)
		{
			play.text = s;
		}

		private void OnCreditsStringChanged(string s)
		{
			credits.text = s;
		}
	}
}