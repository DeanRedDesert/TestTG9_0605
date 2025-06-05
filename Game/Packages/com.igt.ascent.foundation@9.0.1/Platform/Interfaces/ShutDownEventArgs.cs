// -----------------------------------------------------------------------
// <copyright file = "ShutDownEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that the application is being shut down by Foundation.
    /// </summary>
    [Serializable]
    public sealed class ShutDownEventArgs : NonTransactionalEventArgs
    {
        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ShutDownEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion

    }
}