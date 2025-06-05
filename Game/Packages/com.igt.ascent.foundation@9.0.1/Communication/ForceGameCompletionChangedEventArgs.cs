// -----------------------------------------------------------------------
// <copyright file = "ForceGameCompletionChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Defines the event arguments indicating that a
    /// force game completion changed event has occurred. 
    /// </summary>
    [Serializable]
    public class ForceGameCompletionChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the status of a force game-completion condition that has changed.
        /// </summary>
        public ForceGameCompletionStatus ForceGameCompletionStatus { get; private set; }

        /// <summary>
        /// Instantiates a new <see cref="ForceGameCompletionChangedEventArgs"/>.
        /// </summary>
        /// <param name="forceGameCompletionStatus">The force game-completion status that changed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="forceGameCompletionStatus"/> is null.
        /// </exception>
        public ForceGameCompletionChangedEventArgs(ForceGameCompletionStatus forceGameCompletionStatus)
        {
            ForceGameCompletionStatus = forceGameCompletionStatus ?? throw new ArgumentNullException(nameof(forceGameCompletionStatus));
        }
    }
}
