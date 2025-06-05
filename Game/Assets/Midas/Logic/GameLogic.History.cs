using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Midas.Core;
using Midas.Core.LogicServices;
using Midas.Core.Serialization;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;

namespace Midas.Logic
{
	public partial class GameLogic
	{
		private const string HistoryStepNumberCriticalData = "HistoryStepNumber";
		private const string HistoryListCriticalData = "HistoryList";
		private const string HistoryPriorityListCriticalData = "HistoryPriorityList";
		private const string CreditPlayoffHistoryState = "CreditPlayoffHistoryState";
		private const string GameHistoryState = "GameHistoryState";
		private const string GambleHistoryState = "GambleHistoryState";
		private List<int> historyList;
		private List<int> historyPriorityList;
		private int currentHistoryStepIndex;
		private object logicStateHistoryData;
		private object gameStartHistoryData;
		private object creditPlayoffHistoryState;
		private object gameHistoryState;
		private object gambleHistoryState;
		private HistoryStep historyStepData;

		private sealed class HistoryStep
		{
			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(HistoryStep);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					var historyStep = (HistoryStep)o;
					writer.Write((byte)historyStep.HistoryStepType);
					serializeComplex(writer, historyStep.ServiceData);
					serializeComplex(writer, historyStep.GameData);
					serializeComplex(writer, historyStep.PresentationData);
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					var historyStepType = (HistoryStepType)reader.ReadByte();
					var serviceData = deserializeComplex(reader);
					var gameData = deserializeComplex(reader);
					var presentationData = (IReadOnlyDictionary<string, object>)deserializeComplex(reader);
					return new HistoryStep(historyStepType, serviceData, gameData, presentationData);
				}
			}

			static HistoryStep()
			{
				NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
			}

			#endregion

			public HistoryStep(HistoryStepType historyStepType, object serviceData, object gameData, IReadOnlyDictionary<string, object> presentationData)
			{
				HistoryStepType = historyStepType;
				ServiceData = serviceData;
				GameData = gameData;
				PresentationData = presentationData;
			}

			public HistoryStepType HistoryStepType { get; }
			public object ServiceData { get; }
			public object GameData { get; }
			public IReadOnlyDictionary<string, object> PresentationData { get; }
		}

		private void InitHistory()
		{
			historyList = null;
			historyPriorityList = null;
			currentHistoryStepIndex = 0;
		}

		private void ResetHistory()
		{
			// If the history lists exist, clear them.
			// If they don't exist then it doesn't matter since they will be refreshed anyway.

			historyList?.Clear();
			historyPriorityList?.Clear();
		}

		private void SaveHistoryStep(HistoryStepType historyStepType)
		{
			if (foundation.GameMode != FoundationGameMode.Play)
				return;

			// Save a history step.

			foundation.TryReadNvram(NvramScope.GameCycle, HistoryStepNumberCriticalData, out int historyStepIndex);

			if (historyStepIndex == 0)
			{
				logicStateHistoryData = (logicState.SelectedStakeCombinationIndex, logicState.Configuration, logicState.GameTime);
				gameStartHistoryData = GameServices.GetHistoryData(HistorySnapshotType.GameStart);
			}

			object gameCycleHistoryData;
			switch (historyStepType)
			{
				case HistoryStepType.CreditPlayoff:
					creditPlayoffHistoryState = creditPlayoff.GetHistoryState();
					gameHistoryState = game.GetHistoryState();
					gameCycleHistoryData = creditPlayoff.GetGameCycleHistoryData();
					break;
				case HistoryStepType.Game:
					gameHistoryState = game.GetHistoryState();
					gameCycleHistoryData = game.GetGameCycleHistoryData();
					break;
				case HistoryStepType.Gamble:
					gambleHistoryState = gamble.GetHistoryState();
					gameCycleHistoryData = gamble.GetGameCycleHistoryData();
					break;
				default:
					Log.Instance.Fatal($"Unsupported history step type {historyStepType}");
					throw new InvalidOperationException($"Unsupported history step type {historyStepType}");
			}

			historyStepData = new HistoryStep(historyStepType, GameServices.GetHistoryData(HistorySnapshotType.GameCycle), gameCycleHistoryData, GetHistoryPresentationData());
		}

		private void CommitHistoryStep()
		{
			if (foundation.GameMode != FoundationGameMode.Play)
				return;

			foundation.TryReadNvram(NvramScope.GameCycle, HistoryStepNumberCriticalData, out int historyStepIndex);

			if (historyStepIndex == 0)
			{
				foundation.WriteNvram(NvramScope.History, nameof(LogicState), logicStateHistoryData);
				foundation.WriteNvram(NvramScope.History, nameof(HistorySnapshotType.GameStart), gameStartHistoryData);
				logicStateHistoryData = null;
				gameStartHistoryData = null;
			}

			if (creditPlayoffHistoryState != null)
			{
				foundation.WriteNvram(NvramScope.History, CreditPlayoffHistoryState, creditPlayoffHistoryState);
				creditPlayoffHistoryState = null;
			}

			if (gameHistoryState != null)
			{
				foundation.WriteNvram(NvramScope.History, GameHistoryState, gameHistoryState);
				gameHistoryState = null;
			}

			if (gambleHistoryState != null)
			{
				foundation.WriteNvram(NvramScope.History, GameLogic.GambleHistoryState, gambleHistoryState);
				gambleHistoryState = null;
			}

			++historyStepIndex;
			foundation.WriteNvram(NvramScope.GameCycle, HistoryStepNumberCriticalData, historyStepIndex);
			foundation.WriteNvram(NvramScope.History, $"S{historyStepIndex}", historyStepData);
			historyStepData = null;

			// TODO: History priority - Do we need it?
			UpdateHistoryStepLists(historyStepIndex, 0);
		}

		private object LoadCreditPlayoffHistoryState()
		{
			if (foundation.GameMode != FoundationGameMode.History)
				return null;

			foundation.TryReadNvram(NvramScope.History, CreditPlayoffHistoryState, out object state);
			return state;
		}

		private object LoadGameHistoryState()
		{
			if (foundation.GameMode != FoundationGameMode.History)
				return null;

			foundation.TryReadNvram(NvramScope.History, GameHistoryState, out object state);
			return state;
		}

		private object LoadGambleHistoryState()
		{
			if (foundation.GameMode != FoundationGameMode.History)
				return null;

			foundation.TryReadNvram(NvramScope.History, GambleHistoryState, out object state);
			return state;
		}

		private HistoryStep LoadHistory()
		{
			LoadHistoryStepLists();
			var historyStepIndex = historyList[currentHistoryStepIndex];
			if (!foundation.TryReadNvram<HistoryStep>(NvramScope.History, $"S{historyStepIndex}", out var historyStep))
			{
				Log.Instance.FatalFormat("Unable to find history step {0}", historyStepIndex);
			}

			return historyStep;
		}

		private void UpdateHistoryStepLists(int historyStep, int historyStepPriority)
		{
			LoadHistoryStepLists();
			historyList.Add(historyStep);
			historyPriorityList.Add(historyStepPriority);

			foundation.WriteNvram(NvramScope.History, HistoryListCriticalData, ConvertHistoryList(historyList));
			foundation.WriteNvram(NvramScope.History, HistoryPriorityListCriticalData, ConvertHistoryList(historyPriorityList));
		}

		private void LoadHistoryStepLists()
		{
			historyList ??= foundation.TryReadNvram(NvramScope.History, HistoryListCriticalData, out var historyData)
				? ConvertHistoryList(historyData)
				: new List<int>();
			historyPriorityList ??= foundation.TryReadNvram(NvramScope.History, HistoryPriorityListCriticalData, out historyData)
				? ConvertHistoryList(historyData)
				: new List<int>();
		}

		private static byte[] ConvertHistoryList(IReadOnlyCollection<int> list)
		{
			// This is a very specific format so that the foundation can read it (Australian foundation does this)

			using var ms = new MemoryStream();
			using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
			{
				bw.Write(list.Count);
				foreach (var v in list)
					bw.Write(v);
			}

			return ms.ToArray();
		}

		private static List<int> ConvertHistoryList(byte[] array)
		{
			var result = new List<int>();
			using var br = new BinaryReader(new MemoryStream(array), Encoding.UTF8, false);

			for (var count = br.ReadInt32(); count > 0; count--)
				result.Add(br.ReadInt32());

			return result;
		}


	}
}