using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents;
using Midas.Core;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IRuntimeGameEvents runTimeGameEvents;

		private void InitRuntimeGameEvents() => runTimeGameEvents = GameLib.GetInterface<IRuntimeGameEvents>();

		private void DeInitRuntimeGameEvents() => runTimeGameEvents = null;

		///<summary>
		/// Informs the foundation that the input is finished.
		///</summary>
		///<param name="value">True if it is waiting for input, false if the waiting is finished.</param>
		public void WaitingForGenericInput(bool value)
		{
			if (runTimeGameEvents == null || !runTimeGameEvents.RuntimeGameEventsConfiguration.WaitingForGenericInputStatusUpdateEnabled)
				return;

			if (value)
				runTimeGameEvents.WaitingForGenericInputStarted();
			else
				runTimeGameEvents.WaitingForGenericInputEnded();
		}

		/// <summary>
		/// Informs the foundation of the player choice index.
		/// </summary>
		/// <param name="playerChoiceIndex">The index of the player choice.</param>
		public void PlayerChoice(uint playerChoiceIndex)
		{
			if (runTimeGameEvents == null || !runTimeGameEvents.RuntimeGameEventsConfiguration.PlayerChoiceUpdateEnabled)
				return;

			runTimeGameEvents.PlayerChoiceUpdate(playerChoiceIndex);
		}

		/// <summary>
		/// Informs the foundation that denom selection is active.
		/// </summary>
		/// <param name="active">True if active, otherwise false.</param>
		public void DenomSelectionActive(bool active)
		{
			if (runTimeGameEvents == null || !runTimeGameEvents.RuntimeGameEventsConfiguration.GameSelectionStatusUpdateEnabled)
				return;

			if (active)
				runTimeGameEvents.GameSelectionEntered();
			else
				runTimeGameEvents.GameSelectionExited();
		}

		/// <summary>
		/// Send the bet information to the foundation.
		/// </summary>
		/// <param name="stakeCombination">The selected stake combination.</param>
		public void SendBetInformation(IStakeCombination stakeCombination)
		{
			if (runTimeGameEvents == null)
				return;

			if (runTimeGameEvents.RuntimeGameEventsConfiguration.GameBetMeterUpdateEnabled)
			{
				var (hKey, vKey) = BetInformation.GetBetBuckets(stakeCombination);
				runTimeGameEvents.GameBetMeterKeysUpdate(hKey, vKey);
			}

			if (!runTimeGameEvents.RuntimeGameEventsConfiguration.GameBetMeterKeysUpdateEnabled)
				return;

			var (betMultiplier, betValue) = BetInformation.GetBetValues(stakeCombination);
			runTimeGameEvents.GameBetMeterUpdate(betMultiplier, (uint)betValue);
		}

		#region Bet Buckets

		private static class BetInformation
		{
			private static readonly Stake[] betContributingLogicInputs = { Stake.LinesBet, Stake.Multiway, Stake.AnteBet };
			private static readonly Stake[] betElementLogicInputs = { Stake.LinesBet, Stake.Multiway };

			private static readonly Dictionary<Stake, string> betBucketFormatStrings = new Dictionary<Stake, string>
			{
				{ Stake.BetMultiplier, "Bet {0}" },
				{ Stake.LinesBet, "{0} Line{1}" },
				{ Stake.Multiway, "Multiway {0}" },
				{ Stake.AnteBet, "+ {0} AnteBet" }
			};

			public static (string HorizontalKey, string VerticalKey) GetBetBuckets(IStakeCombination stakeCombination)
			{
				var bm = stakeCombination.Values.SingleOrDefault(v => v.Key == Stake.BetMultiplier);
				var v = stakeCombination.Values;
				var horizontalKey = string.Format(betBucketFormatStrings[Stake.BetMultiplier], bm.Value, bm.Value == 1 ? "" : "s");
				var verticalKey = string.Join(" ", betContributingLogicInputs.Where(o => v.ContainsKey(o) && v[o] != 0).Select(o => string.Format(betBucketFormatStrings[o], v[o], v[o] == 1 ? "" : "s")));

				return (horizontalKey, verticalKey);
			}

			public static (long BetMult, int BetValue) GetBetValues(IStakeCombination stakeCombination)
			{
				var betMultiplier = stakeCombination.Values.SingleOrDefault(v => v.Key == Stake.BetMultiplier).Value;
				var betValue = betElementLogicInputs.Where(o => stakeCombination.Values.ContainsKey(o)).Sum(o => stakeCombination.Values[o]);

				return (betMultiplier, (int)betValue);
			}
		}

		#endregion
	}
}