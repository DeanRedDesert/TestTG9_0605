// -----------------------------------------------------------------------
// <copyright file = "BaseCoplayerEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// Base class for all internal events sent from a coplayer to the shell.
    /// </summary>
    /// <inheritdoc/>
    internal abstract class BaseCoplayerEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the ID of the coplayer who sends the event.
        /// </summary>
        public int CoplayerId { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BaseCoplayerEventArgs"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The ID of the coplayer who sends the event.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        protected BaseCoplayerEventArgs(int coplayerId)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID cannot be negative.");
            }

            CoplayerId = coplayerId;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Event sent by Coplayer {CoplayerId}";
        }

        #endregion
    }
}