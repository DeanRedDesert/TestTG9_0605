using System.Threading.Tasks;
using Midas.Presentation.Audio;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceSound : SequenceSkippable
	{
		private ISound soundOnEnter;
		private ISound blockingSound;
		private ISound soundOnExit;

		[Space(12)]
		[SerializeField]
		private SoundId soundOnEnterId = new SoundId();

		[SerializeField]
		private SoundId blockingSoundId = new SoundId();

		[SerializeField]
		private SoundId soundOnExitId = new SoundId();

		protected override void OnStateEnter(SequenceEventArgs sequenceEventArgs, TaskCompletionSource<int> completionSource)
		{
			base.OnStateEnter(sequenceEventArgs, completionSource);
			if (HasNeededIntensity(sequenceEventArgs))
			{
				soundOnEnter?.Play();
			}
		}

		protected override void OnStateExit(SequenceEventArgs sequenceEventArgs)
		{
			base.OnStateExit(sequenceEventArgs);
			if (HasNeededIntensity(sequenceEventArgs))
			{
				soundOnExit?.Play();
			}
		}

		protected override void PlaySkippableSequence(SequenceEventArgs sequenceEventArgs)
		{
			blockingSound?.Play();
		}

		protected override void Awake()
		{
			base.Awake();
			blockingSound = CreateSound(blockingSoundId);
			soundOnEnter = CreateSound(soundOnEnterId);
			soundOnExit = CreateSound(soundOnExitId);
		}

		private void OnDestroy()
		{
			AudioService.DestroySound(blockingSound);
			AudioService.DestroySound(soundOnEnter);
			AudioService.DestroySound(soundOnExit);
			blockingSound = null;
			soundOnEnter = null;
			soundOnExit = null;
		}

		protected override void StopSkippableSequence(SequenceEventArgs sequenceEventArgs)
		{
			soundOnEnter?.Stop();
			blockingSound?.Stop();
		}

		protected override bool IsInterruptibleSequencePlaying => blockingSound?.IsPlaying ?? false;

		private ISound CreateSound(SoundId soundId)
		{
			if (soundId.Id == "")
				return null;

			var sound = AudioService.CreateSound(soundId);
			if (sound != null)
				return sound;

			Log.Instance.Error($"Sound '{soundId}' not found in {gameObject.GetPath()}", this);
			return null;
		}
	}
}