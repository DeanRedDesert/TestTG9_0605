using System;
using System.Collections.Generic;
using Midas.Core.General;
using Midas.Gle.Presentation;
using Midas.Presentation.Data;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.General;
using Midas.Presentation.Reels;
using Midas.Presentation.Symbols;
using TMPro;
using UnityEngine;

namespace Game.Stages.Common.Symbols
{
	[RequireComponent(typeof(ReelSymbol))]
	public sealed class CashOnReelsValueUpdater : MonoBehaviour
	{
		private static readonly Money denomThreshold = Money.FromRationalNumber(1, 1);
		private static readonly Money prizeValueThreshold = Money.FromRationalNumber(1000, 1);

		private ReelSymbol symbol;

		[SerializeField]
		private TMP_Text text;

		[SerializeField]
		private PropertyReference<IReadOnlyDictionary<string, long>> symbolToValue;

		private void Awake()
		{
			symbol = GetComponent<ReelSymbol>();
		}

		private void OnEnable()
		{
			UpdatePrizes();
		}

		private void OnDisable()
		{
			symbolToValue.DeInit();
		}

		private void UpdatePrizes()
		{
			if (!symbol.Reel)
				return;

			// Total bet in idle is the total bet value of the most recent completed game, otherwise it's the current stake combination.
			// This is important because the current game can be a different total bet, and you need to initialise your reels
			// on interruption to what they were based off the previous stake combination.

			var totalBet = symbol.Reel.SpinState == ReelSpinState.Idle
				? GleGameController.GleStatus.GetInitStakeCombination().TotalBet.Value
				: GameStatus.SelectedStakeCombination.TotalBet.Value;

			// ReSharper disable once RedundantAssignment - Unity requirement
			var symbolValue = 0L;

			if (symbolToValue.Value?.TryGetValue(symbol.SymbolId, out symbolValue) != true)
			{
				text.text = $"{symbol.SymbolId}\n$UNK";
				return;
			}

			var prizeValue = new RationalNumber(symbolValue, 1) * totalBet;
			var prizeValueCredits = Credit.FromRationalNumber(prizeValue.Numerator, prizeValue.Denominator);

			var formatMode = MoneyAndCreditDisplayMode.Credit;
			if (StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination >= denomThreshold || Money.FromCredit(prizeValueCredits) >= prizeValueThreshold)
				formatMode = MoneyAndCreditDisplayMode.MoneyWhole;

			text.text = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter.GetFormatted(formatMode, prizeValueCredits, CreditDisplaySeparatorMode.Auto);
		}
	}
}