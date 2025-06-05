using System;
using Midas.Core.General;

namespace Midas.Gamble.LogicToPresentation
{
	public enum TrumpsSelection
	{
		Decline,
		Red,
		Black,
		Heart,
		Diamond,
		Club,
		Spade
	}

	public enum TrumpsSuit
	{
		Heart,
		Diamond,
		Club,
		Spade
	}

	public enum TrumpsResult
	{
		Win,
		Loss
	}

	[Flags]
	public enum GambleCompleteReason
	{
		None = 0,
		Loss = 1,
		MoneyLimit = 2,
		CycleLimit = 4
	}

	public sealed class TrumpsCycleData
	{
		public TrumpsSelection Selection { get; }
		public TrumpsSuit Suit { get; }
		public TrumpsResult Result { get; }
		public Money WinAmount { get; }
		public GambleCompleteReason GambleCompleteReason { get; }

		public TrumpsCycleData(TrumpsSelection selection, TrumpsSuit suit, TrumpsResult result, Money winAmount, GambleCompleteReason gambleCompleteReason)
		{
			Selection = selection;
			Suit = suit;
			Result = result;
			WinAmount = winAmount;
			GambleCompleteReason = gambleCompleteReason;
		}

		public override string ToString() => $"Choice: {Selection}, Suit: {Suit}, Result: {Result}, Win: {WinAmount}, CompleteReason: {GambleCompleteReason}";
	}
}