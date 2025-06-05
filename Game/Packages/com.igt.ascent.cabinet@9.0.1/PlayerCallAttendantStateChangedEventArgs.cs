// -----------------------------------------------------------------------
// <copyright file = "PlayerCallAttendantStateChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Occurs when the player call attendant state changes.
    /// </summary>
    public class PlayerCallAttendantStateChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating whether the state is on.
        /// </summary>
        public bool StateOn { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PlayerCallAttendantStateChangedEventArgs"/>.
        /// </summary>
        public PlayerCallAttendantStateChangedEventArgs(bool stateOn)
        {
            StateOn = stateOn;
        }

        #endregion
    }
}