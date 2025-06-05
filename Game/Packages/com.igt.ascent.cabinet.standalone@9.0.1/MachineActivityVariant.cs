//-----------------------------------------------------------------------
// <copyright file = "MachineActivityVariant.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;

    /// <summary>
    /// Struct that represents the a Machine Activity Setting
    /// maintained by the Foundation.
    /// </summary>
    [Serializable]
    internal struct MachineActivityVariant
    {
        /// <summary>
        /// The interval between attracts.
        /// </summary>
        public uint AttractInterval { private set; get; }

        /// <summary>
        /// The delay before starting an attract.
        /// </summary>
        public uint InactivityDelay { private set; get; }

        /// <summary>
        /// Flag indicating if attracts are enabled.
        /// </summary>
        public bool AttractsEnabled { private set; get; }

        /// <summary>
        /// Flag which indicates if the current game is a new game.
        /// </summary>
        public bool NewGame { private set; get; }

        /// <summary>
        /// Constructor taking parameters for all the fields.
        /// </summary>
        /// <param name="attractInterval">The interval between attracts.</param>
        /// <param name="inactivityDelay">The delay before starting an attract.</param>
        /// <param name="attractsEnabled">Flag indicating if attracts are enabled.</param>
        /// <param name="newGame">Flag which indicates if the current game is a new game.</param>
        public MachineActivityVariant(uint attractInterval, uint inactivityDelay, bool attractsEnabled, bool newGame)
            :this()
        {
            AttractInterval = attractInterval;
            AttractsEnabled = attractsEnabled;
            InactivityDelay = inactivityDelay;
            NewGame = newGame;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $@"Attract Interval:{AttractInterval}s / AttractsEnabled:{AttractsEnabled} / InactivityDelay:{InactivityDelay}s / NewGame:{NewGame}";
        }
    }
}
