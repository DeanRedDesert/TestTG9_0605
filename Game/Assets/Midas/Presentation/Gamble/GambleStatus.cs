using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Gamble
{
	public abstract class GambleStatus : StatusBlock
	{
		private StatusProperty<bool> awaitingSelection;
		private StatusProperty<bool> isGambleOfferable;

		public bool AwaitingSelection
		{
			get => awaitingSelection.Value;
			protected set => awaitingSelection.Value = value;
		}

		public bool IsGambleOfferable
		{
			get => isGambleOfferable.Value;
			private set => isGambleOfferable.Value = value;
		}

		protected GambleStatus(string name) : base(name)
		{
		}

		public abstract void TakeWin();

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.IsGambleOfferable, v => isGambleOfferable.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			awaitingSelection = AddProperty(nameof(AwaitingSelection), false);
			isGambleOfferable = AddProperty(nameof(IsGambleOfferable), false);
		}
	}
}