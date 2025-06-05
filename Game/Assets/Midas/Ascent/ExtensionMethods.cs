using System;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Core;

namespace Midas.Ascent
{
	internal static class ExtensionMethods
	{
		public static MoneyEvent ConvertToSdk(this MoneyEventType met)
		{
			return met switch
			{
				MoneyEventType.MoneyBet => MoneyEvent.MoneyBet,
				MoneyEventType.MoneyWon => MoneyEvent.MoneyWon,
				MoneyEventType.MoneyIn => MoneyEvent.MoneyIn,
				MoneyEventType.MoneyOut => MoneyEvent.MoneyOut,
				MoneyEventType.MoneySet => MoneyEvent.MoneySet,
				MoneyEventType.MoneyWagerable => MoneyEvent.MoneyWagerable,
				MoneyEventType.MoneyCommittedChanged => MoneyEvent.MoneyCommittedChanged,
				_ => throw new ArgumentOutOfRangeException(nameof(met), met, $"Unexpected Money Event Type: {met}")
			};
		}

		public static MoneySource ConvertToSdk(this MoneyInSource mis)
		{
			return mis switch
			{
				MoneyInSource.OtherSource => MoneySource.Other,
				MoneyInSource.Bill => MoneySource.Bill,
				MoneyInSource.Coin => MoneySource.Coin,
				MoneyInSource.Ticket => MoneySource.Ticket,
				MoneyInSource.FundsTransfer => MoneySource.FundsTransfer,
				_ => throw new ArgumentOutOfRangeException(nameof(mis), mis, $"Unexpected Money In Source: {mis}")
			};
		}

		public static MoneyTarget ConvertToSdk(this MoneyOutSource mos)
		{
			return mos switch
			{
				MoneyOutSource.OtherSource => MoneyTarget.Other,
				MoneyOutSource.Bill => MoneyTarget.Bill,
				MoneyOutSource.Coin => MoneyTarget.Coin,
				MoneyOutSource.Ticket => MoneyTarget.Ticket,
				MoneyOutSource.FundsTransfer => MoneyTarget.FundsTransfer,
				MoneyOutSource.Handpay => MoneyTarget.Handpay,
				_ => throw new ArgumentOutOfRangeException(nameof(mos), mos, $"Unexpected Money Out Source: {mos}")
			};
		}

		public static bool IsStandaloneBuild(this IgtGameParameters.GameType gameType)
		{
			return gameType == IgtGameParameters.GameType.StandaloneNoSafeStorage ||
				gameType == IgtGameParameters.GameType.StandaloneFileBackedSafeStorage ||
				gameType == IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage;
		}

		public static bool IsStandardGameBuild(this IgtGameParameters.GameType gameType)
		{
			return gameType == IgtGameParameters.GameType.Standard;
		}
	}
}