using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	public sealed class UnityAnimationSequencePlayable : MonoBehaviour, ISequencePlayable
	{
		private int layerIndex;
		private int stateNameHash;
		private int resetStateNameHash;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private string layerName;

		[SerializeField]
		private string stateName;

		[SerializeField]
		private string resetStateName;

		[SerializeField]
		private bool blockSequence;

		private void Awake()
		{
			if (animator == null)
				return;

			for (var i = 0; i < animator.layerCount; i++)
			{
				if (animator.GetLayerName(i) == layerName)
					layerIndex = i;
			}

			stateNameHash = Animator.StringToHash(stateName);
			resetStateNameHash = Animator.StringToHash(resetStateName);
		}

		public void StartPlay()
		{
			if (!animator)
				return;

			animator.Play(stateNameHash, layerIndex, 0f);
			animator.Update(0);
		}

		public void StopPlay(bool reset)
		{
			if (reset)
			{
				animator.Play(resetStateNameHash, layerIndex, 0f);
				animator.Update(0);
			}
		}

		public bool IsPlaying()
		{
			if (!blockSequence) return false;

			var si = animator.GetCurrentAnimatorStateInfo(layerIndex);
			return si.IsName(stateName) && si.normalizedTime < 1;
		}
	}
}