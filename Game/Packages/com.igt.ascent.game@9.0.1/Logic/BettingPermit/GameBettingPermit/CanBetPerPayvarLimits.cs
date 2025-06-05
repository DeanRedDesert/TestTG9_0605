// -----------------------------------------------------------------------
// <copyright file = "CanBetPerPayvarLimits.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using System;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that checks bet amounts against payvar specific limits.
    /// </summary>
    public sealed class CanBetPerPayvarLimits : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// The config data related to game cycle betting.
        /// </summary>
        private readonly GameCycleBettingConfigData gameCycleBettingConfigData;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetPerPayvarLimits"/>.
        /// </summary>
        /// <param name="gameCycleBettingConfigData">
        /// The config data related to game cycle betting.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCycleBettingConfigData"/> is null.
        /// </exception>
        public CanBetPerPayvarLimits(GameCycleBettingConfigData gameCycleBettingConfigData)
        {
            this.gameCycleBettingConfigData = gameCycleBettingConfigData ?? throw new ArgumentNullException(nameof(gameCycleBettingConfigData));
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            return data.GameCreditValue <= gameCycleBettingConfigData.MaxBetCredits &&
                   data.GameCreditValue >= gameCycleBettingConfigData.MinBetCredits;
        }

        #endregion
    }
}