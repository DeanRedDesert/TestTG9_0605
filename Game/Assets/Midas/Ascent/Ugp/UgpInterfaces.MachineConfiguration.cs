using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpMachineConfiguration machineConfiguration;
		private readonly MachineConfigurationParameters noMachineConfigurationParameters = new MachineConfigurationParameters(false, string.Empty, 100, 3000, false, false, int.MaxValue, false, UgpMachineConfigurationWinCapStyle.None, 0, string.Empty, string.Empty, string.Empty);

		/// <summary>
		/// Event raised when the machine configuration has changed.
		/// </summary>
		public event EventHandler<MachineConfigurationChangedEventArgs> MachineConfigurationChanged;

		private void InitMachineConfig()
		{
			machineConfiguration = GameLib.GetInterface<IUgpMachineConfiguration>();

			if (machineConfiguration != null)
				machineConfiguration.MachineConfigurationChanged += OnMachineConfigurationChanged;
		}

		private void DeInitMachineConfig()
		{
			if (machineConfiguration != null)
				machineConfiguration.MachineConfigurationChanged -= OnMachineConfigurationChanged;

			machineConfiguration = null;
		}

		/// <summary>
		/// Gets the current machine configuration parameters.
		/// </summary>
		public MachineConfigurationParameters GetMachineConfigurationParameters()
		{
			return machineConfiguration == null ? noMachineConfigurationParameters : machineConfiguration.GetMachineConfigurationParameters();
		}

		private void OnMachineConfigurationChanged(object s, MachineConfigurationChangedEventArgs e)
		{
			MachineConfigurationChanged?.Invoke(s, e);
		}
	}
}