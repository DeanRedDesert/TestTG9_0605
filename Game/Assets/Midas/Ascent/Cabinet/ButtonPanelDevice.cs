using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.LogicToPresentation.Data;
using Midas.Presentation;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.ExtensionMethods;
using ButtonFunction = Midas.Presentation.ButtonHandling.ButtonFunction;
using static Midas.Ascent.Cabinet.AscentCabinet;
using static Midas.Ascent.AscentFoundation;
using ButtonPanelType = Midas.Presentation.ButtonHandling.ButtonPanelType;

namespace Midas.Ascent.Cabinet
{
	using PhysicalButton2ButtonFunctionList = IReadOnlyList<(PhysicalButton Button, ButtonFunction ButtonFunction)>;

	internal sealed class ButtonPanelDevice : CabinetDevice
	{
		private ConfigData configData;
		private readonly ButtonLampController buttonLampController = new ButtonLampController();
		private readonly List<(PhysicalButton Button, ButtonFunction ButtonFunction)> buttonDown = new List<(PhysicalButton, ButtonFunction ButtonFunction)>();
		private readonly ButtonMappingStack buttonMappingStack = new ButtonMappingStack();
		private readonly List<IButtonPanelMapper> buttonPanelMappers = new List<IButtonPanelMapper>();

		public ButtonPanelDevice()
			: base(DeviceType.ButtonPanel)
		{
		}

		public override void Init()
		{
			base.Init();
			SetAcquired(false);
			DeviceAcquired += OnDeviceAcquired;
			GameServices.ConfigurationService.ConfigurationData.RegisterChangeListenerAction(OnConfigurationMessage);
		}

		public override void OnBeforeLoadGame()
		{
			UpdatePhysicalButton2ButtonFunctionMapping();
		}

		public override void Resume()
		{
			base.Resume();

			CabinetLib.ButtonPressedEvent += OnButtonPressedEvent;
			ButtonManager.AddButtonsStateChangedListener(OnButtonStatesUpdate);
			FrameUpdateService.Update.OnFrameUpdate += OnFrameUpdate;

			UpdateConnectedDevices();
		}

		public override void Pause()
		{
			ButtonManager.RemoveButtonsStateChangedListener(OnButtonStatesUpdate);
			FrameUpdateService.Update.OnFrameUpdate -= OnFrameUpdate;
			CabinetLib.ButtonPressedEvent -= OnButtonPressedEvent;
			SetAcquired(false);
			base.Pause();
		}

		public override void OnAfterUnLoadGame()
		{
			buttonMappingStack.OnAfterUnLoadGame();
		}

		public override void DeInit()
		{
			GameServices.ConfigurationService.ConfigurationData.UnregisterChangeListenerAction(OnConfigurationMessage);
			DeviceAcquired -= OnDeviceAcquired;
			base.DeInit();
		}

		public ButtonMappingHandle PushButtonMapping(Func<IEnumerable<(PhysicalButton Button, ButtonFunction ButtonFunction)>> buttonToFunctionMapFunction)
		{
			var handle = buttonMappingStack.MergeAndPushButtonMapping(buttonToFunctionMapFunction);
			return handle;
		}

		public void PopButtonMapping(ButtonMappingHandle handle)
		{
			buttonMappingStack.PopButtonMapping(handle);
		}

		public ButtonFunction GetButtonFunctionFromButton(PhysicalButton button)
		{
			return buttonMappingStack.GetButtonFunctionFromButton(button);
		}

		public ButtonFunction GetDefaultButtonFunctionFromButton(PhysicalButton button)
		{
			return buttonMappingStack.GetDefaultButtonFunctionFromButton(button);
		}

		public IEnumerable<PhysicalButton> GetDefaultButtonsFromButtonFunction(ButtonFunction function)
		{
			return buttonMappingStack.GetDefaultButtonsFromButtonFunction(function);
		}

		public void AddButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers)
		{
			buttonPanelMappers.AddRange(mappers);
		}

		public void RemoveButtonPanelMappers(IReadOnlyList<IButtonPanelMapper> mappers)
		{
			foreach (var mapper in mappers)
			{
				buttonPanelMappers.Remove(mapper);
			}
		}

		public ButtonPanel ButtonPanel { get; private set; } = new ButtonPanel(uint.MaxValue, ButtonPanelType.Default);

		private void OnConfigurationMessage(ConfigData newConfigData)
		{
			configData = newConfigData;
			UpdatePhysicalButton2ButtonFunctionMapping();
		}

		private void OnFrameUpdate()
		{
			buttonLampController.FrameUpdate(FrameTime.CurrentTime);
		}

		private void OnButtonStatesUpdate(IReadOnlyCollection<ButtonStateData> buttonEventData)
		{
			var activeButtonMapping = buttonMappingStack.ActiveButtonMapping;
			if (activeButtonMapping != null)
			{
				//set states on physical button panel controller
				buttonLampController.SetButtonStates(buttonEventData, activeButtonMapping);
			}
		}

		private void SendButtonFunctionDown(PhysicalButton button, ButtonFunction buttonFunction)
		{
			if (buttonDown.FindIndex(entry => entry.Button == button) == -1)
			{
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Down, button, ButtonPanel.PanelIdentifier));
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Pressed, button, ButtonPanel.PanelIdentifier));
				buttonDown.Add((button, buttonFunction));
			}
			else
			{
				Log.Instance.Warn($"ButtonEvent Down of button={button} ignored because was already down");
			}
		}

		private void SendButtonFunctionUp(PhysicalButton button)
		{
			var idx = buttonDown.FindIndex(entry => entry.Button == button);
			if (idx != -1)
			{
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonDown[idx].ButtonFunction, ButtonEvent.Up, button, ButtonPanel.PanelIdentifier));
				buttonDown.RemoveAt(idx);
			}
			else
			{
				Log.Instance.Warn($"ButtonEvent Up of button={button} ignored because was not down");
			}
		}

		private void SendButtonUp(PhysicalButton button)
		{
			Log.Instance.Info($"Physical ButtonEvent Up button={button}");
			SendButtonFunctionUp(button);
		}

		private void SendButtonDown(PhysicalButton button)
		{
			Log.Instance.Info($"Physical ButtonEvent Down button={button}");

			var buttonFunction = GetButtonFunctionFromButton(button);
			if (buttonFunction != null && !buttonFunction.Equals(ButtonFunction.Undefined))
			{
				SendButtonFunctionDown(button, buttonFunction);
			}
			else
			{
				Log.Instance.Warn($"ButtonPanel, Button not mapped: {button}");
			}
		}

		private void OnButtonPressedEvent(object sender, CabinetButtonPressedEventArgs e)
		{
			if (e.Pressed)
			{
				SendButtonDown(new PhysicalButton(e.ButtonId));
			}
			else
			{
				SendButtonUp(new PhysicalButton(e.ButtonId));
			}
		}

		private void UpdateConnectedDevices()
		{
			foreach (var buttonPanelConfiguration in CabinetLib.GetButtonPanelConfigurations())
			{
				RequestAcquireDevice(buttonPanelConfiguration.DeviceId);
			}
		}

		private void OnDeviceAcquired(DeviceType deviceType, string deviceId, bool acquired)
		{
			if (acquired)
			{
				// Call to GetButtonPanelConfigurations() is needed because it updates internal panel location mapping (button lamps won't work otherwise, etc.)
				CabinetLib.GetButtonPanelConfigurations();
			}

			SetAcquired(acquired);
		}

		private void SetAcquired(bool acquired)
		{
			ButtonPanel panel;
			if (acquired)
			{
				var panelConfigurations = CabinetLib.GetButtonPanelConfigurations();
				if (panelConfigurations.Count != 1 && GameParameters.Type == IgtGameParameters.GameType.Standard)
				{
					Log.Instance.Fatal(
						$"Unsupported Number of Button Panels, {panelConfigurations.Count}");
				}

				if (panelConfigurations.Count == 0)
				{
					//instantiate a button panel emulation
					Log.Instance.Info("ButtonPanelConfiguration: acquired, but no ButtonPanelConfiguration found. Using ButtonPanelSbp");
					panel = new ButtonPanel(uint.MaxValue, ButtonPanelType.Default);
				}
				else
				{
					var buttonPanelConfiguration = panelConfigurations.First();
					Log.Instance.Info($"ButtonPanelConfiguration: Identifier {buttonPanelConfiguration.PanelIdentifier}");
					panel = CreateButtonPanel(buttonPanelConfiguration);
				}
			}
			else
			{
				Log.Instance.Info("ButtonPanelConfiguration: not acquired. Using old Button Panel");
				panel = ButtonPanel;
			}

			ButtonPanel = panel;
			buttonLampController.SetAcquired(acquired, panel);
			UpdatePhysicalButton2ButtonFunctionMapping();
		}

		private static ButtonPanel CreateButtonPanel(IButtonPanelConfiguration buttonPanelConfiguration)
		{
			switch (buttonPanelConfiguration.PanelIdentifier)
			{
				// Add different button panel configurations as required here.
				default:
					return new ButtonPanel(buttonPanelConfiguration.PanelIdentifier, ButtonPanelType.Default);
			}
		}

		private void UpdatePhysicalButton2ButtonFunctionMapping()
		{
			if (buttonPanelMappers.Count == 0)
				return;

			foreach (var mapper in buttonPanelMappers)
			{
				if (mapper.SupportsButtonPanel(ButtonPanel.PanelType))
				{
					buttonMappingStack.SetDefaultMapping(mapper.CreatePhysicalButton2ButtonFunctionMapping(ButtonPanel.PhysicalButtons, configData));
					return;
				}
			}

			Log.Instance.Fatal($"ButtonPanelConfiguration: No button panel mapper for button panel '{ButtonPanel.PanelType}' found");
		}
	}
}