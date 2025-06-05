using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Cabinet;
using Midas.Presentation.ButtonHandling;
using ButtonFunction = Midas.Presentation.ButtonHandling.ButtonFunction;
using static Midas.Ascent.Cabinet.AscentCabinet;

namespace Midas.Ascent.Cabinet
{
	using ButtonToButtonFunctionList = IReadOnlyList<(PhysicalButton Button, ButtonFunction ButtonFunction)>;

	internal sealed class ButtonLampController
	{
		private bool lightStateDirty;
		private bool acquired;
		private ButtonPanel buttonPanel;

		private readonly List<(PhysicalButton Button, int AnimationRange, bool HasLamp)> buttonAnimationRange =
			new List<(PhysicalButton Button, int AnimationRange, bool HasLamp)>();

		private readonly List<ButtonLampState> currentLampState = new List<ButtonLampState>();

		public void SetAcquired(bool isAcquired, ButtonPanel panel)
		{
			acquired = isAcquired;
			buttonPanel = panel;
			lightStateDirty = true;
		}

		public void SetButtonStates(IReadOnlyCollection<ButtonStateData> buttonEventData, ButtonToButtonFunctionList mapping)
		{
			buttonAnimationRange.Clear();

			foreach (var buttonStateData in buttonEventData)
			{
				foreach (var (button, buttonFunction) in mapping)
				{
					if (buttonFunction.Equals(buttonStateData.ButtonFunction))
					{
						SetButtonAnimationRange(button, buttonStateData);
						lightStateDirty = true;
					}
				}
			}
		}

		public void FrameUpdate(TimeSpan currentTime)
		{
			SetupLampState(currentTime);
			UpdateCabinetLibLampState();
		}

		private void SetButtonAnimationRange(PhysicalButton button, ButtonStateData buttonStateData)
		{
			int animationTime;
			switch (buttonStateData.LightState.State)
			{
				case LightState.LampState.LightOn:
					animationTime = 1;
					break;
				case LightState.LampState.LightOff:
					animationTime = 0;
					break;
				default:
					animationTime = buttonStateData.LightState.Frequency < float.Epsilon ? 1 : (int)(1000.0f / buttonStateData.LightState.Frequency + 0.5f) + 1;
					break;
			}

			buttonAnimationRange.Add((button, animationTime, buttonPanel.HasLamp(button)));
		}

		private void SetupLampState(TimeSpan time)
		{
			if (lightStateDirty)
			{
				RefreshLampState(time);
			}
			else
			{
				UpdateLampState(time);
			}
		}

		private void UpdateLampState(TimeSpan time)
		{
			var idx = 0;
			foreach (var (button, animationRange, hasLamp) in buttonAnimationRange)
			{
				if (hasLamp)
				{
					var on = AnimationRange2OnOff(time, animationRange);
					if (currentLampState[idx].State != on)
					{
						currentLampState[idx] = new ButtonLampState(new ButtonIdentifier(ButtonPanelLocation.Main, button.Id), on);
						lightStateDirty = true;
					}

					idx++;
				}
			}
		}

		private void RefreshLampState(TimeSpan time)
		{
			currentLampState.Clear();
			foreach (var (button, animationRange, hasLamp) in buttonAnimationRange)
			{
				if (hasLamp)
				{
					currentLampState.Add(new ButtonLampState(
						new ButtonIdentifier(ButtonPanelLocation.Main, button.Id),
						AnimationRange2OnOff(time, animationRange)));
				}
			}
		}

		private void UpdateCabinetLibLampState()
		{
			if (lightStateDirty)
			{
				try
				{
					if (acquired)
					{
						CabinetLib.SetLampState(currentLampState);
					}

					lightStateDirty = false;
				}
				catch (Exception e)
				{
					Log.Instance.Warn("Failed setting Lamp state", e);
				}
			}
		}

		private static bool AnimationRange2OnOff(TimeSpan currentTime, int animationRange)
		{
			if (animationRange >= 0 && animationRange <= 1)
			{
				return animationRange == 1;
			}

			if (animationRange > 1)
			{
				var frequency = (animationRange - 1) / 1000.0;
				var x = currentTime.TotalSeconds * frequency;
				x -= Math.Floor(x);
				var blink = Math.Sin(x * 2 * Math.PI);
				return blink >= 0;
			}

			throw new InvalidProgramException($"Wrong animationRange ={animationRange}");
		}
	}
}