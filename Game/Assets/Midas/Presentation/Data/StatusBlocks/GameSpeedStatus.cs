using System;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public enum GameSpeed
	{
		Normal,
		Fast,
		SuperFast
	}

	public sealed class GameSpeedStatus : StatusBlock
	{
		private bool isMainGameSpeedZero;
		private bool isFreeGameSpeedZero;
		private StatusProperty<GameSpeed> gameSpeed;
		private StatusProperty<GameSpeed> defaultGameSpeed;
		private StatusProperty<bool> isChangeGameSpeedAllowed;

		public GameSpeed GameSpeed
		{
			get => gameSpeed.Value;
			set => gameSpeed.Value = value;
		}

		public GameSpeed DefaultGameSpeed
		{
			get => defaultGameSpeed.Value;
			set => defaultGameSpeed.Value = value;
		}

		public bool IsChangeGameSpeedAllowed
		{
			get => isChangeGameSpeedAllowed.Value;
			set => isChangeGameSpeedAllowed.Value = value;
		}

		public GameSpeedStatus() : base(nameof(GameSpeedStatus))
		{
		}

		public void IncreaseSpeedIfAllowed()
		{
			if (IsChangeGameSpeedAllowed)
			{
				gameSpeed.Value = gameSpeed.Value switch
				{
					GameSpeed.Normal => GameSpeed.Fast,
					GameSpeed.Fast => GameSpeed.SuperFast,
					_ => gameSpeed.Value
				};
			}
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.BaseGameCycleTime, OnMainGameTimeChange);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.FreeGameCycleTime, OnFreeGameTimeChange);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			defaultGameSpeed = AddProperty(nameof(DefaultGameSpeed), GameSpeed.Normal);
			gameSpeed = AddProperty(nameof(GameSpeed), defaultGameSpeed.Value);
			isChangeGameSpeedAllowed = AddProperty(nameof(IsChangeGameSpeedAllowed), false);
		}

		#region Private Methods

		private void OnMainGameTimeChange(TimeSpan obj)
		{
			isMainGameSpeedZero = obj == TimeSpan.Zero;
			IsChangeGameSpeedAllowed = isMainGameSpeedZero && isFreeGameSpeedZero;
		}

		private void OnFreeGameTimeChange(TimeSpan obj)
		{
			isFreeGameSpeedZero = obj == TimeSpan.Zero;
			IsChangeGameSpeedAllowed = isMainGameSpeedZero && isFreeGameSpeedZero;
		}

		#endregion
	}
}