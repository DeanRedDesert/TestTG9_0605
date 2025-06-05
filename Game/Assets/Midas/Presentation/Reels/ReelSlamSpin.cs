using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	public sealed class ReelSlamSpin : MonoBehaviour, IInterruptable, IReelSpinStateEventHandler
	{
		#region Fields

		private InterruptController interruptController;
		private ReelSpinController reelContainer;
		private bool registeredForInterrupt;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private ReelSpinStateEvent reelSpinEvent;

		#endregion

		#region IInterruptable Methods

		public bool CanBeInterrupted => reelContainer.IsSpinning && StatusDatabase.ConfigurationStatus.IsSlamSpinAllowed;
		public bool CanBeAutoInterrupted => false;
		public int InterruptPriority { get; private set; }
		public void Interrupt() => reelContainer.SlamReels(StatusDatabase.ConfigurationStatus.IsSlamSpinImmediate);

		#endregion

		#region Unity Methods

		private void Awake()
		{
			reelContainer = GetComponent<ReelSpinController>();
			InterruptPriority = 10;

			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
		}

		private void OnEnable() => reelSpinEvent.Register(this);

		private void OnDisable() => reelSpinEvent.Unregister(this);

		#endregion

		#region IReelSpinStateEventHandler

		public void SpinStarted(ReelContainer container) => RegisterForInterrupt();

		public void SpinSlammed(ReelContainer container)
		{
		}

		public void SpinFinished(ReelContainer container) => UnRegisterFromInterrupt();

		public void ReelStateChanged(ReelContainer container, int reelGroupIndex, ReelSpinState spinState)
		{
		}

		public void ReelAnticipating(ReelContainer container, int reelGroupIndex)
		{
		}

		public void Interrupt(bool immediate) => UnRegisterFromInterrupt();

		public bool IsFinished => true;

		#endregion

		#region Private Methods

		private void RegisterForInterrupt()
		{
			if (registeredForInterrupt || !CanBeInterrupted)
				return;

			interruptController.AddInterruptable(this);
			registeredForInterrupt = true;
		}

		private void UnRegisterFromInterrupt()
		{
			if (!registeredForInterrupt)
				return;

			interruptController.RemoveInterruptable(this);
			registeredForInterrupt = false;
		}

		#endregion

#if UNITY_EDITOR

		public void ConfigureForMakeGame(ReelSpinStateEvent spinEvent) => reelSpinEvent = spinEvent;

#endif
	}
}