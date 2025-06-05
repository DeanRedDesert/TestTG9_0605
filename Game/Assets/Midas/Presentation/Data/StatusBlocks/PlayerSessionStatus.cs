using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class PlayerSessionStatus : StatusBlock
	{
		private StatusProperty<PlayerSession> playerSession;
		private StatusProperty<PlayerSessionParameters> playerSessionParameters;
		private StatusProperty<bool> isSessionTimerDisplayEnabled;

		public PlayerSession PlayerSession => playerSession.Value;
		public PlayerSessionParameters PlayerSessionParameters => playerSessionParameters.Value;
		public bool IsSessionTimerDisplayEnabled => isSessionTimerDisplayEnabled.Value;

		public PlayerSessionStatus() : base(nameof(PlayerSessionStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PlayerSessionService.Session, v => playerSession.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PlayerSessionService.Parameters, v => playerSessionParameters.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PlayerSessionService.IsSessionTimerDisplayEnabled, v => isSessionTimerDisplayEnabled.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			playerSession = AddProperty(nameof(PlayerSession), default(PlayerSession));
			playerSessionParameters = AddProperty(nameof(PlayerSessionParameters), default(PlayerSessionParameters));
			isSessionTimerDisplayEnabled = AddProperty(nameof(IsSessionTimerDisplayEnabled), default(bool));
		}
	}
}