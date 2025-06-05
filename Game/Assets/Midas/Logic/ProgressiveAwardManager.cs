using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Data.Services;
using Midas.LogicToPresentation.Messages;

namespace Midas.Logic
{
	internal static class ProgressiveAwardManager
	{
		private const string AwardDataNvram = "ProgressiveAwards";
		private static IFoundationShim foundation;
		private static IGameLogic gameLogic;
		private static ProgressiveAwardData awardData;
		private static IReadOnlyList<(string LevelId, Money Value)> lastBroadcastList;

		public static void Init(IFoundationShim foundationShim, IGameLogic gameLogicInst)
		{
			foundation = foundationShim;
			gameLogic = gameLogicInst;

			if (foundationShim.GameMode != FoundationGameMode.Play)
			{
				awardData = new ProgressiveAwardData();
				UpdateGameServices();
				return;
			}

			if (!foundationShim.TryReadNvram(NvramScope.Theme, AwardDataNvram, out awardData))
			{
				awardData = new ProgressiveAwardData();
				ApplyAwardDataChanges();
			}
			else
			{
				UpdateGameServices();
			}

			Communication.LogicDispatcher.AddHandler<StartProgressiveAwardMessage>(OnStartProgressiveAward);
			Communication.LogicDispatcher.AddHandler<ProgressiveDisplayCompleteMessage>(OnProgressiveDisplayComplete);
			Communication.LogicDispatcher.AddHandler<ClearProgressiveAwardMessage>(OnClearProgressiveAward);
		}

		public static void DeInit()
		{
			var f = foundation;
			foundation = null;
			gameLogic = null;

			if (f.GameMode != FoundationGameMode.Play)
				return;

			Communication.LogicDispatcher.RemoveHandler<StartProgressiveAwardMessage>(OnStartProgressiveAward);
			Communication.LogicDispatcher.RemoveHandler<ProgressiveDisplayCompleteMessage>(OnProgressiveDisplayComplete);
			Communication.LogicDispatcher.RemoveHandler<ClearProgressiveAwardMessage>(OnClearProgressiveAward);
		}

		/// <summary>
		/// Give progressive award an opportunity to adjust any broadcast data so that the presentation gets values that are pending award.
		/// </summary>
		/// <param name="broadcastList">The broadcast data from the foundation.</param>
		/// <returns>The adjusted broadcast data.</returns>
		public static IReadOnlyList<(string LevelId, Money Value)> GetBroadcastData(IReadOnlyList<(string LevelId, Money Value)> broadcastList)
		{
			lastBroadcastList = broadcastList;

			if (awardData.Awards.Length == 0 || awardData.Awards.All(a => a.State == ProgressiveAwardState.Cleared))
				return broadcastList;

			var initHitIndex = awardData.HitIndex == -1 ? 0 : awardData.HitIndex;
			var newBroadcastList = new List<(string LevelId, Money Value)>();
			foreach (var broadcast in broadcastList)
			{
				var hitFound = false;
				for (var i = initHitIndex; i < awardData.Awards.Length; i++)
				{
					var hit = awardData.Awards[i].Hit;
					if (hit.LevelId != broadcast.LevelId)
						continue;

					newBroadcastList.Add((broadcast.LevelId, hit.Amount));
					hitFound = true;
					break;
				}

				if (!hitFound)
					newBroadcastList.Add(broadcast);
			}

			return newBroadcastList;
		}

		/// <summary>
		/// Save any potential hits from the result. These will be cleared next time SetHits is called.
		/// </summary>
		/// <param name="foundationPrizes">The foundation prize list.</param>
		/// <param name="isNewGame">Is this the first call after starting a new game.</param>
		public static void SetPotentialHits(IReadOnlyList<IFoundationPrize> foundationPrizes, bool isNewGame)
		{
			var potentialHits = new List<ProgressiveAwardServiceData>();
			foreach (var prize in foundationPrizes)
			{
				var ph = prize.ProgressiveHit;
				if (ph != null)
				{
					var lastBroadcast = lastBroadcastList.Single(p => p.LevelId == ph.LevelId);
					var hit = new ProgressiveHit(ph.LevelId, lastBroadcast.Value, ph.SourceName, ph.SourceDetails);
					potentialHits.Add(CreateData(hit, ProgressiveAwardState.Pending, lastBroadcast.Value));
				}
			}

			if (isNewGame)
				awardData.TotalProgressivesAwarded = Money.Zero;

			awardData.Awards = potentialHits.ToArray();
			ApplyAwardDataChanges();
		}

		public static void SetHits(IReadOnlyList<ProgressiveHit> hits)
		{
			Log.Instance.InfoFormat("Progressive award with {0} hits", hits.Count);

			// New hits means reset the progressive award.

			if (hits.Count == 0)
			{
				awardData.HitIndex = -1;
				awardData.Awards = Array.Empty<ProgressiveAwardServiceData>();
			}
			else
			{
				awardData.HitIndex = 0;
				awardData.Awards = hits.Select(h => CreateData(h, ProgressiveAwardState.Triggered, h.Amount)).ToArray();
			}

			ApplyAwardDataChanges();
		}

		private static void OnStartProgressiveAward(StartProgressiveAwardMessage msg)
		{
			Log.Instance.InfoFormat("Progressive award start message received");

			if (awardData.HitIndex == -1)
			{
				var message = "There is no progressive available to award.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			var ca = awardData.Awards[awardData.HitIndex];
			awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.Starting, ca.DisplayAmount);
			ApplyAwardDataChanges();

			// Do this last because it can cause the "Verified" message may be sent synchronously.

			foundation.StartProgressiveAward(awardData.HitIndex, ca.Hit.LevelId, ca.DisplayAmount);
		}

		public static void SetVerified(int awardIndex, string levelId, ProgressiveAwardPayType payType, Money verifiedAmount)
		{
			Log.Instance.InfoFormat("Progressive hit {0} verified: {1} {2}, {3}", awardIndex, levelId, verifiedAmount, payType);

			if (awardData.HitIndex == -1)
			{
				Log.Instance.Fatal("There is no progressive award in progress.");
				throw new InvalidOperationException("There is no progressive award in progress.");
			}

			var ca = awardData.Awards[awardData.HitIndex];
			if (awardIndex != awardData.HitIndex)
			{
				var message = $"Progressive level {awardIndex} is not the currently awarding progressive.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			if (ca.State != ProgressiveAwardState.Starting)
			{
				var message = $"Progressive award received verification message in {ca.State} state.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.Verified, verifiedAmount);
			ApplyAwardDataChanges();
		}

		private static void OnProgressiveDisplayComplete(ProgressiveDisplayCompleteMessage msg)
		{
			Log.Instance.InfoFormat("Progressive award display complete message received");

			if (awardData.HitIndex == -1)
			{
				var message = "There is no progressive available to award.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			var ca = awardData.Awards[awardData.HitIndex];

			awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.FinishedDisplay, ca.DisplayAmount);
			ApplyAwardDataChanges();

			// Do this last because it can cause the "Paid" message may be sent synchronously.

			foundation.FinishedProgressiveAwardDisplay(awardData.HitIndex, ca.Hit.LevelId, ca.DisplayAmount);
		}

		public static void SetPaid(int awardIndex, string levelId, Money paidAmount)
		{
			Log.Instance.InfoFormat("Progressive hit {0} paid: {1} {2}", awardIndex, levelId, paidAmount);

			if (awardData.HitIndex == -1)
			{
				Log.Instance.Fatal("There is no progressive award in progress.");
				throw new InvalidOperationException("There is no progressive award in progress.");
			}

			var ca = awardData.Awards[awardData.HitIndex];
			if (awardIndex != awardData.HitIndex)
			{
				var message = $"Progressive level {awardIndex} is not the currently awarding progressive.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			if (ca.State != ProgressiveAwardState.FinishedDisplay)
			{
				var message = $"Progressive award received paid message in {ca.State} state.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			awardData.TotalProgressivesAwarded += paidAmount;
			awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.Paid, paidAmount);
			ApplyAwardDataChanges();
		}

		private static void OnClearProgressiveAward(ClearProgressiveAwardMessage obj)
		{
			Log.Instance.InfoFormat("Clear progressive award message received");

			if (awardData.HitIndex == -1)
			{
				var message = "There is no progressive available to award.";
				Log.Instance.Fatal(message);
				throw new InvalidOperationException(message);
			}

			var ca = awardData.Awards[awardData.HitIndex];
			awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.Cleared, ca.DisplayAmount);
			awardData.HitIndex++;

			if (awardData.HitIndex == awardData.Awards.Length)
			{
				awardData.HitIndex = -1;
			}
			else
			{
				var hit = awardData.Awards[awardData.HitIndex];
				awardData.Awards[awardData.HitIndex] = CreateData(ca.Hit, ProgressiveAwardState.Triggered, hit.Hit.Amount);
			}

			ApplyAwardDataChanges();

			// Refresh broadcast data.

			gameLogic.SetProgressiveValues(lastBroadcastList);
		}

		public static ProgressiveAwardWaitState GetProgressiveAwardWaitState(out int awardIndex)
		{
			awardIndex = awardData.HitIndex;
			if (awardIndex == -1)
				return ProgressiveAwardWaitState.None;

			return awardData.Awards[awardData.HitIndex]?.State switch
			{
				ProgressiveAwardState.Starting => ProgressiveAwardWaitState.Verification,
				ProgressiveAwardState.FinishedDisplay => ProgressiveAwardWaitState.Payment,
				_ => ProgressiveAwardWaitState.None
			};
		}

		private static void ApplyAwardDataChanges()
		{
			foundation.WriteNvram(NvramScope.Theme, AwardDataNvram, awardData);
			UpdateGameServices();
		}

		private static void UpdateGameServices()
		{
			GameServices.ProgressiveService.TotalProgressiveAwardInGameService.SetValue(awardData.TotalProgressivesAwarded);
			GameServices.ProgressiveService.ProgressiveAwardsService.SetValue(awardData.Awards.ToArray());
		}

		private static ProgressiveAwardServiceData CreateData(ProgressiveHit hit, ProgressiveAwardState state, Money amount)
		{
			return new ProgressiveAwardServiceData(hit, state, amount, IsDisplayAmountVerified(state));
		}

		private static bool IsDisplayAmountVerified(ProgressiveAwardState state)
		{
			return foundation.FoundationType switch
			{
				FoundationType.Ugp => state != ProgressiveAwardState.Pending && state != ProgressiveAwardState.Triggered && state != ProgressiveAwardState.Starting,
				FoundationType.Ascent => state != ProgressiveAwardState.Pending,
				_ => state != ProgressiveAwardState.Pending
			};
		}

		private sealed class ProgressiveAwardData
		{
			public ProgressiveAwardServiceData[] Awards = Array.Empty<ProgressiveAwardServiceData>();
			public int HitIndex = -1;
			public Money TotalProgressivesAwarded;

			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(ProgressiveAwardData);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					var d = (ProgressiveAwardData)o;

					serializeComplex(writer, d.Awards);
					writer.Write(d.HitIndex);
					serializeComplex(writer, d.TotalProgressivesAwarded);
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					var awards = (ProgressiveAwardServiceData[])deserializeComplex(reader);
					var hitIndex = reader.ReadInt32();
					var total = (Money)deserializeComplex(reader);

					return new ProgressiveAwardData
					{
						Awards = awards,
						HitIndex = hitIndex,
						TotalProgressivesAwarded = total
					};
				}
			}

			static ProgressiveAwardData()
			{
				NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
			}

			#endregion
		}
	}
}