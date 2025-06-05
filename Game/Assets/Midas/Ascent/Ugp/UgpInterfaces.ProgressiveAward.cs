using System;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpProgressiveAward progressiveAward;

		/// <summary>
		/// Event raised when the progressive award has been verified.
		/// </summary>
		public event EventHandler<ProgressiveAwardVerifiedEventArgs> ProgressiveAwardVerified;

		/// <summary>
		/// Event raised when the progressive award has been paid.
		/// </summary>
		public event EventHandler<ProgressiveAwardPaidEventArgs> ProgressiveAwardPaid;

		private void InitProgressiveAward()
		{
			progressiveAward = GameLib.GetInterface<IUgpProgressiveAward>();
			if (progressiveAward != null)
			{
				progressiveAward.ProgressiveAwardVerified += OnProgressiveAwardVerified;
				progressiveAward.ProgressiveAwardPaid += OnProgressiveAwardPaid;
			}
		}

		private void DeInitProgressiveAward()
		{
			if (progressiveAward != null)
			{
				progressiveAward.ProgressiveAwardVerified -= OnProgressiveAwardVerified;
				progressiveAward.ProgressiveAwardPaid -= OnProgressiveAwardPaid;
			}

			progressiveAward = null;
		}

		/// <summary>
		/// Informs the foundation that the progressive award is started.
		/// </summary>
		/// <param name="progressiveAwardIndex">
		/// The index of the progressive award.
		/// </param>
		/// <param name="progressiveLevelId">
		/// The level ID of the progressive award.
		/// </param>
		/// <param name="defaultVerifiedAmount">
		/// The default verified amount of the progressive award, in units of base denom.
		/// </param>
		public void StartingProgressiveAward(int progressiveAwardIndex, string progressiveLevelId, long defaultVerifiedAmount)
		{
			if (progressiveAward != null)
				progressiveAward.StartingProgressiveAward(progressiveAwardIndex, progressiveLevelId, defaultVerifiedAmount);
			else
			{
				ProgressiveAwardVerified?.Invoke(null, new ProgressiveAwardVerifiedEventArgs
				{
					ProgressiveAwardIndex = progressiveAwardIndex,
					ProgressiveLevelId = progressiveLevelId,
					VerifiedAmount = defaultVerifiedAmount
				});
			}
		}

		/// <summary>
		/// Informs the foundation that the award display has finished. Payment can then be performed.
		/// </summary>
		/// <param name="progressiveAwardIndex">
		/// The index of the progressive award.
		/// </param>
		/// <param name="progressiveLevelId">
		/// The level ID of the progressive award.
		/// </param>
		/// <param name="defaultPaidAmount">
		/// The default paid amount of the progressive award, in units of base denom.
		/// </param>
		public void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount)
		{
			if (progressiveAward != null)
				progressiveAward.FinishedDisplay(progressiveAwardIndex, progressiveLevelId, defaultPaidAmount);
			else
			{
				ProgressiveAwardPaid?.Invoke(null, new ProgressiveAwardPaidEventArgs
				{
					ProgressiveAwardIndex = progressiveAwardIndex,
					ProgressiveLevelId = progressiveLevelId,
					PaidAmount = defaultPaidAmount
				});
			}
		}

		private void OnProgressiveAwardVerified(object s, ProgressiveAwardVerifiedEventArgs e)
		{
			ProgressiveAwardVerified?.Invoke(s, e);
		}

		private void OnProgressiveAwardPaid(object s, ProgressiveAwardPaidEventArgs e)
		{
			ProgressiveAwardPaid?.Invoke(s, e);
		}
	}
}