using System.Collections.Generic;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.WinPresentation
{
	public enum VisibilityType
	{
		HiddenBecauseNotActive,
		HiddenBecauseFlashing,
		HiddenBecauseWaitBetweenWins,
		HiddenBecauseWaitBetweenCycles,
		Visible,
	}

	public sealed class DetailedWinPresStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyList<IWinInfo>> wins;
		private StatusProperty<bool> isActive;
		private StatusProperty<int> highlightedWinIndex;
		private StatusProperty<IWinInfo> highlightedWin;
		private StatusProperty<bool> firstCycleComplete;
		private StatusProperty<VisibilityType> visibility;

		public IReadOnlyList<IWinInfo> Wins { get => wins.Value; set => wins.Value = value; }
		public bool IsActive { get => isActive.Value; set => isActive.Value = value; }
		public int HighlightedWinIndex { get => highlightedWinIndex.Value; set => highlightedWinIndex.Value = value; }
		public IWinInfo HighlightedWin { get => highlightedWin.Value; set => highlightedWin.Value = value; }
		public VisibilityType Visibility { get => visibility.Value; set => visibility.Value = value; }
		public bool FirstCycleComplete { get => firstCycleComplete.Value; set => firstCycleComplete.Value = value; }

		public DetailedWinPresStatus() : base(nameof(DetailedWinPresStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			wins = AddProperty(nameof(Wins), default(IReadOnlyList<IWinInfo>));
			isActive = AddProperty(nameof(IsActive), default(bool));
			highlightedWinIndex = AddProperty(nameof(HighlightedWinIndex), default(int));
			highlightedWin = AddProperty(nameof(HighlightedWin), default(IWinInfo));
			visibility = AddProperty(nameof(Visibility), VisibilityType.HiddenBecauseNotActive);
			firstCycleComplete = AddProperty(nameof(FirstCycleComplete), default(bool));
		}
	}
}