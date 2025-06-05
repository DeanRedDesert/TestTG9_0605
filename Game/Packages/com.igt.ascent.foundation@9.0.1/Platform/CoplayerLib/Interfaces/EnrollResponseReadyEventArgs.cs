// -----------------------------------------------------------------------
// <copyright file = "EnrollResponseReadyEventArgs.cs" company = "IGT">
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
    /// Event notifying the client that enrollment results are available.
    /// </summary>
    [Serializable]
    public sealed class EnrollResponseReadyEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Indicates host enrollment success/failure.
        /// </summary>
        public bool Success { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an instance of <see cref="EnrollResponseReadyEventArgs"/>.
        /// </summary>
        /// <param name="success">The flag that indicates the success/failure of host enrollment.</param>
        public EnrollResponseReadyEventArgs(bool success)
        {
            Success = success;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("EnrollResponseReadyEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t Success: " + Success);

            return builder.ToString();
        }

        #endregion
    }
}