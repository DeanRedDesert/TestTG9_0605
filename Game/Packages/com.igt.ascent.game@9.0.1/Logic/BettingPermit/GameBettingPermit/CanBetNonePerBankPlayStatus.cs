// -----------------------------------------------------------------------
// <copyright file = "CanBetNonePerBankPlayStatus.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using System;
    using Concurrent.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that disallows all bet amounts per bank status.
    /// This operand does not care about the individual bet amount.
    /// </summary>
    public sealed class CanBetNonePerBankPlayStatus : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// The interface used to query current bank properties.
        /// </summary>
        private readonly IShellExposition shellExposition;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetNonePerBankPlayStatus"/>.
        /// </summary>
        /// <param name="shellExposition">
        /// The interface used to query current bank properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shellExposition"/> is null.
        /// </exception>
        public CanBetNonePerBankPlayStatus(IShellExposition shellExposition)
        {
            this.shellExposition = shellExposition ?? throw new ArgumentNullException(nameof(shellExposition));
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            // Note that per Foundation implementation, BankPlay.CanBet will be true only in Play mode.
            return !shellExposition.BankProperties.CanBet;
        }

        #endregion
    }
}