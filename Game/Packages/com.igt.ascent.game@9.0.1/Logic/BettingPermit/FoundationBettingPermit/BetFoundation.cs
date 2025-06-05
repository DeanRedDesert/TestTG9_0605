//-----------------------------------------------------------------------
// <copyright file = "BetFoundation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.FoundationBettingPermit
{
    using System;
    using Communication.Platform.GameLib.Interfaces;
    using Game.Core.Communication.Foundation;

    /// <summary>
    /// Methods to validate against the foundation.
    /// </summary>
    public static class BetFoundation
    {
        /// <summary>
        /// Get the wagerable credits from the player meters, following the "Banked Credit"
        /// environment variable - "Wagerable" if banked, "Bank" otherwise. In game credits.
        /// </summary>
        /// <param name="gameLib">Instance of <see cref="IGameLib"/> to access the foundation through.</param>
        /// <returns>The amount of credits that may be wagered.</returns>
        public static long GetWagerableCredits(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException(nameof(gameLib));
            }
            var meters = gameLib.GetPlayerMeters();
            var banked = gameLib.IsEnvironmentTrue(EnvironmentAttribute.BankedCredits);
            var wagerableBaseCredits = banked ? meters.Wagerable : meters.Bank;
            return Utility.ConvertToCredits(wagerableBaseCredits, gameLib.GameDenomination);
        }

        /// <summary>
        /// Get the amount of credits already committed in the current game cycle.
        /// </summary>
        /// <param name="gameLib">Instance of <see cref="IGameLib"/> to access the foundation through.</param>
        /// <returns>The amount of credits already committed.</returns>
        public static long GetCommittedCredits(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException(nameof(gameLib));
            }

            gameLib.GetCommittedBet(out var bet, out var denom);
            long credits;
            if(denom != gameLib.GameDenomination)
            {
                checked
                {
                    credits = bet * denom / gameLib.GameDenomination;
                }
            }
            else
            {
                credits = bet;
            }
            return credits;
        }
    }
}