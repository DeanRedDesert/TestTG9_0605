using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent
{
	public sealed class FlashingPlayerClockInterfaces
	{
		private readonly IFlashPlayerClock clock;

		public event EventHandler<bool> FlashingClockChanged;

		public FlashingPlayerClockInterfaces()
		{
			clock = GameLib.GetInterface<IFlashPlayerClock>();

			if (clock != null)
				clock.FlashPlayerClockPropertiesChangedEvent += OnClockFlashPlayerClockPropertiesChangedEvent;
		}

		public void DeInit()
		{
			if (clock != null)
				clock.FlashPlayerClockPropertiesChangedEvent -= OnClockFlashPlayerClockPropertiesChangedEvent;
		}

		public FlashPlayerClockConfig GetConfig() => clock == null ? new FlashPlayerClockConfig() : clock.FlashPlayerClockConfig;

		public FlashPlayerClockProperties GetProperties() => clock == null ? new FlashPlayerClockProperties() : clock.FlashPlayerClockProperties;

		private void OnClockFlashPlayerClockPropertiesChangedEvent(object sender, FlashPlayerClockPropertiesChangedEventArgs e) => FlashingClockChanged?.Invoke(sender, e.PlayerClockSessionActive ?? false);
	}
}