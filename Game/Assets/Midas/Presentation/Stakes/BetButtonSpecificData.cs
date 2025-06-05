using System;

namespace Midas.Presentation.Stakes
{
	public sealed class BetButtonSpecificData : IStakeButtonSpecificData, IEquatable<BetButtonSpecificData>
	{
		public long StakeMultiplier { get; }
		public bool HasAnteBet { get; }
		public bool IsSelected { get; }

		public BetButtonSpecificData(long stakeMultiplier, bool hasAnteBet, bool isSelected)
		{
			StakeMultiplier = stakeMultiplier;
			HasAnteBet = hasAnteBet;
			IsSelected = isSelected;
		}

		#region Equality Implementation

		public bool Equals(BetButtonSpecificData other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return StakeMultiplier == other.StakeMultiplier && HasAnteBet == other.HasAnteBet && IsSelected == other.IsSelected;
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is BetButtonSpecificData other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (StakeMultiplier.GetHashCode() * 397) ^ IsSelected.GetHashCode();
			}
		}

		#endregion
	}
}