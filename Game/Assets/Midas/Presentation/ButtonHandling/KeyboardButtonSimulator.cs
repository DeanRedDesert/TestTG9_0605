using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.General;
using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	[Serializable]
	public sealed class KeyCodeButtonFunctionDictionary : SerializableDictionary<KeyCode, ButtonFunction>
	{
	}

	[Serializable]
	public sealed class KeyCodeButtonNameDictionary : SerializableDictionary<KeyCode, ButtonName>
	{
	}

	public sealed class KeyboardButtonSimulator : MonoBehaviour, IGamingSubsystem
	{
		private bool isForFrameUpdateRegistered;
		private readonly List<(KeyCode KeyKode, ButtonFunction ButtonFunction)> buttonDown = new List<(KeyCode KeyKode, ButtonFunction ButtonFunction)>();

		[SerializeField]
		private KeyCodeButtonFunctionDictionary keyCodeButtonFunctionMapping = new KeyCodeButtonFunctionDictionary();

		[SerializeField]
		private KeyCodeButtonNameDictionary keyCodeButtonNameMapping = new KeyCodeButtonNameDictionary();

		#region IGamingSubsystem Implementation

		public string Name => nameof(KeyboardButtonSimulator);

		public void Init()
		{
			// nothing to be done
		}

		public void OnStart()
		{
			// nothing to be done
		}

		public void OnBeforeLoadGame()
		{
			StatusDatabase.ConfigurationStatus.AddPropertyChangedHandler<MachineConfig>(nameof(ConfigurationStatus.MachineConfig), OnMachineConfigChanged);
			SetMachineConfig(StatusDatabase.ConfigurationStatus.MachineConfig);
		}

		public void OnAfterUnloadGame()
		{
			StatusDatabase.ConfigurationStatus.RemovePropertyChangedHandler<MachineConfig>(nameof(ConfigurationStatus.MachineConfig), OnMachineConfigChanged);
			UnRegisterFromFrameUpdate();
		}

		public void OnStop()
		{
			// nothing to be done
		}

		public void DeInit()
		{
			// nothing to be done
		}

		#endregion

		#region Private Methods

		private void OnMachineConfigChanged(StatusBlock _, string __, MachineConfig newValue, MachineConfig ___)
		{
			SetMachineConfig(newValue);
		}

		private void SetMachineConfig(MachineConfig newValue)
		{
			if (newValue is { AreShowFeaturesEnabled: true })
				RegisterForFrameUpdate();
			else
				UnRegisterFromFrameUpdate();
		}

		private void OnFrameUpdate()
		{
#if DEBUG_KEYCODES
            foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(value))
                {
                    Log.Instance.Debug($"Keydown={value}");
                }
            }
#endif
			foreach (var buttonFunction in keyCodeButtonFunctionMapping)
			{
				if (Input.GetKeyDown(buttonFunction.Key))
				{
					SendButtonFunctionDown(buttonFunction.Key, buttonFunction.Value);
				}

				if (Input.GetKeyUp(buttonFunction.Key))
				{
					SendButtonFunctionUp(buttonFunction.Key);
				}
			}

			foreach (var keyCodeButtonPair in keyCodeButtonNameMapping)
			{
				if (keyCodeButtonPair.Value.Button == null)
				{
					continue;
				}

				if (keyCodeButtonPair.Key == KeyCode.Space &&
					Input.GetMouseButtonDown(2))
				{
					var buttonFunction = CabinetManager.Cabinet.GetButtonFunctionFromButton(keyCodeButtonPair.Value.Button.Value);
					if (buttonFunction != null && !buttonFunction.Equals(ButtonFunction.Undefined))
					{
						SendButtonFunctionDown(keyCodeButtonPair.Key, buttonFunction);
						SendButtonFunctionUp(keyCodeButtonPair.Key);
					}
				}

				if (Input.GetKeyDown(keyCodeButtonPair.Key))
				{
					var buttonFunction = CabinetManager.Cabinet.GetButtonFunctionFromButton(keyCodeButtonPair.Value.Button.Value);
					if (buttonFunction != null && !buttonFunction.Equals(ButtonFunction.Undefined))
					{
						SendButtonFunctionDown(keyCodeButtonPair.Key, buttonFunction);
					}
				}

				if (Input.GetKeyUp(keyCodeButtonPair.Key))
				{
					SendButtonFunctionUp(keyCodeButtonPair.Key);
				}
			}
		}

		private void SendButtonFunctionDown(KeyCode keyCode, ButtonFunction buttonFunction)
		{
			if (buttonDown.FindIndex(entry => entry.KeyKode == keyCode) == -1)
			{
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Down));
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonFunction, ButtonEvent.Pressed));
				buttonDown.Add((keyCode, buttonFunction));
			}
		}

		private void SendButtonFunctionUp(KeyCode keyCode)
		{
			var idx = buttonDown.FindIndex(entry => entry.KeyKode == keyCode);
			if (idx != -1)
			{
				ButtonManager.PostButtonEvent(new ButtonEventData(buttonDown[idx].ButtonFunction, ButtonEvent.Up));
				buttonDown.RemoveAt(idx);
			}
		}

		private void RegisterForFrameUpdate()
		{
			if (!isForFrameUpdateRegistered && FrameUpdateService.Update != null)
			{
				FrameUpdateService.Update.OnFrameUpdate += OnFrameUpdate;
				isForFrameUpdateRegistered = true;
			}
		}

		private void UnRegisterFromFrameUpdate()
		{
			if (isForFrameUpdateRegistered && FrameUpdateService.Update != null)
			{
				FrameUpdateService.Update.OnFrameUpdate -= OnFrameUpdate;
				isForFrameUpdateRegistered = false;
			}
		}

		private void OnDestroy()
		{
			UnRegisterFromFrameUpdate();
		}

		#endregion
	}
}