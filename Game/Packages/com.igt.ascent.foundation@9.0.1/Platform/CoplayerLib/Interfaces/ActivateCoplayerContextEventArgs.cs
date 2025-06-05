// -----------------------------------------------------------------------
// <copyright file = "ActivateCoplayerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a new coplayer context is being activated.
    /// </summary>
    public sealed class ActivateCoplayerContextEventArgs : TransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ActivateCoplayerContextEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}
