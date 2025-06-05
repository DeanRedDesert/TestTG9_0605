//-----------------------------------------------------------------------
// <copyright file = "ProgressiveBroadcastEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Event to broadcast the progressive amounts and prize strings.
    /// </summary>
    [Serializable]
    public class ProgressiveBroadcastEventArgs : EventArgs
    {
        /// <summary>
        /// List of progressive broadcast data to be displayed to the player,
        /// keyed by game levels.
        /// </summary>
        public IDictionary<int, ProgressiveBroadcastData> BroadcastDataList { get; private set; }

        /// <summary>
        /// Constructs a ProgressiveBroadcastEventArgs with given list of
        /// progressive broadcast data keyed by game levels.
        /// </summary>
        /// <param name="broadcastDataList">
        /// List of progressive broadcast data keyed by game levels.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="broadcastDataList"/> is null.
        /// </exception>
        public ProgressiveBroadcastEventArgs(IDictionary<int, ProgressiveBroadcastData> broadcastDataList)
        {
            BroadcastDataList = broadcastDataList ?? throw new ArgumentNullException(nameof(broadcastDataList));
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ProgressiveBroadcastEvent -");

            foreach (var entry in BroadcastDataList)
            {
                builder.AppendFormat("\t Game Level {0}: {1}{2}",
                                     entry.Key, entry.Value, Environment.NewLine);
            }

            return builder.ToString();
        }
    }
}
