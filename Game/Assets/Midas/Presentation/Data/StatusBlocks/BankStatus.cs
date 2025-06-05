using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class BankStatus : StatusBlock
	{
		private StatusProperty<bool> isPlayerWagerAvailable;
		private StatusProperty<bool> isCashoutAvailable;
		private StatusProperty<Money> bankMeter;
		private StatusProperty<Money> wagerableMeter;
		private StatusProperty<Money> paidMeter;
		private StatusProperty<Money> winMeter;
		private StatusProperty<Money> cycleAward;
		private StatusProperty<Money> totalAward;

		public bool IsPlayerWagerAvailable => isPlayerWagerAvailable.Value;
		public bool IsCashoutAvailable => isCashoutAvailable.Value;

		/// <summary>
		/// The credits on the credit meter.
		/// </summary>
		public Money BankMeter => bankMeter.Value;

		/// <summary>
		/// The credits available for betting.
		/// </summary>
		/// <remarks>This value is correct for all game states.</remarks>
		public Money WagerableMeter => wagerableMeter.Value;

		/// <summary>
		/// The amount of money paid to the player.
		/// </summary>
		public Money PaidMeter => paidMeter.Value;

		/// <summary>
		/// The credits won in the most recent game cycle.
		/// </summary>
		public Money CycleAward => cycleAward.Value;

		/// <summary>
		/// The total credits won in the most recent game.
		/// </summary>
		public Money TotalAward => totalAward.Value;

		/// <summary>
		/// The value to show on the win meter.
		/// </summary>
		public Money WinMeter
		{
			get => winMeter.Value;
			set => winMeter.Value = value;
		}

		public BankStatus() : base(nameof(BankStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.IsPlayerWagerAvailable, v => isPlayerWagerAvailable.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.IsCashoutAvailable, v => isCashoutAvailable.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MetersService.CreditMeter, v => bankMeter.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MetersService.WagerableMeter, v => wagerableMeter.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MetersService.PaidMeter, v => paidMeter.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MetersService.CycleAward, v => cycleAward.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MetersService.TotalAward, v => totalAward.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			isPlayerWagerAvailable = AddProperty(nameof(IsPlayerWagerAvailable), false);
			isCashoutAvailable = AddProperty(nameof(IsCashoutAvailable), false);
			bankMeter = AddProperty(nameof(BankMeter), Money.Zero);
			wagerableMeter = AddProperty(nameof(WagerableMeter), Money.Zero);
			paidMeter = AddProperty(nameof(PaidMeter), Money.Zero);
			winMeter = AddProperty(nameof(WinMeter), Money.Zero);
			cycleAward = AddProperty(nameof(CycleAward), Money.Zero);
			totalAward = AddProperty(nameof(TotalAward), Money.Zero);
		}
	}
}