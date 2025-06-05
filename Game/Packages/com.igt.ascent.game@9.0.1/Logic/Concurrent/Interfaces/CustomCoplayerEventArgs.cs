// -----------------------------------------------------------------------
// <copyright file = "CustomCoplayerEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Text;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// A custom event sent by a coplayer.
    /// </summary>
    /// <inheritdoc/>
    [Serializable]
    public class CustomCoplayerEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the ID of the coplayer who sends the event.
        /// </summary>
        public int CoplayerId { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CustomCoplayerEventArgs"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The ID of the coplayer who sends the event.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        public CustomCoplayerEventArgs(int coplayerId)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException("coplayerId", "Coplayer ID cannot be negative.");
            }

            CoplayerId = coplayerId;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("CustomCoplayerEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t CoplayerId: " + CoplayerId);

            return builder.ToString();
        }

        #endregion

    }
}