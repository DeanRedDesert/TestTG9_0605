using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Lights;

namespace Midas.Presentation.Cabinet
{
	public interface ICabinetShim
	{
		bool IsActive { get; }
		IGamingSubsystem CabinetSubsystem { get; }
		void SetReadyState(bool ready);

		void RequestCashout();
		void RequestService();

		PhysicalButtons PhysicalButtons { get; }

		/// <summary>
		///     Merges the button mapping with the current and pushes new button mapping onto stack
		/// </summary>
		/// <param name="buttonToFunctionMapFunction">a function that returns the New mapping of PhysicalButton to ButtonFunction</param>
		ButtonMappingHandle PushButtonMapping(Func<IEnumerable<(PhysicalButton Button, ButtonFunction ButtonFunction)>> buttonToFunctionMapFunction);

		/// <summary>
		///     Restores previous button mapping
		/// </summary>
		/// <param name="handle">the handle of the button mapping which to remove from the stack</param>
		void PopButtonMapping(ButtonMappingHandle handle);

		void AddButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers);

		void RemoveButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers);

		public ButtonFunction GetButtonFunctionFromButton(PhysicalButton button);

		IEnumerable<PhysicalButton> GetDefaultButtonsFromButtonFunction(ButtonFunction function);

		#region Peripheral Lights

		LightsHandle AddLights(IStreamingLights streamingLights);
		LightsHandle AddLights(IRuntimeLights runtimeLights);
		TimeSpan PlayLights(LightsHandle lightsHandle, bool loop);
		void StopLights();

		#endregion

		#region Monitor

		IReadOnlyList<MonitorConfig> MonitorConfigs { get; }
		void SetMonitorConfigs(IReadOnlyList<MonitorConfig> monitorConfigs);

		#endregion

		#region Volume

		event Action<VolumeConfig> VolumeConfigChanged;
		event Action<float> GameVolumeAttenuationChanged;

		VolumeConfig GetVolumeConfig();
		void SetVolumeConfig(VolumeConfig newVolumeConfig);
		float GetGameVolumeAttenuation();

		#endregion
	}
}