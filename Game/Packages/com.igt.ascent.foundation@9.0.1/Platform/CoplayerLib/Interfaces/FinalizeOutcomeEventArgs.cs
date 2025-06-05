// -----------------------------------------------------------------------
// <copyright file = "FinalizeOutcomeEventArgs.cs" company = "IGT">
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
    /// Event notifying the client that the Foundation has finalized (metered, posted, and logged) the outcome.
    /// Transitions the GameCycleState from FinalizePending to Finalized.
    /// </summary>
    [Serializable]
    public sealed class FinalizeOutcomeEventArgs : NonTransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("FinalizeOutcomeEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}