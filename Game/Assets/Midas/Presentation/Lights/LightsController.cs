using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation.Lights
{
	public sealed class LightsController : IPresentationController
	{
		private LightsStatus lightsStatus;
		private Coroutine lightsCoroutine;

		private readonly Dictionary<LightsBase, LightsDetails> registeredLights = new Dictionary<LightsBase, LightsDetails>();
		private readonly List<LightsDetails> activeLights = new List<LightsDetails>();

		public void Init()
		{
			lightsStatus = StatusDatabase.LightsStatus;
			lightsCoroutine = FrameUpdateService.Update.StartCoroutine(LightsCoroutine());
		}

		public void DeInit()
		{
			CabinetManager.Cabinet.StopLights();
			lightsStatus.ActiveOneShot = null;
			lightsCoroutine?.Stop();
			lightsCoroutine = null;
			lightsStatus = null;
		}

		public void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> LightsCoroutine()
		{
			LightsDetails currentLight = null;
			var oneShotExpiry = TimeSpan.Zero;

			while (true)
			{
				while (!CabinetManager.Cabinet.IsActive)
					yield return null;

				while (StatusDatabase.GameStatus.GameLogicPaused)
					yield return null;

				while (true)
				{
					if (!CabinetManager.Cabinet.IsActive || StatusDatabase.GameStatus.GameLogicPaused)
						break;

					// Get the highest priority light and see if it has changed.

					if (lightsStatus.ActiveOneShot != null)
					{
						if (!ReferenceEquals(currentLight, lightsStatus.ActiveOneShot))
						{
							currentLight = lightsStatus.ActiveOneShot;
							var duration = CabinetManager.Cabinet.PlayLights(currentLight.Handle, false);
							oneShotExpiry = FrameTime.CurrentTime + duration;
						}

						if (FrameTime.CurrentTime < oneShotExpiry)
						{
							yield return null;
							continue;
						}

						// If we get here the light sequence has ended

						lightsStatus.ActiveOneShot = null;
					}

					var highestPriorityLight = activeLights.LastOrDefault();

					if (!ReferenceEquals(highestPriorityLight, currentLight))
					{
						currentLight = highestPriorityLight;
						if (currentLight == null)
							CabinetManager.Cabinet.StopLights();
						else
							CabinetManager.Cabinet.PlayLights(currentLight.Handle, true);
					}
					else
					{
						yield return null;
					}
				}

				CabinetManager.Cabinet.StopLights();
				currentLight = null;
				lightsStatus.ActiveOneShot = null;
			}
		}

		public void Play(LightsBase light, bool loop)
		{
			if (!registeredLights.TryGetValue(light, out var lightsDetails))
			{
				lightsDetails = lightsStatus.RegisterLights(light);
				registeredLights.Add(light, lightsDetails);
			}

			if (!loop)
			{
				if (lightsStatus.ActiveOneShot == null || lightsStatus.ActiveOneShot.Priority <= light.Priority)
					lightsStatus.ActiveOneShot = lightsDetails;

				return;
			}

			activeLights.Remove(lightsDetails);
			var i = 0;
			for (; i < activeLights.Count; i++)
			{
				var activeLight = activeLights[i];
				if (activeLight.Priority > light.Priority)
					break;
			}

			activeLights.Insert(i, lightsDetails);
			lightsStatus.ActiveLights = activeLights;
		}

		public void Stop(LightsBase light)
		{
			if (!registeredLights.TryGetValue(light, out var lightsDetails))
				return;

			if (ReferenceEquals(lightsStatus.ActiveOneShot, lightsDetails))
				lightsStatus.ActiveOneShot = null;

			activeLights.Remove(lightsDetails);
		}
	}
}