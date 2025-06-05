using System.Collections;
using UnityEngine;

namespace Midas.Presentation.Reels.SmartSymbols
{
	[RequireComponent(typeof(Animator))]
	public sealed class AnimatorReelStopAnimation : MonoBehaviour, IReelStopAnimation
	{
		private Animator animator;

		private int layerIndex;
		private int idleStateHash;
		private int playStateHash;

		[SerializeField]
		private string layerName = "Smart Layer";

		[SerializeField]
		private string idleStateName = "Idle";

		[SerializeField]
		private string playStateName = "Play";

		private void Awake()
		{
			animator = GetComponent<Animator>();

			for (var i = 0; i < animator.layerCount; i++)
			{
				var ln = animator.GetLayerName(i);

				if (ln == layerName)
				{
					layerIndex = i;
				}
			}

			idleStateHash = Animator.StringToHash(idleStateName);
			playStateHash = Animator.StringToHash(playStateName);

			IsReelStopAnimationFinished = true;
		}

		private void OnDisable()
		{
			IsReelStopAnimationFinished = true;
		}

		public bool IsReelStopAnimationFinished { get; private set; }

		public void PlayReelStopAnimation()
		{
			StopAllCoroutines();

			StartCoroutine(Coroutine());
		}

		public void StopReelStopAnimation()
		{
			StopAllCoroutines();

			animator.Play(idleStateHash, layerIndex, 0);
			animator.Update(0);

			IsReelStopAnimationFinished = true;
		}

		private IEnumerator Coroutine()
		{
			IsReelStopAnimationFinished = false;

			animator.Play(playStateHash, layerIndex, 0);
			animator.Update(0);

			while (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(playStateName))
			{
				yield return null;
			}

			IsReelStopAnimationFinished = true;
		}
	}
}