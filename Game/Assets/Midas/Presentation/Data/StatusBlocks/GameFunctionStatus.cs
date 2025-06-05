using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;
using DenominationPlayableStatus = Midas.Core.DenominationPlayableStatus;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class GameFunctionStatus : StatusBlock
	{
		private StatusProperty<GameButtonBehaviours> gameButtonBehaviours;
		private StatusProperty<IReadOnlyList<DenominationPlayableStatus>> denominationPlayableStatus;
		private StatusProperty<TimeSpan> timeout;
		private StatusProperty<bool> isTimeoutActive;

		public GameButtonBehaviours GameButtonBehaviours => gameButtonBehaviours.Value;
		public IReadOnlyList<DenominationPlayableStatus> DenominationPlayableStatus => denominationPlayableStatus.Value;
		public TimeSpan Timeout => timeout.Value;
		public bool IsTimeoutActive => isTimeoutActive.Value;

		public GameFunctionStatus() : base(nameof(GameFunctionStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GameFunctionStatusService.GameButtonBehaviours, v => gameButtonBehaviours.Value = new GameButtonBehaviours(v));
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GameFunctionStatusService.DenominationPlayableStatus, v => denominationPlayableStatus.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GameFunctionStatusService.Timeout, v => timeout.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GameFunctionStatusService.IsTimeoutActive, v => isTimeoutActive.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			gameButtonBehaviours = AddProperty(nameof(GameButtonBehaviours), default(GameButtonBehaviours));
			denominationPlayableStatus = AddProperty(nameof(DenominationPlayableStatus), default(IReadOnlyList<DenominationPlayableStatus>));
			timeout = AddProperty(nameof(Timeout), default(TimeSpan));
			isTimeoutActive = AddProperty(nameof(IsTimeoutActive), default(bool));
		}
	}
}