using System.Collections.Generic;
using System.Linq;
using IGT.Ascent.Assets.SimulateOperatorMenu;
using IGT.Ascent.Communication.Platform.Interfaces;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;

namespace Midas
{
	public sealed partial class GamingSystem : ISimulateOperatorMenuDependency
	{
		private ISimulateGameModeControl gameModeControl;

		bool ISimulateOperatorMenuDependency.CanEnterOperatorMenu()
		{
			return StatusDatabase.IsInitialised && StatusDatabase.GameStatus.GameIsIdle;
		}

		ISimulateGameModeControl ISimulateOperatorMenuDependency.GameModeControl => gameModeControl ??= new DemoGameModeControl();

		private sealed class DemoGameModeControl : ISimulateGameModeControl
		{
			public void EnterMode(GameMode nextMode)
			{
				switch (nextMode)
				{
					case GameMode.Play:
						Communication.ToLogicSender.Send(new DemoChangeModeMessage(FoundationGameMode.Play));
						break;
					case GameMode.History:
						Communication.ToLogicSender.Send(new DemoChangeModeMessage(FoundationGameMode.History));
						break;
					case GameMode.Utility:
						Communication.ToLogicSender.Send(new DemoChangeModeMessage(FoundationGameMode.Utility, UtilityTheme, UtilityPaytable, Money.FromMinorCurrency(UtilityDenomination)));
						break;
				}
			}

			public void ExitMode() => Log.Instance.Info("ExitMode - Foundation doesn't do this from operator menu!");

			public void ShutDown() => Log.Instance.Info("ShutDown - not supported yet");

			public int GetHistoryRecordCount() => StatusDatabase.HistoryStatus.HistoryRecordCount;

			public bool IsNextAvailable() => StatusDatabase.HistoryStatus.NextRecordAvailable;

			public bool IsPreviousAvailable() => StatusDatabase.HistoryStatus.PreviousRecordAvailable;

			public void NextHistoryRecord()
			{
				Communication.ToLogicSender.Send(new DemoChangeHistoryRecordMessage(DemoHistoryRecordChangeDirection.Next));
			}

			public void PreviousHistoryRecord()
			{
				Communication.ToLogicSender.Send(new DemoChangeHistoryRecordMessage(DemoHistoryRecordChangeDirection.Previous));
			}

			public IReadOnlyList<string> GetRegistrySupportedThemes()
			{
				return StatusDatabase.UtilityStatus.SupportedThemes;
			}

			public IReadOnlyDictionary<KeyValuePair<string, string>, IEnumerable<long>> GetRegistrySupportedDenominations(string theme)
			{
				return StatusDatabase.UtilityStatus.RegistrySupportedDenominations.ToDictionary(d => d.Key, d => d.Value.Select(p => p));
			}

			public bool IsUtilityModeEnabled { get { return true; } }
			public bool UtilitySelectionComplete { get; set; }
			public string UtilityTheme { get; set; }
			public KeyValuePair<string, string> UtilityPaytable { get; set; }
			public long UtilityDenomination { get; set; }
		}
	}
}