// -----------------------------------------------------------------------
// <copyright file = "ActivateShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a new shell context is being activated.
    /// </summary>
    [Serializable]
    public sealed class ActivateShellContextEventArgs : TransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ActivateShellContextEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}
