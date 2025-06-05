// -----------------------------------------------------------------------
// <copyright file = "MachineWideBetConstraints.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Text;

    /// <summary>
    /// Contains the machine wide bet constraints.
    /// </summary>
    public class MachineWideBetConstraints
    {
        #region Properties

        /// <summary>
        /// Gets the tokenized amount, in units of base units.
        /// A non-zero amount indicates that bets must be tokenized such that bet offerings must be evenly divisible by this amount.
        /// A zero amount means "not tokenized", such that tokenization logic need not be applied.
        /// </summary>
        public long TokenizedAmount { get; }

        /// <summary>
        /// Gets the max bet behavior to be applied to mid-game bets.
        /// </summary>
        public MidGameMaxBetBehavior MidGameMaxBetBehavior { get; }

        /// <summary>
        /// Gets the configuration limits related to the starting bet.
        /// </summary>
        public BetLimits StartingBetLimits { get; }

        /// <summary>
        /// Gets the configuration limits related to mid-game bets.
        /// </summary>
        public BetLimits MidGameBetLimits { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MachineWideBetConstraints"/>.
        /// </summary>
        /// <param name="tokenizedAmount">
        /// The tokenized amount, in units of base units.
        /// </param>
        /// <param name="midGameMaxBetBehavior">
        /// The max bet behavior to be applied to mid-game bets.
        /// </param>
        /// <param name="startingBetLimits">
        /// The configuration limits related to the starting bet.
        /// </param>
        /// <param name="midGameBetLimits">
        /// The configuration limits related to mid-game bets.
        /// </param>
        public MachineWideBetConstraints(long tokenizedAmount,
                                         MidGameMaxBetBehavior midGameMaxBetBehavior,
                                         BetLimits startingBetLimits,
                                         BetLimits midGameBetLimits)
        {
            TokenizedAmount = tokenizedAmount;
            MidGameMaxBetBehavior = midGameMaxBetBehavior;
            StartingBetLimits = startingBetLimits;
            MidGameBetLimits = midGameBetLimits;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("MachineWideBetConstraints -");
            builder.AppendLine("\t TokenizedAmount: " + TokenizedAmount);
            builder.AppendLine("\t MidGameMaxBetBehavior: " + MidGameMaxBetBehavior);
            builder.AppendLine("\t Starting" + StartingBetLimits);
            builder.AppendLine("\t MidGame" + MidGameBetLimits);

            return builder.ToString();
        }

        #endregion
    }
}
