// -----------------------------------------------------------------------
// <copyright file = "MachineActivityStatus.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// This struct represents the machine activity status data.
    /// </summary>
    public struct MachineActivityStatus
    {
        /// <summary>
        /// Flag which indicates if machine is active.
        /// </summary>
        public readonly bool Active;

        /// <summary>
        /// The interval between attracts.
        /// </summary>
        public readonly uint AttractInterval;

        /// <summary>
        /// The delay before starting an attract.
        /// </summary>
        public readonly uint InactivityDelay;

        /// <summary>
        /// Flag indicating if attracts are enabled.
        /// </summary>
        public readonly bool AttractsEnabled;

        /// <summary>
        /// Flag which indicates if the current game is a new game.
        /// </summary>
        public readonly bool NewGame;

        /// <summary>
        /// Construct the instance with the machine activity status parameters.
        /// </summary>
        /// <param name="active">Flag which indicates if machine is active.</param>
        /// <param name="attractInterval">The interval between attracts.</param>
        /// <param name="inactivityDelay">The delay before starting an attract.</param>
        /// <param name="attractsEnabled">Flag indicating if attracts are enabled</param>
        /// <param name="newGame">Flag which indicates if the current game is a new game.</param>
        public MachineActivityStatus(bool active, uint attractInterval, uint inactivityDelay,
                                     bool attractsEnabled, bool newGame)
        {
            Active = active;
            AttractInterval = attractInterval;
            InactivityDelay = inactivityDelay;
            AttractsEnabled = attractsEnabled;
            NewGame = newGame;
        }

        #region Overrides of ValueType

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $@"Machine Active:{Active} / Attract Interval:{AttractInterval}s / AttractsEnabled:{AttractsEnabled} / InactivityDelay:{InactivityDelay}s / NewGame:{NewGame}";
        }

        #endregion
    }
}