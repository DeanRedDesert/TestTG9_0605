using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Denom
{
	public enum DenomMenuState
	{
		Hidden,
		Attract,
		Confirm,
		WaitForChange
	}

	public sealed class DenomStatus : StatusBlock
	{
		private StatusProperty<DenomMenuState> denomMenuState;
		private StatusProperty<Money> selectedDenom;
		private StatusProperty<bool> showBetInfo;

		public DenomMenuState DenomMenuState
		{
			get => denomMenuState.Value;
		}

		public Money SelectedDenom
		{
			get => selectedDenom.Value;
			set => selectedDenom.Value = value;
		}

		public bool ShowBetInfo
		{
			get => showBetInfo.Value;
			set => showBetInfo.Value = value;
		}

		public DenomStatus() : base(nameof(DenomStatus))
		{
		}

		public void SetState(DenomMenuState newDenomMenuState)
		{
			denomMenuState.Value = newDenomMenuState;
			StatusDatabase.PopupStatus.Set(Popup.DenomMenu, denomMenuState.Value != DenomMenuState.Hidden);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			denomMenuState = AddProperty(nameof(DenomMenuState), DenomMenuState.Hidden);
			selectedDenom = AddProperty(nameof(SelectedDenom), default(Money));
			showBetInfo = AddProperty(nameof(ShowBetInfo), false);
		}
	}
}