//-----------------------------------------------------------------------
// <copyright file = "BetConfiguration.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.FoundationBettingPermit
{
    using System;
    using Communication.Platform.GameLib.Interfaces;
    using Game.Core.Logic.BetFramework;

    /// <summary>
    /// Contains the configuration information required for the bet object.
    /// </summary>
    public class BetConfiguration : BetConfigurationBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BetConfiguration"/>.
        /// </summary>
        /// <param name="gameLib">
        /// Game library object to access wager configuration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLib"/> is null.
        /// </exception>
        public BetConfiguration(IGameLib gameLib)
        {
            GameLib = gameLib ?? throw new ArgumentNullException(nameof(gameLib));

            Initialize(gameLib.GameMaxBet, gameLib.GameMinBet, gameLib.ButtonPanelMinBet);
        }

        #region Protected fields

        /// <summary>
        /// Game library object to access wager configuration.
        /// </summary>
        protected IGameLib GameLib { get; }

        #endregion
    }
}
