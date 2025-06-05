// -----------------------------------------------------------------------
// <copyright file = "DenominationsWithProgressivesBroadcastEventArgs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Event to broadcast the progressive data for denominations that have been configured with progressive levels.
    /// </summary>
    [Serializable]
    public sealed class DenominationsWithProgressivesBroadcastEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Progressive broadcast data for a collection of denominations.
        /// Each denomination has a list of broadcast data; Each data is for a game level.
        /// </summary>
        public IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> BroadcastData { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DenominationsWithProgressivesBroadcastEventArgs"/>.
        /// </summary>
        /// <param name="broadcastData">
        /// Progressive broadcast data for a collection of denominations.
        /// Each denomination has a list of broadcast data; Each data is for a game level.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="broadcastData"/> is null.
        /// </exception>
        public DenominationsWithProgressivesBroadcastEventArgs(IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> broadcastData)
        {
            BroadcastData = broadcastData ?? throw new ArgumentNullException(nameof(broadcastData));
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("DenominationsWithProgressivesBroadcastEventArgs -");

            foreach(var denomEntry in BroadcastData)
            {
                builder.AppendLine($"\t Denomination = {denomEntry.Key}");

                var levelsBroadcastData = denomEntry.Value;
                foreach(var levelEntry in levelsBroadcastData)
                {
                    builder.AppendLine($"\t\t Game Level {levelEntry.Key}: {levelEntry.Value} base units");
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}