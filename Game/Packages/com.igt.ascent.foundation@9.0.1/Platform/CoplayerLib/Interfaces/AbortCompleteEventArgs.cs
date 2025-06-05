// -----------------------------------------------------------------------
// <copyright file = "AbortCompleteEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying the client that the Foundation has finished processing the abort request. 
    /// Transitions the GameCycleState from AbortPending to Finalized.
    /// </summary>
    public sealed class AbortCompleteEventArgs : NonTransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("AbortCompleteEventArgs -");
            builder.AppendLine(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}