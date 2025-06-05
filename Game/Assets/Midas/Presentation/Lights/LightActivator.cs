using System.Collections.Generic;
using Midas.Presentation.Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Lights
{
	public sealed class LightActivator : MonoBehaviour, ILightOwner
	{
		private enum AutoPlay
		{
			None,
			OnAwake,
			OnEnable
		}

		private LightsController lightsController;
		private IReadOnlyList<LightsBase> ownedLights;

		[FormerlySerializedAs("light")]
		[SerializeField]
		private LightsBase lights;

		[SerializeField]
		private bool loop;

		[SerializeField]
		private AutoPlay autoPlay;

		public void Play()
		{
			lightsController?.Play(lights, loop);
		}

		public void Stop()
		{
			lightsController?.Stop(lights);
		}

		private void Awake()
		{
			lightsController = GameBase.GameInstance.GetPresentationController<LightsController>();
			if (autoPlay == AutoPlay.OnAwake)
				Play();
		}

		private void OnEnable()
		{
			if (autoPlay == AutoPlay.OnEnable)
				Play();
		}

		private void OnDisable()
		{
			if (autoPlay == AutoPlay.OnEnable)
				Stop();
		}

		public IReadOnlyList<LightsBase> Lights => ownedLights ??= new[] { lights };
	}
}