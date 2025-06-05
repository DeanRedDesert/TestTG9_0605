// -----------------------------------------------------------------------
// <copyright file = "ActionResponseLiteEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a client initiated lightweight transaction becomes available.
    /// </summary>
    [Serializable]
    public sealed class ActionResponseLiteEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the name of the transaction represented by this event.
        /// Null if none is specified.
        /// </summary>
        /// <remarks>
        /// This is the name specified in the ActionRequestLite call to which
        /// this event is responding.
        /// </remarks>
        public string TransactionName { get; private set; }

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="T:IGT.Ascent.Communication.Platform.Interfaces.ActionResponseLiteEventArgs" />.
        /// </summary>
        /// <param name="transactionName">
        /// The name of the lightweight transaction represented by this event.
        /// </param>
        public ActionResponseLiteEventArgs(string transactionName) : base(false)
        {
            TransactionName = transactionName;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ActionResponseLiteEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t TransactionName: " + TransactionName);

            return builder.ToString();
        }

        #endregion
    }
}