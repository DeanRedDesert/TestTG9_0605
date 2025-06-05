using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using Midas.Ascent.Cabinet.Lights.Visualiser;
using Midas.Core;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Lights;
using ButtonFunction = Midas.Presentation.ButtonHandling.ButtonFunction;
using IStreamingLights = Midas.Presentation.Cabinet.IStreamingLights;

namespace Midas.Ascent.Cabinet
{
	internal sealed class AscentCabinetShim : ICabinetShim
	{
		private IReadiness readiness;
		private LightsVisualiserWindow lightsVisualiserWindow;

		private IReadiness Readiness => readiness ??= AscentCabinet.CabinetLib?.GetInterface<IReadiness>();

		public AscentCabinetShim(IGamingSubsystem cabinetSubsystem)
		{
			CabinetSubsystem = cabinetSubsystem;
		}

		public bool IsActive => AscentCabinet.IsActive;
		public IGamingSubsystem CabinetSubsystem { get; }

		public void SetReadyState(bool ready) => Readiness?.SetReadyState(ready ? ReadyState.ReadyForDisplay : ReadyState.NotReadyForDisplay);

		public void RequestCashout() => AscentCabinet.ServiceController.RequestCashOut();
		public void RequestService() => AscentCabinet.ServiceController.RequestService();

		public PhysicalButtons PhysicalButtons => AscentCabinet.ButtonPanel.ButtonPanel?.PhysicalButtons;
		public ButtonMappingHandle PushButtonMapping(Func<IEnumerable<(PhysicalButton Button, ButtonFunction ButtonFunction)>> buttonToFunctionMapFunction) => AscentCabinet.ButtonPanel.PushButtonMapping(buttonToFunctionMapFunction);
		public void PopButtonMapping(ButtonMappingHandle handle) => AscentCabinet.ButtonPanel.PopButtonMapping(handle);
		public void AddButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers) => AscentCabinet.ButtonPanel.AddButtonPanelMappers(mappers);
		public void RemoveButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers) => AscentCabinet.ButtonPanel.RemoveButtonPanelMappers(mappers);
		public ButtonFunction GetButtonFunctionFromButton(PhysicalButton button) => AscentCabinet.ButtonPanel.GetButtonFunctionFromButton(button);
		public IEnumerable<PhysicalButton> GetDefaultButtonsFromButtonFunction(ButtonFunction function) => AscentCabinet.ButtonPanel.GetDefaultButtonsFromButtonFunction(function);

		public LightsHandle AddLights(IStreamingLights streamingLights) => AscentCabinet.ChoreographyPlayer.AddLights(streamingLights);
		public LightsHandle AddLights(IRuntimeLights runtimeLights) => AscentCabinet.ChoreographyPlayer.AddLights(runtimeLights);
		public TimeSpan PlayLights(LightsHandle lightsHandle, bool loop) => AscentCabinet.ChoreographyPlayer.Play(lightsHandle, loop);
		public void StopLights() => AscentCabinet.ChoreographyPlayer.Stop(true);

		public IReadOnlyList<MonitorConfig> MonitorConfigs { get; private set; }
		public void SetMonitorConfigs(IReadOnlyList<MonitorConfig> newMonitorConfigs) => MonitorConfigs = newMonitorConfigs;

		public event Action<VolumeConfig> VolumeConfigChanged
		{
			add => AscentCabinet.VolumeController.VolumeConfigChanged += value;
			remove => AscentCabinet.VolumeController.VolumeConfigChanged -= value;
		}

		public event Action<float> GameVolumeAttenuationChanged
		{
			add => AscentCabinet.VolumeController.GameVolumeAttenuationChanged += value;
			remove => AscentCabinet.VolumeController.GameVolumeAttenuationChanged -= value;
		}

		public VolumeConfig GetVolumeConfig() => AscentCabinet.VolumeController.GetVolumeConfig();
		public void SetVolumeConfig(VolumeConfig volumeConfig) => AscentCabinet.VolumeController.SetVolumeConfig(volumeConfig);
		public float GetGameVolumeAttenuation() => AscentCabinet.VolumeController.GetGameVolumeAttenuation();
	}
}