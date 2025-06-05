using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Audio;
using UnityEngine;

namespace Midas.Presentation.Reels.Sound
{
	public sealed class ReelSoundController : MonoBehaviour, IReelSpinStateEventHandler
	{
		private readonly Dictionary<SoundId, ISound> stopSounds = new Dictionary<SoundId, ISound>();
		private ISound anticipationSound;
		private ISound spinStartSound;
		private List<HashSet<SoundId>> smartSoundSets;

		[SerializeField]
		private List<ReelStopSounds> stopSoundEntries;

		[SerializeField]
		private List<ReelStopTriggerQualifyingSound> triggerQualifyingEntries;

		[SerializeField]
		private SoundId[] defaultStopSoundIds;

		[SerializeField]
		private SoundId anticipationSoundId;

		[SerializeField]
		private SoundId spinStartSoundId;

		[SerializeField]
		private ReelSpinStateEvent reelSpinEvent;

		private void Awake()
		{
			anticipationSound = ConstructSound(anticipationSoundId);
			spinStartSound = ConstructSound(spinStartSoundId);

			foreach (var soundId in defaultStopSoundIds)
			{
				if (!stopSounds.ContainsKey(soundId))
					stopSounds.Add(soundId, ConstructSound(soundId));
			}

			if (stopSoundEntries != null)
			{
				foreach (var soundId in stopSoundEntries.SelectMany(e => e.SoundIds))
				{
					if (soundId.IsValid && !stopSounds.ContainsKey(soundId))
						stopSounds.Add(soundId, ConstructSound(soundId));
				}
			}

			if (triggerQualifyingEntries != null)
			{
				foreach (var soundId in triggerQualifyingEntries.Select(e => e.SoundId))
				{
					if (!stopSounds.ContainsKey(soundId))
						stopSounds.Add(soundId, ConstructSound(soundId));
				}
			}

			ISound ConstructSound(SoundId soundId)
			{
				return soundId is { IsValid: true } ? AudioService.CreateSound(soundId) : null;
			}
		}

		private void OnEnable()
		{
			reelSpinEvent.Register(this);
		}

		private void OnDisable()
		{
			reelSpinEvent.Unregister(this);
		}

		private void OnDestroy()
		{
			if (anticipationSound != null)
			{
				AudioService.DestroySound(anticipationSound);
				anticipationSound = null;
			}

			foreach (var sound in stopSounds.Values)
				AudioService.DestroySound(sound);

			if (stopSoundEntries != null)
			{
				foreach (var entry in stopSoundEntries)
					entry.DeInit();
			}

			if (triggerQualifyingEntries != null)
			{
				foreach (var entry in triggerQualifyingEntries)
					entry.DeInit();
			}
		}

		public void SpinStarted(ReelContainer container)
		{
			spinStartSound?.Play();

			var groups = container.GetReelGroups();
			if (smartSoundSets == null)
			{
				smartSoundSets = new List<HashSet<SoundId>>(groups.Count);
				for (var i = 0; i < groups.Count; i++)
					smartSoundSets.Add(new HashSet<SoundId>());
			}
			else
			{
				foreach (var s in smartSoundSets)
					s.Clear();
			}

			foreach (var entry in stopSoundEntries)
			{
				entry.Apply(container, smartSoundSets);
			}

			for (var i = 0; i < smartSoundSets.Count; i++)
			{
				var soundSet = smartSoundSets[i];

				if (soundSet.Count == 0)
					soundSet.Add(defaultStopSoundIds[i]);
			}
		}

		public void SpinSlammed(ReelContainer container)
		{
		}

		public void SpinFinished(ReelContainer container)
		{
		}

		public void ReelStateChanged(ReelContainer container, int reelGroupIndex, ReelSpinState spinState)
		{
			if (spinState != ReelSpinState.Overshooting)
				return;

			foreach (var soundId in smartSoundSets[reelGroupIndex])
			{
				stopSounds[soundId]?.Play();
			}

			foreach (var triggerQualifyingEntry in triggerQualifyingEntries)
			{
				var soundId = triggerQualifyingEntry.GetQualifyingStopSound(container, reelGroupIndex);

				if (soundId != null)
					stopSounds[soundId]?.Play();
			}
		}

		public void ReelAnticipating(ReelContainer container, int reelGroupIndex)
		{
			anticipationSound?.Play();
		}

		public void Interrupt(bool immediate)
		{
			if (immediate)
			{
				anticipationSound?.Stop();
				spinStartSound?.Stop();

				foreach (var sound in stopSounds.Values)
					sound.Stop();
			}
		}

		public bool IsFinished => true;

#if UNITY_EDITOR

		public void ConfigureForMakeGame(ReelSpinStateEvent spinEvent)
		{
			reelSpinEvent = spinEvent;
			defaultStopSoundIds = new[]
			{
				new SoundId("ReelStop1", "Reels"),
				new SoundId("ReelStop2", "Reels"),
				new SoundId("ReelStop3", "Reels"),
				new SoundId("ReelStop4", "Reels"),
				new SoundId("ReelStop5", "Reels")
			};

			anticipationSoundId = new SoundId("suspend CD", "Reels");
		}

#endif
	}
}