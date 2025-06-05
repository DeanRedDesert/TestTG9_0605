using System.Linq;
using IGT.Game.Core.Communication.Cabinet;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Presentation.Data;

namespace Midas.Ascent.Cabinet
{
	internal sealed class ServiceController : ICabinetController
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private IService service;
		private ICandle candle;

		public void Init()
		{
		}

		public void OnBeforeLoadGame()
		{
			UpdateConfig();
		}

		public void Resume()
		{
			service = AscentCabinet.CabinetLib.GetInterface<IService>();

			Log.Instance.Info(service != null ? "CabinetLib.GetInterface<IService>() found in Resume" : "CabinetLib.GetInterface<IService>()==null in Resume");

			if (service != null)
			{
				service.RegisterForPromptPlayerOnCashoutConfigItemChangedEvents();
				service.PromptPlayerOnCashoutConfigItemChangedEvent += OnPromptPlayerOnCashoutConfigItemChangedEvent;
				service.RegisterForEmulatedServiceButtonEnabledConfigItemChangedEvents();
				service.EmulatedServiceButtonEnabledConfigItemChangedEvent += OnEmulatedServiceButtonEnabledConfigItemChangedEvent;
			}

			candle = AscentCabinet.CabinetLib.GetInterface<ICandle>();
			Log.Instance.Info(candle != null ? "CabinetLib.GetInterface<ICandle>() found in Resume" : "CabinetLib.GetInterface<ICandle>()==null in Resume");

			if (candle != null)
			{
				candle.RegisterForCandleStateChangeEvents(CandleID.Candle1);
				candle.CandleStateChangedEvent += OnCandleStateChanged;
			}
		}

		public void Pause()
		{
			if (service != null)
			{
				service.PromptPlayerOnCashoutConfigItemChangedEvent -= OnPromptPlayerOnCashoutConfigItemChangedEvent;
				service.EmulatedServiceButtonEnabledConfigItemChangedEvent -= OnEmulatedServiceButtonEnabledConfigItemChangedEvent;
			}

			if (candle != null)
				candle.CandleStateChangedEvent -= OnCandleStateChanged;
		}

		public void OnAfterUnLoadGame()
		{
		}

		public void DeInit() => autoUnregisterHelper.UnRegisterAll();

		public void RequestService()
		{
			if (service != null)
				service.RequestService();
			else
				Log.Instance.Error("CabinetLib.GetInterface<IService> is null");
		}

		public void RequestCashOut()
		{
			if (service != null)
				service.RequestCashOut();
			else
				Log.Instance.Error("CabinetLib.GetInterface<IService> is null");
		}

		private void UpdateConfig()
		{
			var isCashOutConfirmationRequired = true;
			var cashOutButtonEmulationNeeded = true;
			var serviceButtonEmulationNeeded = true;
			var isServiceRequestActive = false;
			if (service != null)
			{
				isCashOutConfirmationRequired = service.GetPromptPlayerOnCashoutConfigItemValue();
				var buttonsToEmulate = service.GetTheButtonsThatTheEgmRequiresToBeEmulated();
				cashOutButtonEmulationNeeded = buttonsToEmulate.Contains(EmulatableButton.Cashout);
				serviceButtonEmulationNeeded = buttonsToEmulate.Contains(EmulatableButton.Service);
			}

			if (candle != null)
				isServiceRequestActive = candle.CandleIlluminated(CandleID.Candle1);
			StatusDatabase.ConfigurationStatus.UpdateServiceConfig(new ServiceConfig(isCashOutConfirmationRequired, cashOutButtonEmulationNeeded, serviceButtonEmulationNeeded, isServiceRequestActive, false, false, isServiceRequestActive));
		}

		private static void OnPromptPlayerOnCashoutConfigItemChangedEvent(object sender, PromptPlayerOnCashoutConfigItemChangedEventArgs args)
		{
			var cashOutConfirmationRequired = args.PromptPlayerOnCashoutConfigItemValue;
			var config = StatusDatabase.ConfigurationStatus.ServiceConfig;
			if (config.IsCashOutConfirmationRequired != cashOutConfirmationRequired)
				StatusDatabase.ConfigurationStatus.UpdateServiceConfig(config.ChangeCashOutConfirmationRequired(cashOutConfirmationRequired));
		}

		private static void OnEmulatedServiceButtonEnabledConfigItemChangedEvent(object sender, EmulatedServiceButtonEnabledConfigItemChangedEventArgs args)
		{
			var serviceButtonRequired = args.EmulatedServiceButtonEnabledConfigItemValue;
			var config = StatusDatabase.ConfigurationStatus.ServiceConfig;
			if (config.IsServiceButtonSimulationRequired != serviceButtonRequired)
				StatusDatabase.ConfigurationStatus.UpdateServiceConfig(config.ChangedServiceButtonRequiredSettings(serviceButtonRequired));
		}

		private static void OnCandleStateChanged(object sender, CandleStateChangedEventArgs args)
		{
			Log.Instance.Info($"OnCandleStateChanged CandleId={args.CandleId}, Illuminated={args.Illuminated}");
			if (args.CandleId != CandleID.Candle1)
				return;

			if (StatusDatabase.GameStatus == null || StatusDatabase.ConfigurationStatus == null || StatusDatabase.ConfigurationStatus.ServiceConfig == null)
				return;

			var illuminated = args.Illuminated && StatusDatabase.GameStatus.InActivePlay;
			var config = StatusDatabase.ConfigurationStatus.ServiceConfig;
			if (config.IsServiceRequestActive != illuminated)
				StatusDatabase.ConfigurationStatus.UpdateServiceConfig(config.ChangeServiceRequestActive(illuminated));
		}
	}
}