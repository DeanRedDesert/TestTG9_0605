using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	public class AnimatorReelSymbolAnimation : MonoBehaviour, IReelSymbolAnimation
	{
		#region Fields

		private int playState;
		private int stopState;

		#endregion

		#region Inspector Fields

		/// <summary>
		/// The animator that will be controlled.
		/// </summary>
		[SerializeField]
		private Animator animatorObject;

		/// <summary>
		/// The name of the state that should be started when the symbol is played.
		/// </summary>
		[SerializeField]
		private string playStateName = "WinCycle";

		/// <summary>
		/// The name of the state that should be started when the symbol is stopped.
		/// </summary>
		[SerializeField]
		private string stopStateName = "Idle";

		#endregion

		#region Implementation of IReelSymbolAnimation

		/// <summary>
		/// Tell this symbol to start playing.
		/// </summary>
		public void Play(IWinInfo winInfo)
		{
			animatorObject.Play(playState, 0, 0f);
		}

		/// <summary>
		/// Tell this symbol to stop playing.
		/// </summary>
		public void Stop()
		{
			if (!animatorObject.gameObject.activeInHierarchy)
				return;

			animatorObject.Play(stopState, 0, 0f);
			// Tell the animator to force an update this frame (normally this wouldn't occur until the next frame)
			animatorObject.Update(0);
		}

		#endregion

		#region Unity Hooks

		/// <summary>
		/// Called on Unity awake.
		/// </summary>
		protected virtual void Awake()
		{
			if (animatorObject == null)
				animatorObject = GetComponentInChildren<Animator>();

			// Ids are used for optimized setters and getters on parameters.
			playState = Animator.StringToHash(playStateName);
			stopState = Animator.StringToHash(stopStateName);
		}

		private void OnDisable()
		{
			animatorObject.WriteDefaultValues();
		}

		#endregion
	}
}