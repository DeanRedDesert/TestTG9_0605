using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.Tween
{
	public class DoTweenAnimation : TweenAnimation
	{
		private readonly List<(TweenClip tweenClip, DG.Tweening.Sequence tweenSequence, bool aborted, Action onInterrupt)> runningClips = new List<(TweenClip, DG.Tweening.Sequence, bool, Action)>();

		#region Public

		public override void Play(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs)
		{
			if (tweenClip is DoTweenClip doTweenClip)
			{
				var runningClip = runningClips.FindIndex(c => c.tweenClip == tweenClip);
				if (runningClip != -1)
				{
					runningClips[runningClip].tweenSequence.Play();
					return;
				}

				var tweenSequence = DOTween.Sequence();

				Action onInterrupt = null;
				doTweenClip.ClipDelegate.Invoke(tweenSequence, sequenceEventArgs, ref onInterrupt);
				runningClips.Add((doTweenClip, tweenSequence, false, onInterrupt));

				tweenSequence.onComplete += () => TweenSequenceCompleted(doTweenClip, tweenSequence);
				tweenSequence.onKill += () => TweenSequenceKilled(doTweenClip);
			}
			else
			{
				Log.Instance.Error($"Wrong type of TweenClip {tweenClip.GetType()} in {this.GetPath()}", this);
			}
		}

		public override void Stop(TweenClip tweenClip)
		{
			for (var i = runningClips.Count - 1; i >= 0; --i)
			{
				var runningTween = runningClips[i];
				if (runningTween.tweenClip == tweenClip)
				{
					runningClips[i] = (runningTween.tweenClip, runningTween.tweenSequence, true, runningTween.onInterrupt);
					runningTween.tweenSequence.Complete();
				}
			}
		}

		public override void Stop()
		{
			for (var i = runningClips.Count - 1; i >= 0; --i)
			{
				var runningTween = runningClips[i];
				runningClips[i] = (runningTween.tweenClip, runningTween.tweenSequence, true, runningTween.onInterrupt);
				runningTween.tweenSequence.Complete();
			}
		}

		public override bool IsPlaying() => runningClips.Count != 0;

		public override bool IsPlaying(TweenClip tweenClip)
		{
			for (var i = 0; i < runningClips.Count; ++i)
			{
				if (runningClips[i].tweenClip == tweenClip)
					return true;
			}

			return false;
		}

		public override bool IsAborted()
		{
			for (var i = 0; i < runningClips.Count; ++i)
			{
				if (runningClips[i].aborted)
					return true;
			}

			return false;
		}

		public override bool IsAborted(TweenClip tweenClip)
		{
			for (var i = 0; i < runningClips.Count; ++i)
			{
				if (runningClips[i].tweenClip == tweenClip)
					return runningClips[i].aborted;
			}

			return false;
		}

		public override void Interrupt(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs)
		{
			for (var i = 0; i < runningClips.Count; ++i)
			{
				if (runningClips[i].tweenClip == tweenClip)
				{
					runningClips[i].tweenSequence.Goto(0f);
					runningClips[i].onInterrupt?.Invoke();
				}
			}
		}

		public sealed class DoTweenClip : TweenClip
		{
			#region Public

			public TweenClipDelegate ClipDelegate { get; }

			public DoTweenClip(string name, TweenClipDelegate tweenClipDelegate) : base(name)
			{
				ClipDelegate = tweenClipDelegate;
			}

			public delegate void TweenClipDelegate(DG.Tweening.Sequence sequence, SequenceEventArgs sequenceEventArgs, ref Action onInterrupt);

			#endregion
		}

		#endregion

		#region Protected

		protected override List<TweenClip> FindTweenClips()
		{
			var tweenClips = new List<TweenClip>();
			var type = GetType();
			foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
			{
				if (method.GetCustomAttribute<TweenAttribute>(false) == null)
					continue;

				try
				{
					var d = (DoTweenClip.TweenClipDelegate)Delegate.CreateDelegate(typeof(DoTweenClip.TweenClipDelegate), this, method);
					tweenClips.Add(new DoTweenClip(method.Name, d));
				}
				catch (Exception ex)
				{
					Log.Instance.Error($"TweenClip in {this} and method {method} has thrown an exception {ex}");
					throw;
				}
			}

			return tweenClips;
		}

		#endregion

		#region Private

		private void TweenSequenceKilled(DoTweenClip tweenClip) => Log.Instance.Warn($"Tween Killed called on {this.GetPath()} for {tweenClip.ClipDelegate.Target}."
			+ " This is most probably a mistake from another script in your game having old references to a DoTween Sequence");

		private void TweenSequenceCompleted(DoTweenClip tweenClip, DG.Tweening.Tween tweenSequence)
		{
			for (var i = 0; i < runningClips.Count; ++i)
			{
				if (runningClips[i].tweenClip != tweenClip || runningClips[i].tweenSequence != tweenSequence)
					continue;

				tweenSequence.onKill = null;
				RaiseClipStopped(runningClips[i].tweenClip, runningClips[i].aborted);
				runningClips.RemoveAt(i);
				break;
			}
		}

		#endregion
	}
}