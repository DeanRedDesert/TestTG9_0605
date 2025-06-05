// -----------------------------------------------------------------------
// <copyright file = "CustomShellEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System.Text;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// A custom event sent by the shell.
    /// </summary>
    /// <inheritdoc/>
    public class CustomShellEventArgs : NonTransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("CustomShellEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}