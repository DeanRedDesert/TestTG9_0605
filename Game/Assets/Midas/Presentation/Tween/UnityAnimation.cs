using System.Collections.Generic;
using Midas.Presentation.Sequencing;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Tween
{
	public sealed class UnityAnimation : TweenAnimation
	{
		private UnityClip runningTween;
		private bool stopped = true;
		private bool aborted;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private string stopTrigger = "Stop";

		[SerializeField]
		private List<string> idleStates = new List<string> { "Idle" };

		[SerializeField]
		private bool disableAnimatorIfNotRunning;

		[SerializeField]
		private UnityEvent clipStopped;

		public override void Play(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs)
		{
			if (!stopped)
			{
				Log.Instance.Error($"can not start a new tween {tweenClip} the old one is not stopped");
				return;
			}

			if (runningTween != null)
			{
				Log.Instance.Error($"can not start a second tween {tweenClip} while {runningTween} is still running.");
				return;
			}

			if (!(tweenClip is UnityClip unityClip))
			{
				Log.Instance.Error($"{tweenClip} can not converted into (UnityClip).");
				return;
			}

			runningTween = unityClip;
			stopped = false;
			aborted = false;
			animator.enabled = true;
			animator.SetTrigger(unityClip.NameHash);
		}

		public override void Stop(TweenClip tweenClip)
		{
			Stop();
		}

		public override void Interrupt(TweenClip tweenClip, SequenceEventArgs sequenceEventArgs)
		{
		}

		public override void Stop()
		{
			if (runningTween != null && !stopped)
			{
				aborted = true;
				animator.SetTrigger(stopTrigger);
				stopped = true;
			}
		}

		public override bool IsPlaying()
		{
			return runningTween != null;
		}

		public override bool IsPlaying(TweenClip tweenClip)
		{
			return runningTween == tweenClip;
		}

		public override bool IsAborted()
		{
			return runningTween != null && aborted;
		}

		public override bool IsAborted(TweenClip tweenClip)
		{
			return runningTween == tweenClip && aborted;
		}

		public sealed class UnityClip : TweenClip
		{
			#region Public

			public UnityClip(string name, int nameHash)
				: base(name)
			{
				NameHash = nameHash;
			}

			public int NameHash { get; }

			#endregion
		}

		private void Update()
		{
			if (runningTween == null)
				return;

			var startTriggered = animator.GetBool(runningTween.NameHash);
			if (startTriggered)
				return;

			var currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			var nextStateInfo = animator.GetNextAnimatorStateInfo(0);
			var layerName = animator.GetLayerName(0);

			foreach (var idleState in idleStates)
			{
				var isCurrentStateIdle = currentStateInfo.IsName($"{layerName}.{idleState}") || currentStateInfo.fullPathHash == 0;
				var isNextStateIdle = nextStateInfo.IsName($"{layerName}.{idleState}") || nextStateInfo.fullPathHash == 0;
				if (isCurrentStateIdle && isNextStateIdle)
				{
					RaiseClipStopped(runningTween, aborted);
					runningTween = null;
					stopped = true;
					if (disableAnimatorIfNotRunning)
					{
						animator.enabled = false;
					}

					clipStopped?.Invoke();
					break;
				}
			}
		}

		protected override List<TweenClip> FindTweenClips()
		{
			var tweenClips = new List<TweenClip>();
			var parameters = animator.parameters;
			foreach (var parameter in parameters)
			{
				if (parameter.type == AnimatorControllerParameterType.Trigger)
				{
					tweenClips.Add(new UnityClip(parameter.name, parameter.nameHash));
				}
			}

			return tweenClips;
		}

		private void Reset()
		{
			animator = GetComponent<Animator>();
			if (animator == null)
			{
				animator = gameObject.AddComponent(typeof(Animator)) as Animator;
			}
		}
	}
}