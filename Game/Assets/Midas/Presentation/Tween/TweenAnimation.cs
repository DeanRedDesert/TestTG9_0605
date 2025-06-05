using System;
using System.Collections.Generic;
using System.Text;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Midas.Presentation.Tween
{
	public abstract class TweenAnimation : MonoBehaviour
	{
		private List<TweenClip> clips = new List<TweenClip>();

		#region Public

		public void Play(string tweenName, SequenceEventArgs sequenceEventArgs)
		{
			var tweenClips = GetTweenClips();
			var tweenClip = tweenClips.Find(t => t.Name == tweenName);
			if (tweenClip != null)
				Play(tweenClip, sequenceEventArgs);
		}

		public abstract void Play(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs);

		public abstract void Stop();

		public abstract void Stop(TweenClip tweenClip);

		public void Stop(string tweenName)
		{
			var tweenClips = GetTweenClips();
			var tweenClip = tweenClips.Find(t => t.Name == tweenName);
			if (tweenClip != null)
				Stop(tweenClip);
		}

		public void Interrupt(string tweenName, SequenceEventArgs sequenceEventArgs)
		{
			var tweenClips = GetTweenClips();
			var tweenClip = tweenClips.Find(t => t.Name == tweenName);
			if (tweenClip != null)
				Interrupt(tweenClip, sequenceEventArgs);
		}

		public abstract void Interrupt(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs);

		public List<TweenClip> GetTweenClips()
		{
			if (clips == null || clips.Count == 0)
				clips = FindTweenClips();

			return clips;
		}

		public bool IsPlaying(string tweenName)
		{
			var tweenClips = GetTweenClips();
			var tweenClip = tweenClips.Find(t => t.Name == tweenName);
			return IsPlaying(tweenClip);
		}

		public abstract bool IsPlaying(TweenClip tweenClip);

		public abstract bool IsPlaying();

		public bool IsAborted(string tweenName)
		{
			var tweenClips = GetTweenClips();
			var tweenClip = tweenClips.Find(t => t.Name == tweenName);
			return IsAborted(tweenClip);
		}

		public abstract bool IsAborted(TweenClip tweenClip);

		public abstract bool IsAborted();

		public event EventHandler<TweenAnimationClipStoppedEventArgs> ClipStopped;

		public abstract class TweenClip
		{
			#region Public

			public override string ToString() => Name;

			public string Name { get; }

			#endregion

			#region Protected

			protected TweenClip(string name) => Name = name;

			#endregion
		}

		public class TweenAnimationClipStoppedEventArgs : EventArgs
		{
			#region Public

			public TweenClip TweenClip { get; }
			public bool Aborted { get; }

			public TweenAnimationClipStoppedEventArgs(TweenClip tweenClip, bool aborted)
			{
				TweenClip = tweenClip;
				Aborted = aborted;
			}

			public override string ToString()
			{
				var builder = new StringBuilder();
				builder.AppendLine(base.ToString());
				builder.AppendLine($"TweenClip {TweenClip} Aborted {Aborted}");
				return builder.ToString();
			}

			#endregion
		}

		#endregion

		#region Protected

		protected abstract List<TweenClip> FindTweenClips();

		protected void RaiseClipStopped(TweenClip tweenClip, bool aborted) => ClipStopped?.Invoke(this, new TweenAnimationClipStoppedEventArgs(tweenClip, aborted));

		#endregion
	}
}