using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Presentation;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Game.Stages.Common.PreShow
{
	/// <summary>
	/// Uses an animator to control the darkening of a game object.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public sealed class AnimatorDarkener : MonoBehaviour, IDarkener
	{
		#region Fields

		private Coroutine coroutine;
		private Animator animator;
		private int layerIndex;
		private int idleStateHash;
		private int darkenStateHash;
		private int lightenStateHash;
		private bool isPlaying;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private string layerName;

		[SerializeField]
		private string idleStateName;

		[SerializeField]
		private string darkenStateName;

		[SerializeField]
		private string lightenStateName;

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			animator = GetComponent<Animator>();

			for (var i = 0; i < animator.layerCount; i++)
			{
				if (animator.GetLayerName(i) == layerName)
					layerIndex = i;
			}

			idleStateHash = Animator.StringToHash(idleStateName);
			darkenStateHash = Animator.StringToHash(darkenStateName);
			lightenStateHash = Animator.StringToHash(lightenStateName);
		}

		private void OnDestroy()
		{
			if (coroutine != null)
			{
				coroutine.Stop();
				coroutine = null;
			}
		}

		#endregion;

		#region IDarkener Implementation

		public void Idle(bool immediate)
		{
			if (coroutine == null)
				return;

			coroutine.Stop();
			coroutine = null;

			if (immediate)
			{
				animator.Play(idleStateHash, layerIndex, 0f);
				animator.Update(0);
			}
			else
			{
				if (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == darkenStateHash)
				{
					animator.Play(lightenStateHash, layerIndex, 0f);
				}
			}
		}

		public void DarkenThenLighten(TimeSpan darkenLength)
		{
			coroutine = FrameUpdateService.Update.StartCoroutine(DarkenLightenRoutine(darkenLength));
		}

		private IEnumerator<CoroutineInstruction> DarkenLightenRoutine(TimeSpan darkenLength)
		{
			animator.Play(darkenStateHash, layerIndex, 0f);
			yield return new CoroutineDelay(darkenLength);
			animator.Play(lightenStateHash, layerIndex, 0f);
		}

		#endregion
	}
}