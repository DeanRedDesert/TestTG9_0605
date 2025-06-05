using System.Collections;
using UnityEngine;

namespace Midas.Presentation.Transitions
{
	public sealed class AnimationPresentation : InterruptablePresentation
	{
		#region Fields

		private bool isFinished;
		private int layerIndex;
		private int idleStateHash;
		private int playStateHash;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private bool disableRootAtEnd;

		[SerializeField]
		private GameObject rootObject;

		[SerializeField]
		private Animator animator;

		[SerializeField]
		private string layerName = "Base Layer";

		[SerializeField]
		private string idleStateName = "Idle";

		[SerializeField]
		private string playStateName = "Play";

		#endregion

		#region Overrides of InterruptablePresentation

		/// <inheritdoc />
		public override bool IsFinished => isFinished;

		/// <inheritdoc />
		public override void Show()
		{
			StartCoroutine(RunAnimation());
		}

		/// <inheritdoc />
		public override void Interrupt()
		{
			StopAllCoroutines();

			// Use the idle state to reset everything back to initial state.
			// This ensures that the default values in the animator are correct when the animator is next enabled.

			animator.Play(idleStateHash, layerIndex, 0);
			animator.Update(0);

			if (rootObject)
				rootObject.SetActive(false);
		}

		#endregion

		#region Private Methods

		private IEnumerator RunAnimation()
		{
			if (rootObject)
				rootObject.SetActive(true);

			layerIndex = animator.GetLayerIndex(layerName);
			idleStateHash = Animator.StringToHash(idleStateName);
			playStateHash = Animator.StringToHash(playStateName);

			isFinished = false;
			animator.Play(playStateHash, layerIndex, 0);
			animator.Update(0);

			while (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash != idleStateHash)
				yield return null;

			if (rootObject != null && disableRootAtEnd)
				rootObject.SetActive(false);

			isFinished = true;
		}

		#endregion
	}
}