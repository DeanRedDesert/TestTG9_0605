using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class ExternalJackpotsStatus : StatusBlock
	{
		private StatusProperty<bool> isVisible;
		private StatusProperty<int> iconId;
		private StatusProperty<IReadOnlyList<ExternalJackpot>> externalJackpots;

		public bool IsVisible => isVisible.Value;
		public int IconId => iconId.Value;
		public IReadOnlyList<ExternalJackpot> ExternalJackpots => externalJackpots.Value;

		public ExternalJackpotsStatus() : base(nameof(ExternalJackpotsStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ExternalJackpotService.IsVisible, v => isVisible.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ExternalJackpotService.IconId, v => iconId.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ExternalJackpotService.ExternalJackpots, v => externalJackpots.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			isVisible = AddProperty(nameof(IsVisible), default(bool));
			iconId = AddProperty(nameof(IconId), default(int));
			externalJackpots = AddProperty(nameof(ExternalJackpots), default(IReadOnlyList<ExternalJackpot>));
		}
	}
}