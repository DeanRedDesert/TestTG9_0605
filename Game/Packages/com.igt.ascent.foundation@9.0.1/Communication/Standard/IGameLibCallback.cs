//-----------------------------------------------------------------------
// <copyright file = "IGameLibCallback.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// GameLib interface which contains functionality that should
    /// only be used by communication components owned by Game Lib.
    /// </summary>
    public interface IGameLibCallback
    {
        /// <summary>
        /// Set the environment attributes passed down from the Foundation.
        /// </summary>
        /// <param name="environmentAttributes">
        /// List of the environment attributes from the Foundation
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="environmentAttributes"/> is null.
        /// </exception>
        void SetEnvironmentAttributes(ICollection<EnvironmentAttribute> environmentAttributes);

        /// <summary>
        /// Set the jurisdiction passed down from the Foundation.
        /// </summary>
        /// <param name="jurisdiction">
        /// The jurisdiction as a string.
        /// </param>
        void SetJurisdiction(string jurisdiction);

        /// <summary>
        /// Shut down the game process by raising a ShutDownEvent immediately
        /// instead of posting to the event queue.
        /// </summary>
        void ShutDownProcess();
    }
}
