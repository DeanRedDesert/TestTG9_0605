// -----------------------------------------------------------------------
// <copyright file = "GamingMeters.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;

    /// <summary>
    /// Contains all the gaming meters displayable to the player that are maintained by the Foundation.
    /// </summary>
    [Serializable]
    public struct GamingMeters
    {
        #region Properties

        /// <summary>
        /// Gets the amount of money that is transferable to the player bettable meter,
        /// and is suitable for display to the player, in base units.
        /// </summary>
        public long Transferable { get; }

        /// <summary>
        /// Gets the amount of money available for player betting that is suitable for display to the player, in base units.
        /// </summary>
        public long Bettable { get; }

        /// <summary>
        /// Gets the amount paid to the player during the last/current cashout,
        /// and is suitable for display to the player, in base units.
        /// </summary>
        public long Paid { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GamingMeters"/>.
        /// </summary>
        /// <param name="transferable">
        /// The value of player transferable meter, in base units.
        /// </param>
        /// <param name="bettable">
        /// The value of player bettable meter, in base units.
        /// </param>
        /// <param name="paid">
        /// The value of paid meter, in base units.
        /// </param>
        public GamingMeters(long transferable, long bettable, long paid) : this()
        {
            Transferable = transferable;
            Bettable = bettable;
            Paid = paid;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"GamingMeters: Transferable({Transferable}) / Bettable({Bettable}) / Paid({Paid})";
        }

        #endregion
    }
}
