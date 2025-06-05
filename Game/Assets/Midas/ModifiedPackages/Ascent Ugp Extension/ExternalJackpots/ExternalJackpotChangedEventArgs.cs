//-----------------------------------------------------------------------
// <copyright file = "ExternalJackpotChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for UGP external jackpot being changed.
    /// </summary>
    [Serializable]
    public class ExternalJackpotChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the information of the external jackpots.
        /// </summary>
        public ExternalJackpots ExternalJackpots { get; private set; }

        /// <summary>
        /// Instantiates a new <see cref="ExternalJackpotChangedEventArgs"/>.
        /// </summary>
        /// <param name="externalJackpots">
        /// The external jackpots received from the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="externalJackpots"/> is null.
        /// </exception>
        public ExternalJackpotChangedEventArgs(ExternalJackpots externalJackpots)
        {
            ExternalJackpots = externalJackpots ?? throw new ArgumentNullException(nameof(externalJackpots));
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("External Jackpot Changed Event -");
            builder.AppendLine("\t ExternalJackpots = " + ExternalJackpots);

            return builder.ToString();
        }
    }
}
