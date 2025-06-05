namespace Midas.Ascent.Ugp
{
	/// <summary>
	/// UGP specific interfaces with default implementations for when they don't exist.
	/// </summary>
	public sealed partial class UgpInterfaces
	{
		/// <summary>
		/// Is the foundation UGP or Ascent?
		/// </summary>
		public bool IsUgpFoundation => machineConfiguration != null;

		public UgpInterfaces()
		{
			InitProgressiveAward();
			InitPid();
			InitMessageStrip();
			InitReserve();
			InitRuntimeGameEvents();
			InitMachineConfig();
			InitExternalJackpots();
			InitProgressives();
			InitGameFunctionStatus();

			InitStandalone();
		}

		public void DeInit()
		{
			DeInitProgressiveAward();
			DeInitPid();
			DeInitMessageStrip();
			DeInitReserve();
			DeInitRuntimeGameEvents();
			DeInitMachineConfig();
			DeInitExternalJackpots();
			DeInitProgressives();
			DeInitGameFunctionStatus();
		}
	}
}