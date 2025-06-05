using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;

namespace Midas.Presentation.History
{
	public sealed class HistoryStatus : StatusBlock
	{
		private StatusProperty<bool> isHistoryActive;
		private StatusProperty<bool> refreshRequired;
		private StatusProperty<int> historyStepCount;
		private StatusProperty<int> historyStepIndex;
		private StatusProperty<HistoryStepType> historyStepType;
		private StatusProperty<string> historyStepDescription;

		private StatusProperty<int> historyRecordCount;
		private StatusProperty<bool> nextRecordAvailable;
		private StatusProperty<bool> previousRecordAvailable;

		public bool IsHistoryActive
		{
			get => isHistoryActive.Value;
		}

		public bool RefreshRequired
		{
			get => refreshRequired.Value;
			set => refreshRequired.Value = value;
		}

		public int HistoryStepCount
		{
			get => historyStepCount.Value;
			set => historyStepCount.Value = value;
		}

		public int HistoryStepIndex
		{
			get => historyStepIndex.Value;
			set => historyStepIndex.Value = value;
		}

		public HistoryStepType HistoryStepType
		{
			get => historyStepType.Value;
			set => historyStepType.Value = value;
		}

		public string HistoryStepDescription
		{
			get => historyStepDescription.Value;
			set => historyStepDescription.Value = value;
		}

		public int HistoryRecordCount
		{
			get => historyRecordCount.Value;
			set => historyRecordCount.Value = value;
		}

		public bool NextRecordAvailable
		{
			get => nextRecordAvailable.Value;
			set => nextRecordAvailable.Value = value;
		}

		public bool PreviousRecordAvailable
		{
			get => previousRecordAvailable.Value;
			set => previousRecordAvailable.Value = value;
		}

		public HistoryStatus() : base(nameof(HistoryStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.GameMode), (_, __) => isHistoryActive.Value = StatusDatabase.GameStatus.GameMode == FoundationGameMode.History);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			isHistoryActive = AddProperty(nameof(IsHistoryActive), false);
			refreshRequired = AddProperty(nameof(RefreshRequired), false);
			historyStepCount = AddProperty(nameof(HistoryStepCount), 0);
			historyStepIndex = AddProperty(nameof(HistoryStepIndex), 0);
			historyStepType = AddProperty(nameof(HistoryStepType), HistoryStepType.Game);
			historyStepDescription = AddProperty<string>(nameof(HistoryStepDescription), null);

			historyRecordCount = AddProperty(nameof(HistoryRecordCount), 0);
			nextRecordAvailable = AddProperty(nameof(NextRecordAvailable), false);
			previousRecordAvailable = AddProperty(nameof(PreviousRecordAvailable), false);
		}
	}
}