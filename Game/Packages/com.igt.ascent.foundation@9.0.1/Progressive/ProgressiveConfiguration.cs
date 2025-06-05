//-----------------------------------------------------------------------
// <copyright file = "ProgressiveConfiguration.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;

    /// <summary>
    /// Struct that represents the settings of a progressive level.
    /// </summary>
    [Serializable]
    public struct ProgressiveConfiguration : IEquatable<ProgressiveConfiguration>
    {
        #region Contants

        /// <summary>
        /// The upper limit for progressive related amount.
        /// </summary>
        /// <devdoc>
        /// This is the maximum value defined in the default.config
        /// for AVP standalone progressive configurations.
        /// </devdoc>
        public const long AmountUpperLimit = 100000000000;

        /// <summary>
        /// Comparison tolerance for contribution percentages.
        /// </summary>
        private const float ContributionTolerance = 0.00001f;

        #endregion

        #region Properties

        /// <summary>
        /// The 0-based id of the progressive level being configured.
        /// </summary>
        public int LevelId { get; }

        /// <summary>
        /// The starting amount for this progressive level when it is reset,
        /// in base units.
        /// </summary>
        public long StartingAmount { get; }

        /// <summary>
        /// The inclusive maximum amount for the configured progressive level,
        /// in base units.
        /// </summary>
        public long MaximumAmount { get; }

        /// <summary>
        /// The percentage of the bet amount that is contributed to
        /// the progressive amount per game cycle in the range 0 - 1.
        /// </summary>
        public float ContributionPercentage { get; }

        /// <summary>
        /// The description of a prize to be awarded that is not part of
        /// of the progressive amount.
        /// </summary>
        public string PrizeString { get; }

        /// <summary>
        /// The flag indicating whether the progressive level is event-based or not.
        /// When a level is configured to be "event-based", only contributions specifically declared by the game-client
        /// are accumulated by the controller; Contributions are not accumulated based on a percentage of the bets/wagers.
        /// </summary>
        public bool IsEventBased { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor taking arguments for all the field.
        /// </summary>
        /// <param name="levelId">
        /// The 0-based id of the progressive level being configured.
        /// </param>
        /// <param name="startingAmount">
        /// The starting amount for this progressive level when it is reset.
        /// </param>
        /// <param name="maximumAmount">
        /// The inclusive maximum amount for the configured progressive level.
        /// </param>
        /// <param name="contributionPercentage">
        /// The percentage of the bet amount that is contributed to
        /// the progressive amount per game cycle, in the range 0 - 1,
        /// where 1 means 100%.
        /// </param>
        /// <param name="prizeString">
        /// The description of a prize to be awarded that is not part of the progressive amount.
        /// </param>
        /// <param name="isEventBased">
        /// The flag indicating whether the progressive level is event-based.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of the argument is out of the acceptable range,
        /// or the specified starting amount is greater than the maximum amount.
        /// </exception>
        /// <DevDoc>
        /// The sanity check here might become unnecessary
        /// if there are ways to guarantee the data validity
        /// in the configuration setup.
        /// </DevDoc>
        public ProgressiveConfiguration(int levelId,
                                        long startingAmount,
                                        long maximumAmount,
                                        float contributionPercentage,
                                        string prizeString = null,
                                        bool isEventBased = false)
            : this()
        {
            if(levelId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelId), "Level Id cannot be negative.");
            }

            if(startingAmount < 0 || startingAmount > AmountUpperLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(startingAmount),
                                                      "Amount must be in the range of 0 - " + AmountUpperLimit);
            }

            if(maximumAmount < 0 || maximumAmount > AmountUpperLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumAmount),
                                                      "Amount must be in the range of 0 - " + AmountUpperLimit);
            }

            if(contributionPercentage < 0 || contributionPercentage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(contributionPercentage),
                                                      "Contribution Percentage must be in the range of 0 - 1");
            }

            if(startingAmount > maximumAmount)
            {
                throw new ArgumentOutOfRangeException(nameof(startingAmount),
                                                      "Starting amount must be less than or equal to the maximum amounts");
            }

            LevelId = levelId;
            StartingAmount = startingAmount;
            MaximumAmount = maximumAmount;
            ContributionPercentage = contributionPercentage;
            PrizeString = prizeString ?? string.Empty;
            IsEventBased = isEventBased;
        }

        #endregion

        #region Equality members

        // Generated by ReSharper

        /// <inheritdoc />
        public bool Equals(ProgressiveConfiguration other)
        {
            return LevelId == other.LevelId &&
                   StartingAmount == other.StartingAmount &&
                   MaximumAmount == other.MaximumAmount &&
                   Math.Abs(ContributionPercentage - other.ContributionPercentage) < ContributionTolerance &&
                   PrizeString == other.PrizeString &&
                   IsEventBased == other.IsEventBased;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ProgressiveConfiguration other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LevelId;
                hashCode = (hashCode * 397) ^ StartingAmount.GetHashCode();
                hashCode = (hashCode * 397) ^ MaximumAmount.GetHashCode();
                hashCode = (hashCode * 397) ^ ContributionPercentage.GetHashCode();
                hashCode = (hashCode * 397) ^ PrizeString.GetHashCode();
                hashCode = (hashCode * 397) ^ IsEventBased.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Overloads the operator ==.
        /// </summary>
        public static bool operator ==(ProgressiveConfiguration left, ProgressiveConfiguration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Overloads the operator !=.
        /// </summary>
        public static bool operator !=(ProgressiveConfiguration left, ProgressiveConfiguration right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region ToString Overrides

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Level({LevelId})/Starting({StartingAmount})/Maximum({MaximumAmount})/Contribution%({ContributionPercentage:P2})/Prize({PrizeString})/IsEventBased({IsEventBased})";
        }

        #endregion
    }
}
