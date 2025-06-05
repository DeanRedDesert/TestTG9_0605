namespace Midas.Core.Configuration
{
	public sealed class ServiceConfig
	{
		/// <summary>
		///<param name="isCashOutConfirmationRequired">"Prompt Player On CashOut" needed?</param>
		///<param name="isCashOutButtonSimulationRequired">Is the CashOut button required to be emulated(true) or is it still a physical button(false).</param>
		///<param name="isServiceButtonSimulationRequired">Is the Service button required to be emulated(true) or is it still a physical button(false).</param>
		///<param name="isCashOutConfirmationForced">"Prompt Player On CashOut" forced by game designer?</param>
		///<param name="isCashOutButtonSimulationForced">Is the CashOut button forced by game designer to be emulated(true) or is it still a physical button(false).</param>
		///<param name="isServiceButtonSimulationForced">Is the Service button forced by game designer to be emulated(true) or is it still a physical button(false).</param>
		///<param name="isServiceRequestActive">Is the Service request is active.</param>
		/// </summary>
		public ServiceConfig(bool isCashOutConfirmationRequired, bool isCashOutButtonSimulationRequired, bool isServiceButtonSimulationRequired, bool isCashOutConfirmationForced,
			bool isCashOutButtonSimulationForced, bool isServiceButtonSimulationForced, bool isServiceRequestActive)
		{
			IsCashOutConfirmationRequired = isCashOutConfirmationRequired;
			IsCashOutButtonSimulationRequired = isCashOutButtonSimulationRequired;
			IsServiceButtonSimulationRequired = isServiceButtonSimulationRequired;
			IsCashOutConfirmationForced = isCashOutConfirmationForced;
			IsCashOutButtonSimulationForced = isCashOutButtonSimulationForced;
			IsServiceButtonSimulationForced = isServiceButtonSimulationForced;
			IsServiceRequestActive = isServiceRequestActive;
		}

		/// <summary>
		/// "Prompt Player On CashOut" needed?
		/// </summary>
		public bool IsCashOutConfirmationRequired { get; }

		/// <summary>
		/// Is the CashOut button required to be emulated(true) or is it still a physical button(false)
		/// </summary>
		public bool IsCashOutButtonSimulationRequired { get; }

		/// <summary>
		/// Is the Service button required to be emulated(true) or is it still a physical button(false)
		/// </summary>
		public bool IsServiceButtonSimulationRequired { get; }

		/// <summary>
		/// "Prompt Player On CashOut" forced?
		/// </summary>
		public bool IsCashOutConfirmationForced { get; }

		/// <summary>
		/// Is the CashOut button forced to be emulated(true) or is it still a physical button(false)
		/// </summary>
		public bool IsCashOutButtonSimulationForced { get; }

		/// <summary>
		/// Is the Service button forced to be emulated(true) or is it still a physical button(false)
		/// </summary>
		public bool IsServiceButtonSimulationForced { get; }

		/// <summary>
		/// "Prompt Player On CashOut" needed?
		/// </summary>
		public bool IsCashOutConfirmationNeeded => IsCashOutConfirmationRequired || IsCashOutConfirmationForced;

		/// <summary>
		/// Is the CashOut button needed to be emulated(true)
		/// </summary>
		public bool IsCashOutButtonSimulationNeeded => IsCashOutButtonSimulationRequired || IsCashOutButtonSimulationForced;

		/// <summary>
		/// Is the Service button needed to be emulated(true)
		/// </summary>
		public bool IsServiceButtonSimulationNeeded => IsServiceButtonSimulationRequired || IsServiceButtonSimulationForced;

		/// <summary>
		/// Is the Service request is active(in Ascent: CandleIlluminated(CandleID.Candle1))
		/// </summary>
		public bool IsServiceRequestActive { get; }

		public override string ToString() => $"CashOutConfirmRequired:{IsCashOutConfirmationRequired}, SimulateCashOutRequired:{IsCashOutButtonSimulationRequired}, SimulateServiceRequired:{IsServiceButtonSimulationRequired},CashOutConfirmForced:{IsCashOutConfirmationForced}, SimulateCashOutForced:{IsCashOutButtonSimulationForced}, SimulateServiceForced:{IsServiceButtonSimulationForced}, ServiceActive:{IsServiceRequestActive}";

		/// <summary>
		/// Change if "Prompt Player On CashOut" is needed
		/// </summary>
		/// <param name="isCashOutConfirmationRequired"></param>
		/// <returns>A new created config with only that single parameter changed</returns>
		public ServiceConfig ChangeCashOutConfirmationRequired(bool isCashOutConfirmationRequired) =>
			new ServiceConfig(isCashOutConfirmationRequired, IsCashOutButtonSimulationRequired, IsServiceButtonSimulationRequired,
				IsCashOutConfirmationForced, IsCashOutButtonSimulationForced, IsServiceButtonSimulationForced,
				IsServiceRequestActive);

		/// <summary>
		/// Change if "IsServiceRequestActive"
		/// </summary>
		/// <param name="isServiceRequestActive"></param>
		/// <returns>A new created config with only that single parameter changed</returns>
		public ServiceConfig ChangeServiceRequestActive(bool isServiceRequestActive) =>
			new ServiceConfig(IsCashOutConfirmationRequired, IsCashOutButtonSimulationRequired, IsServiceButtonSimulationRequired,
				IsCashOutConfirmationForced, IsCashOutButtonSimulationForced, IsServiceButtonSimulationForced,
				isServiceRequestActive);

		/// <summary>
		/// Change IsServiceButtonSimulationRequired setting"
		/// </summary>
		/// <param name="isServiceButtonSimulationRequired"></param>
		/// <returns>A new created config with only these parameters changed</returns>
		public ServiceConfig ChangedServiceButtonRequiredSettings(bool isServiceButtonSimulationRequired) =>
			new ServiceConfig(IsCashOutConfirmationRequired, IsCashOutButtonSimulationRequired, isServiceButtonSimulationRequired,
				IsCashOutConfirmationForced, IsCashOutButtonSimulationForced, IsServiceButtonSimulationForced,
				IsServiceRequestActive);

		/// <summary>
		/// Change all Is*Forced settings"
		/// </summary>
		/// <param name="isCashOutConfirmationForced"></param>
		/// <param name="isCashOutButtonSimulationForced"></param>
		/// <param name="isServiceButtonSimulationForced"></param>
		/// <returns>A new created config with only these parameters changed</returns>
		public ServiceConfig ChangedForcedSettings(bool isCashOutConfirmationForced, bool isCashOutButtonSimulationForced, bool isServiceButtonSimulationForced) =>
			new ServiceConfig(IsCashOutConfirmationRequired, IsCashOutButtonSimulationRequired, IsServiceButtonSimulationRequired,
				isCashOutConfirmationForced, isCashOutButtonSimulationForced, isServiceButtonSimulationForced,
				IsServiceRequestActive);
	}
}