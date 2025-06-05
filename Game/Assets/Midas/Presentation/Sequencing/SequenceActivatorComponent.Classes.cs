using Midas.Presentation.Audio;
using Midas.Presentation.Tween;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceActivatorComponentISequencePlayable : SequenceActivatorComponent<ISequencePlayable>
	{
		#region Public

		public override void StartComponent(SequenceComponent p, SequenceEventArgs eventArgs)
		{
			Unwrap(p).StartPlay();
		}

		public override void StopComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool reset)
		{
			Unwrap(p).StopPlay(reset);
		}

		public override bool TimeOutComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool wcResetTimeout, bool finished)
		{
			var playable = Unwrap(p);
			if (wcResetTimeout)
			{
				playable.StopPlay(false);
			}

			if (playable.IsPlaying())
			{
				finished = false;
			}

			return finished;
		}

		#endregion
	}

	public sealed class SequenceActivatorComponentSoundPlayerBase : SequenceActivatorComponent<SoundPlayerBase>
	{
		#region Public

		public override void StartComponent(SequenceComponent p, SequenceEventArgs eventArgs)
		{
			Unwrap(p).Play();
		}

		public override void StopComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool reset)
		{
			Unwrap(p).Stop();
		}

		public override bool TimeOutComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool wcResetTimeout, bool finished)
		{
			var sound = Unwrap(p);
			if (wcResetTimeout)
			{
				sound.Stop();
			}

			if (sound.Sound.IsPlaying && !sound.IsStopping)
			{
				finished = false;
			}

			return finished;
		}

		#endregion
	}

	public sealed class SequenceActivatorComponentTweenAnimation : SequenceActivatorComponent<TweenAnimation>
	{
		#region Public

		public override void StartComponent(SequenceComponent p, SequenceEventArgs eventArgs)
		{
			Unwrap(p).Play(p.AnimationComponentClipName, eventArgs);
		}

		public override void StopComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool reset)
		{
			Unwrap(p).Stop(p.AnimationComponentClipName);
		}

		public override bool TimeOutComponent(SequenceComponent p, SequenceEventArgs eventArgs, bool wcResetTimeout, bool finished)
		{
			var arg = Unwrap(p);
			if (wcResetTimeout)
			{
				arg.Stop(p.AnimationComponentClipName);
			}

			if (arg.IsPlaying(p.AnimationComponentClipName))
			{
				finished = false;
			}

			return finished;
		}

		#endregion
	}
}