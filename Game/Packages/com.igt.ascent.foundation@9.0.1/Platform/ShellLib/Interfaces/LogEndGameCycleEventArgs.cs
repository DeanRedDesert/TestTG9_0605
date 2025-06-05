// -----------------------------------------------------------------------
// <copyright file = "LogEndGameCycleEventArgs.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for when the Foundation logs the end of a game cycle.
    /// </summary>
    [Serializable]
    public sealed class LogEndGameCycleEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the coplayer id where the game cycle has been played.
        /// </summary>
        public int CoplayerId { get; private set; }

        /// <summary>
        /// Gets the number of history steps that are recorded in history for the current game cycle.
        /// </summary>
        public int NumberOfSteps { get; private set; }

        /// <summary>
        /// Gets the gaming meters at the end of the game cycle.
        /// </summary>
        public GamingMeters GamingMeters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEndGameCycleEventArgs"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The current coplayer to log the end game cycle.
        /// </param>
        /// <param name="numberOfSteps">
        /// The number of history steps that are recorded in history for the current game cycle.
        /// </param>
        /// <param name="gamingMeters">
        /// Current gaming meters at the end of game cycle on this coplayer.
        /// </param>
        public LogEndGameCycleEventArgs(int coplayerId, int numberOfSteps, GamingMeters gamingMeters)
        {
            CoplayerId = coplayerId;
            NumberOfSteps = numberOfSteps;
            GamingMeters = gamingMeters;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("LogEndGameCycleEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t Coplayer ID: " + CoplayerId);
            builder.AppendLine("\t Number of Steps: " + NumberOfSteps);
            builder.AppendLine("\t " + GamingMeters);

            return builder.ToString();
        }

        #endregion
    }
}