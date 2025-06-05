// -----------------------------------------------------------------------
// <copyright file = "PlayerServiceRequestStateChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Occurs when the player request service state changes.
    /// </summary>
    public class PlayerServiceRequestStateChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating whether the state is on.
        /// </summary>
        public bool StateOn { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PlayerServiceRequestStateChangedEventArgs"/>.
        /// </summary>
        public PlayerServiceRequestStateChangedEventArgs(bool stateOn)
        {
            StateOn = stateOn;
        }

        #endregion
    }
}