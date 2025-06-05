//-----------------------------------------------------------------------
// <copyright file = "ShellHistoryUpdateEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Interfaces;

    /// <summary>
    /// Event arguments containing updated shell history data, sent from the history coplayer.
    /// </summary>
    internal class ShellHistoryUpdateEventArgs : CustomCoplayerEventArgs
    {
        /// <summary>
        /// Gets the cotheme presentation key of the coplayer in history display.
        /// </summary>
        public CothemePresentationKey CoplayerPresentationKey { get; }

        /// <summary>
        /// Gets the history record of the shell.
        /// </summary>
        public HistoryRecord ShellRecord { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellHistoryUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="coplayerPresentationKey">The cotheme presentation key of the coplayer in history display</param>
        /// <param name="shellRecord">The history record of the shell.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coplayerPresentationKey"/> is null.
        /// </exception>
        public ShellHistoryUpdateEventArgs(CothemePresentationKey coplayerPresentationKey, HistoryRecord shellRecord)
            : base(coplayerPresentationKey != null ? coplayerPresentationKey.CoplayerId : 0)
        {
            CoplayerPresentationKey = coplayerPresentationKey ?? throw new ArgumentNullException(nameof(coplayerPresentationKey));
            ShellRecord = shellRecord;
        }
    }
}
