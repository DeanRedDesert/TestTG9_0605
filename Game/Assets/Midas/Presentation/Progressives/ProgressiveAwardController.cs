using Midas.LogicToPresentation.Data.Services;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation.Progressives
{
	public abstract class ProgressiveAwardController : IPresentationController
	{
		/// <summary>
		/// Gets whether the progressive award at the provided index is payable by this progressive award controller.
		/// </summary>
		/// <param name="awardIndex">The index of the progressive award.</param>
		/// <returns>True if the progressive can be awarded by this controller, otherwise false.</returns>
		public bool CanAwardProgressive(int awardIndex)
		{
			if (IsAwarding)
			{
				Log.Instance.Error("Attempted to award progressives while already awarding one");
				return false;
			}

			var pa = StatusDatabase.ProgressiveStatus.ProgressiveAwards;
			if (pa == null || awardIndex < 0 || awardIndex >= pa.Count)
				return false;

			var progressiveToAward = pa[awardIndex];
			if (!CanAwardProgressive(progressiveToAward))
				return false;

			if (progressiveToAward.State == ProgressiveAwardState.Cleared || awardIndex == 0 || pa[awardIndex - 1].State == ProgressiveAwardState.Cleared)
				return true;

			Log.Instance.Error("Attempted to award progressives out of order");
			return false;
		}

		/// <summary>
		/// Instruct the controller to start awarding.
		/// </summary>
		/// <param name="awardIndex">The index of the progressive award to pay.</param>
		public void StartProgressiveAward(int awardIndex)
		{
			if (!CanAwardProgressive(awardIndex))
				return;

			AwardProgressive(awardIndex);
		}

		public abstract bool IsAwarding { get; }

		protected abstract bool CanAwardProgressive(ProgressiveAwardServiceData award);
		protected abstract void AwardProgressive(int awardIndex);

		public abstract void Init();
		public abstract void DeInit();
		public abstract void Destroy();
	}
}