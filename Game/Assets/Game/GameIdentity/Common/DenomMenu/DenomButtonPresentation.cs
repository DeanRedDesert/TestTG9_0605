using System;
using Game.GameIdentity.Common;
using Midas.Core;
using Midas.Core.General;
using Midas.Logic;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Denom;
using Midas.Presentation.General;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.DenomMenu
{
	[RequireComponent(typeof(Animator))]
	public sealed class DenomButtonPresentation : ButtonPresentation
	{
		#region Static Fields

		private static readonly int stateHash = Animator.StringToHash("State");
		private static readonly int denomHash = Animator.StringToHash("Denom");

		#endregion

		private enum BetHintType
		{
			None,
			Lines,
			Ways,
			Bet
		}

		private Animator animator;
		private BetHintType betHintType;

		[SerializeField]
		private LocalizedString linesString;

		[SerializeField]
		private LocalizedString waysString;

		[SerializeField]
		private LocalizedString betString;

		[SerializeField]
		private TMP_Text denomText;

		[SerializeField]
		private TMP_Text betHintText;

		[SerializeField]
		private MultiwayWaysMapping waysMapping;

		[SerializeField]
		private float currencySymbolOffset;

		[SerializeField]
		private float minorCurrencySymbolOffset;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			linesString.Arguments = new object[] { 0L };
			waysString.Arguments = new object[] { 0L };
			betString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			linesString.StringChanged += OnLinesStringChanged;
			waysString.StringChanged += OnWaysStringChanged;
			betString.StringChanged += OnBetStringChanged;
		}

		private void OnDisable()
		{
			linesString.StringChanged -= OnLinesStringChanged;
			waysString.StringChanged -= OnWaysStringChanged;
			betString.StringChanged += OnBetStringChanged;
		}

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return true;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			if (buttonStateData != null)
			{
				var denomData = (DenomButtonSpecificData)buttonStateData.SpecificData;

				animator.SetInteger(stateHash, (int)denomData.DenomState);

				var cf = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter;
				denomText.text = cf.GetFormatted(MoneyAndCreditDisplayMode.MoneyBase, denomData.Denom, CreditDisplaySeparatorMode.Auto);
				ConfigureBetHint(denomData.DenomBetData);

				animator.SetInteger(denomHash, (int)denomData.DenomBetData.DenomLevel);
			}

			return;
		}

		private void ConfigureBetHint(IDenomBetData denomBetData)
		{
			if (denomBetData == null)
			{
				betHintType = BetHintType.None;
				return;
			}

			switch (denomBetData)
			{
				case { MaxLines: var l } when l > 0:
					betHintType = BetHintType.Lines;
					linesString.Arguments = new object[] { l };
					linesString.RefreshString();
					break;

				case { MaxMultiway: var mw } when mw > 0:
					if (!waysMapping)
						throw new InvalidOperationException("Please provide a ways mapping");

					betHintType = BetHintType.Ways;
					linesString.Arguments = new object[] { waysMapping.GetMultiwayMapping()[mw] };
					linesString.RefreshString();
					break;

				case { MaxBetAtMultiplierOne: var mb } when mb != Credit.Zero:
					betHintType = BetHintType.Ways;
					linesString.Arguments = new object[] { mb.Credits };
					linesString.RefreshString();
					break;

				default:
					betHintType = BetHintType.None;
					throw new ArgumentOutOfRangeException(nameof(denomBetData), "Bet data values are not supported.");
			}
		}

		private void OnLinesStringChanged(string value)
		{
			if (betHintType == BetHintType.Lines)
				betHintText.text = value;
		}

		private void OnWaysStringChanged(string value)
		{
			if (betHintType == BetHintType.Ways)
				betHintText.text = value;
		}

		private void OnBetStringChanged(string value)
		{
			if (betHintType == BetHintType.Bet)
				betHintText.text = value;
		}
	}
}