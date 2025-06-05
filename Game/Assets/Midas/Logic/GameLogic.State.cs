using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.LogicToPresentation.Data;

namespace Midas.Logic
{
	public partial class GameLogic
	{
		private const string PresentationDataName = "PresentationData";

		private enum LogicStage
		{
			CreditPlayoff,
			GameStart,
			GameFeature
		}

		private enum ChangeDenomState
		{
			None,
			InMenu,
			Changing
		}

		private LogicState logicState;
		private bool isGameOverMessageVisible;
		private ChangeDenomState changeDenomState;
		private bool isChooserRequested;
		private LogicStage logicStage;
		private Dictionary<string, (bool HistoryRequired, object Data)> presentationData;

		private sealed class LogicState
		{
			public int SelectedStakeCombinationIndex;
			public ConfigData Configuration;

			public Money TotalProgressiveAwardedValue;
			public bool HasWinCapBeenReached;

			public DateTime GameTime;

			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(LogicState);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					var logicState = (LogicState)o;
					writer.Write(logicState.SelectedStakeCombinationIndex);
					serializeComplex(writer, logicState.Configuration);
					serializeComplex(writer, logicState.TotalProgressiveAwardedValue);
					writer.Write(logicState.HasWinCapBeenReached);
					writer.Write(logicState.GameTime.Ticks);
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					var result = new LogicState();
					result.SelectedStakeCombinationIndex = reader.ReadInt32();
					result.Configuration = (ConfigData)deserializeComplex(reader);
					result.TotalProgressiveAwardedValue = (Money)deserializeComplex(reader);
					result.HasWinCapBeenReached = reader.ReadBoolean();
					result.GameTime = new DateTime(reader.ReadInt64());
					return result;
				}
			}

			static LogicState()
			{
				NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
			}

			#endregion
		}

		private void InitLogicState()
		{
			switch (foundation.GameMode)
			{
				case FoundationGameMode.Play:
					if (!foundation.TryReadNvram(NvramScope.Variation, nameof(LogicState), out logicState))
					{
						logicState = new LogicState
						{
							SelectedStakeCombinationIndex = 0,
							TotalProgressiveAwardedValue = Money.Zero,
							GameTime = DateTime.Now
						};
						SaveLogicState();
					}

					// Re-read configuration in play mode.

					logicState.Configuration = foundation.ReadConfiguration();

					break;

				case FoundationGameMode.History:
					if (!foundation.TryReadNvram(NvramScope.History, nameof(LogicState), out (int StakeCombo, ConfigData Config, DateTime GameTime) logicStateData))
					{
						Log.Instance.Fatal("Could not find history state");
						throw new Exception("Could not find history state");
					}

					logicState = new LogicState
					{
						SelectedStakeCombinationIndex = logicStateData.StakeCombo,
						Configuration = logicStateData.Config,
						HasWinCapBeenReached = false,
						TotalProgressiveAwardedValue = Money.Zero,
						GameTime = logicStateData.GameTime
					};

					break;

				case FoundationGameMode.Utility:
					logicState.Configuration = foundation.ReadConfiguration();
					break;
			}
		}

		private void SaveLogicState()
		{
			if (foundation.GameMode == FoundationGameMode.Play)
				foundation.WriteNvram(NvramScope.Variation, nameof(LogicState), logicState);
		}

		private void UpdateGameOverState(bool? newValue = null)
		{
			switch (foundation.GameMode)
			{
				case FoundationGameMode.Play:
					if (newValue.HasValue)
					{
						isGameOverMessageVisible = newValue.Value;
						foundation.WriteNvram(NvramScope.Variation, nameof(isGameOverMessageVisible), isGameOverMessageVisible);
					}
					else
					{
						foundation.TryReadNvram(NvramScope.Variation, nameof(isGameOverMessageVisible), out isGameOverMessageVisible);
					}

					break;

				default:
					isGameOverMessageVisible = false;
					break;
			}

			GameServices.MachineStateService.ShowGameOverMessageService.SetValue(isGameOverMessageVisible);
		}

		private void UpdateChangeDenomState(ChangeDenomState? newValue = null)
		{
			switch (foundation.GameMode)
			{
				case FoundationGameMode.Play:
					if (newValue.HasValue)
					{
						changeDenomState = newValue.Value;
						foundation.WriteNvram(NvramScope.Theme, nameof(changeDenomState), changeDenomState);
					}
					else
					{
						foundation.TryReadNvram(NvramScope.Theme, nameof(changeDenomState), out changeDenomState);
					}

					break;

				default:
					changeDenomState = ChangeDenomState.None;
					break;
			}
		}

		private void UpdateChooserRequestedState(bool? newValue = null)
		{
			switch (foundation.GameMode)
			{
				case FoundationGameMode.Play:
					if (newValue.HasValue)
					{
						isChooserRequested = newValue.Value;
						foundation.WriteNvram(NvramScope.Theme, nameof(isChooserRequested), isChooserRequested);
					}
					else
					{
						foundation.TryReadNvram(NvramScope.Theme, nameof(isChooserRequested), out isChooserRequested);
					}

					break;

				default:
					isChooserRequested = false;
					break;
			}
		}

		private void UpdateLogicStage(LogicStage? newValue = null)
		{
			if (foundation.GameMode == FoundationGameMode.Play)
			{
				if (newValue.HasValue)
				{
					logicStage = newValue.Value;
					foundation.WriteNvram(NvramScope.Variation, nameof(logicStage), logicStage);
				}
				else
				{
					foundation.TryReadNvram(NvramScope.Variation, nameof(logicStage), out logicStage);
				}
			}
			else if (foundation.GameMode == FoundationGameMode.Utility)
			{
				if (newValue.HasValue)
					logicStage = newValue.Value;
			}
		}

		private void InitPresentationData()
		{
			switch (foundation.GameMode)
			{
				case FoundationGameMode.Play:
					if (!foundation.TryReadNvram(NvramScope.Variation, nameof(PresentationDataName), out presentationData))
						presentationData = new Dictionary<string, (bool, object)>();
					break;

				default:
					presentationData = new Dictionary<string, (bool, object)>();
					break;
			}
		}

		private void SavePresentationData()
		{
			foundation.WriteNvram(NvramScope.Variation, nameof(PresentationDataName), presentationData, 2000);
		}

		private void LoadHistoryPresentationData(IReadOnlyDictionary<string, object> historyData)
		{
			presentationData = historyData.ToDictionary(
				kvp => kvp.Key,
				kvp => (true, kvp.Value));
		}

		private IReadOnlyDictionary<string, object> GetHistoryPresentationData()
		{
			return presentationData
				.Where(kvp => kvp.Value.HistoryRequired)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Data);
		}
	}
}