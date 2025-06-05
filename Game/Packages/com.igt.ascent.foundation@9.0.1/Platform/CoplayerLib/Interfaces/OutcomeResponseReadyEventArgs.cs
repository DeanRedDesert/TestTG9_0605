// -----------------------------------------------------------------------
// <copyright file = "OutcomeResponseReadyEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying the game-client that the OutcomeList has been evaluated and (potentially) adjusted.
    /// </summary>
    [Serializable]
    public sealed class OutcomeResponseReadyEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Indicates if this was the last outcome or not, which determines if the foundation state was
        /// progressed to Playing or MainPlayComplete.
        /// </summary>
        public bool IsLastOutcome { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an instance of <see cref="OutcomeResponseReadyEventArgs"/>.
        /// </summary>
        /// <param name="isLastOutcome">The flag that indicates if this was the last outcome or not.</param>
        public OutcomeResponseReadyEventArgs(bool isLastOutcome)
        {
            IsLastOutcome = isLastOutcome;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeResponseReadyEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t IsLastOutcome: " + IsLastOutcome);

            return builder.ToString();
        }

        #endregion
    }
}