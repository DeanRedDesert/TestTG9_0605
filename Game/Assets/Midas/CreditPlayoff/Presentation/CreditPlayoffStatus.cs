using Midas.Core.General;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.ExtensionMethods;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffStatus : CreditPlayoffStatusBase
	{
		private StatusProperty<bool> isReadyToPlay;
		private StatusProperty<CreditPlayoffState> state;
		private StatusProperty<long> weight;
		private StatusProperty<long> totalWeight;
		private StatusProperty<long> result;
		private StatusProperty<Money> bet;

		public override bool IsReadyToPlay => isReadyToPlay.Value;
		public override bool IsWin => State == CreditPlayoffState.Win;
		public override bool IsAvailable => State == CreditPlayoffState.Available || State == CreditPlayoffState.Idle;
		public override bool IsPlaying => State == CreditPlayoffState.Committed || State == CreditPlayoffState.Win || State == CreditPlayoffState.Loss;

		public CreditPlayoffState State => state.Value;
		public long Weight => weight.Value;
		public long TotalWeight => totalWeight.Value;
		public long Result => result.Value;
		public override Money Bet => bet.Value;

		public CreditPlayoffStatus() : base(nameof(CreditPlayoffStatus))
		{
		}

		public void SetPlayAllowed(bool value) => isReadyToPlay.Value = value;

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(CreditPlayoffService.Instance.State, OnStateChange);
			unregisterHelper.RegisterGameServiceChangedHandler(CreditPlayoffService.Instance.Cash, v => weight.Value = v.AsMinorCurrency);
			unregisterHelper.RegisterGameServiceChangedHandler(CreditPlayoffService.Instance.Bet, v =>
			{
				totalWeight.Value = v.AsMinorCurrency;
				bet.Value = v;
			});
			unregisterHelper.RegisterGameServiceChangedHandler(CreditPlayoffService.Instance.ResultNumber, v => result.Value = v);
		}

		private void OnStateChange(CreditPlayoffState value)
		{
			state.Value = value;
			StatusDatabase.PopupStatus.Set(Popup.CreditPlayoff, value != CreditPlayoffState.Available && value != CreditPlayoffState.Unavailable);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			isReadyToPlay = AddProperty(nameof(IsReadyToPlay), false);
			state = AddProperty(nameof(State), default(CreditPlayoffState));
			weight = AddProperty(nameof(Weight), default(long));
			totalWeight = AddProperty(nameof(TotalWeight), default(long));
			result = AddProperty(nameof(Result), default(long));
			bet = AddProperty(nameof(Bet), default(Money));
		}
	}
}