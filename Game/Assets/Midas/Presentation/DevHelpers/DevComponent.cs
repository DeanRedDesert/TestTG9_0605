using Midas.Core.Configuration;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;

namespace Midas.Presentation.DevHelpers
{
	public enum DevLevel
	{
		Show,
		Dev
	}

	public class DevComponent : MonoBehaviour
	{
		private bool isRegistered;

		[SerializeField]
		private DevLevel devLevel;

		private void OnEnable()
		{
			StatusDatabase.ConfigurationStatus.AddPropertyChangedHandler(nameof(ConfigurationStatus.MachineConfig), OnMachineConfigChanged);
			isRegistered = true;
			UpdateMachineConfig(StatusDatabase.ConfigurationStatus?.MachineConfig);
		}

		private void OnDisable()
		{
			if (!isRegistered)
				return;

			StatusDatabase.ConfigurationStatus.RemovePropertyChangedHandler(nameof(ConfigurationStatus.MachineConfig), OnMachineConfigChanged);
			isRegistered = false;
		}

		private void OnMachineConfigChanged(StatusBlock sender, string propertyname)
		{
			UpdateMachineConfig(StatusDatabase.ConfigurationStatus.MachineConfig);
		}

		private void UpdateMachineConfig(MachineConfig machineConfig)
		{
			if (machineConfig == null)
				return;

			StatusDatabase.ConfigurationStatus.RemovePropertyChangedHandler(nameof(ConfigurationStatus.MachineConfig), OnMachineConfigChanged);
			isRegistered = false;

			var kill = false;

			switch (devLevel)
			{
				case DevLevel.Show:
					kill = !machineConfig.AreShowFeaturesEnabled;
					break;
				case DevLevel.Dev:
					kill = !machineConfig.AreDevFeaturesEnabled;
					break;
			}

			if (kill)
				Destroy(gameObject);
		}
	}
}