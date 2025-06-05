using System;
using Midas.Core.General;
using Midas.Presentation.Data;
using UnityEngine;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffDialController : MonoBehaviour
	{
		/// <summary>
		/// The number of distinct odds steps that can be displayed. This is also the number of images available to display the dial showing the distinct odds.
		/// </summary>
		private const int NrOfOddsSteps = 100;

		/// <summary>
		/// A 3 degree wedge which is the maximum odds for display.
		/// This indicates the maximum percentage (and minimum) percentage that can be displayed.
		/// </summary>
		private const double MaximumOdds = 0.99;

		/// <summary>
		/// The minimum distance the stopped needle keeps from the borders of the win or lose area.
		/// If the actual stop position resulting from the random numbers is too close to a border, the
		/// stop position is moved away from the border at least this distance if possible.
		/// </summary>
		private const double MinimumDistance = 0.01;

		private static readonly int percentage = Shader.PropertyToID("_Percentage");
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private double winningOdds;
		private bool clockUpdatingLocked;

		[SerializeField]
		private SpriteRenderer dialSprite;

		#region Unity Methods

		private void OnEnable()
		{
			var cps = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
			autoUnregisterHelper.RegisterPropertyChangedHandler(cps, nameof(CreditPlayoffStatus.TotalWeight), OnCreditBetChanged);
			autoUnregisterHelper.RegisterPropertyChangedHandler(cps, nameof(CreditPlayoffStatus.Weight), OnCreditBetChanged);
			UpdateWinningOdds(cps.Weight, cps.TotalWeight);
		}

		private void OnDisable() => autoUnregisterHelper.UnRegisterAll();

		#endregion

		#region Public

		/// <summary>
		///  Return the corrected stop position of the needle.
		///  The needle must stop in the correct area (red or green) but should keep a minimum distance to the borders
		///  of the area. This makes it easier for the player to tell if it is a win or a loss.
		///  If the area is too small to keep the minimum distance from both borders, the stop position is set to the middle of the area.
		/// </summary>
		/// <returns>The stop position of the needle.</returns>
		public float GetStopPosition(bool isWin, double stopPosition)
		{
			// Get the odds like they are displayed in the graphical representation (discretized).
			var discreteWinningOdds = GetDiscretizedOdds(winningOdds);

			// Calculate the target area size as displayed in the graphical representation (discretized).
			var discreteDomain = isWin ? discreteWinningOdds : 1.0 - discreteWinningOdds;

			// The allowed range is the discrete domain size subtracted the required distance to the borders on both
			// sides.
			var range = discreteDomain - 2.0 * MinimumDistance;

			if (range > 0.0)
			{
				// Calculate the precise target area size (not discretized).
				var continuousDomain = isWin ? winningOdds : 1.0 - winningOdds;

				// Calculate the relative stop position in the target area (not discretized).
				var relPos = isWin ? stopPosition / continuousDomain : (stopPosition - winningOdds) / continuousDomain;

				// Calculate the relative position of the target area (red or green part, discretized).
				var bias = isWin ? MinimumDistance : MinimumDistance + discreteWinningOdds;

				// Convert the stop position to the smaller range.
				stopPosition = relPos * range + bias;

				// Enforce safe space ranges. Safe space calcs can be violated due to clamping with MaximumOdds and
				// using the original LuckyNumber to try to divide the range.
				if (isWin)
				{
					if (stopPosition > winningOdds - MinimumDistance)
						stopPosition = winningOdds - MinimumDistance;
				}
				else
				{
					if (stopPosition < winningOdds + MinimumDistance)
						stopPosition = winningOdds + MinimumDistance;
				}
			}
			else
			{
				// If there is not enough space to keep the required distance to the borders
				// stop in the middle of the target area.
				stopPosition = isWin ? winningOdds * 0.5 : (1.0 + winningOdds) * 0.5;
			}

			// Because this controller straddles 12 o'clock rather than starts at 12 o'clock (original playoff implementation),
			// the original "percentage" calculations can be used with half the "angle" subtracted to determine the final position.
			stopPosition -= winningOdds * 0.5;
			while (stopPosition < 0.0)
				stopPosition += 1.0;

			return (float)stopPosition;
		}

		public bool ClockUpdatingLocked
		{
			get => clockUpdatingLocked;
			set
			{
				clockUpdatingLocked = value;
				if (clockUpdatingLocked)
					dialSprite.material.SetFloat(percentage, (float)winningOdds);
			}
		}

		#endregion

		#region Private

		private void OnCreditBetChanged(StatusBlock sender, string propertyname)
		{
			var cps = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
			UpdateWinningOdds(cps.Weight, cps.TotalWeight);
		}

		private void UpdateWinningOdds(long playOffBet, long totalBet)
		{
			winningOdds = playOffBet / (double)totalBet;

			// When either the green or red sector odd is 1% or less, a predetermined wedge of 3 degrees is displayed.
			// A 3 degree wedge is an odd of 99%.
			if (winningOdds > MaximumOdds)
				winningOdds = MaximumOdds;

			if (winningOdds < 1.0 - MaximumOdds)
				winningOdds = Math.Round(1.0 - MaximumOdds, 2, MidpointRounding.AwayFromZero);

			if (!ClockUpdatingLocked)
				dialSprite.material.SetFloat(percentage, (float)winningOdds);
		}

		/// <summary>
		/// Get the discretized odds from 0.0 to 1.0 (excluding 1.0). Returns only odds which can be exactly visualized by the available dial images.
		/// </summary>
		/// <param name="odds">The odds of winning from 0.0 to 1.0 (excluding 1.0).</param>
		/// <returns>The discretized odds in the range from 0.0 to 1.0 (excluding 1.0) corresponding to the given odds.</returns>
		private static double GetDiscretizedOdds(double odds) => (double)GetOddsIndex(odds) / NrOfOddsSteps;

		/// <summary>
		/// Get the odds index in the range 0 to NrOfOddsSteps - 1.
		/// The odds index is the transformation of the "continuous" range from 0.0 to 1.0 (excluding 1.0) to the discrete integer range 0 to NrOfOddsSteps - 1.
		/// Note: Don't use data type float. E.g. odds = 0.01f would lead to return 0 instead of correct value 1.
		/// </summary>
		/// <param name="odds">The odds of winning from 0.0 to 1.0 (excluding 1.0).</param>
		/// <returns>The odds index in the range from 0 to NrOfOddsSteps - 1 corresponding to the given odds.</returns>
		private static int GetOddsIndex(double odds) => (int)(odds * NrOfOddsSteps);

		#endregion
	}
}