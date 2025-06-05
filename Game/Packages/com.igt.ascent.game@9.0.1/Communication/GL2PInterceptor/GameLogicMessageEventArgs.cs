//-----------------------------------------------------------------------
// <copyright file = "GameLogicMessageEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Logic.CommServices;

    /// <summary>
    /// Event argument that contains a reference to a GameLogicGenericMsg object.
    /// </summary>
    public class GameLogicMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for GameLogicMessageEventArgs.
        /// </summary>
        /// <param name="message">GameLogicGenericMsg object.</param>
        /// <exception cref="ArgumentNullException">Thrown message parameter is null.</exception>
        public GameLogicMessageEventArgs(GameLogicGenericMsg message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "Message argument cannot be null.");
            }

            Message = message;
        }

        /// <summary>
        /// Gets GameLogicGenericMsg object.
        /// </summary>
        public GameLogicGenericMsg Message { get; private set; }
    }
}
