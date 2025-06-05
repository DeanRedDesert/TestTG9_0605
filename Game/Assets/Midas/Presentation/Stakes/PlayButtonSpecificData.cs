using System;
using Midas.Core;

namespace Midas.Presentation.Stakes
{
	public sealed class PlayButtonSpecificData : IStakeButtonSpecificData, IEquatable<PlayButtonSpecificData>
	{
		public bool IsCostToCover { get; }
		public IStakeCombination StakeCombination { get; }
		public bool IsSelected { get; }

		public PlayButtonSpecificData(IStakeCombination stakeCombination, bool isSelected, bool isCostToCover)
		{
			IsCostToCover = isCostToCover;
			StakeCombination = stakeCombination;
			IsSelected = isSelected;
		}

		#region Equality Implementation

		public bool Equals(PlayButtonSpecificData other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return IsSelected == other.IsSelected && IsStakeCombinationEqual(other.StakeCombination);
		}

		private bool IsStakeCombinationEqual(IStakeCombination otherStakeCombination)
		{
			if (IsCostToCover && otherStakeCombination.TotalBet != StakeCombination.TotalBet)
				return false;

			if (otherStakeCombination.Values.Count != StakeCombination.Values.Count)
				return false;

			foreach (var kvp in otherStakeCombination.Values)
			{
				if (!IsCostToCover && kvp.Key == Stake.BetMultiplier)
					continue;

				if (!StakeCombination.Values.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
					return false;
			}

			return true;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is PlayButtonSpecificData other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (StakeCombination.GetHashCode() * 397) ^ IsSelected.GetHashCode();
			}
		}

		#endregion
	}
}