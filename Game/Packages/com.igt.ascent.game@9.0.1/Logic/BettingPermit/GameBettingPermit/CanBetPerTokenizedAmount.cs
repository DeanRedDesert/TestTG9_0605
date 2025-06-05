// -----------------------------------------------------------------------
// <copyright file = "CanBetPerTokenizedAmount.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that checks bet amounts against the tokenized amount.
    /// </summary>
    public sealed class CanBetPerTokenizedAmount : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// The tokenized amount set by the Foundation.
        /// If not zero, the bet amount must be evenly divisible by this amount.
        /// </summary>
        private readonly long tokenizedAmount;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetPerTokenizedAmount"/>.
        /// </summary>
        /// <param name="tokenizedAmount">
        /// The tokenized amount set by the Foundation.
        /// If not zero, the bet amount must be evenly divisible by this amount.
        /// </param>
        public CanBetPerTokenizedAmount(long tokenizedAmount)
        {
            this.tokenizedAmount = tokenizedAmount;
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            return tokenizedAmount == 0 || data.BaseValue % tokenizedAmount == 0;
        }

        #endregion
    }
}