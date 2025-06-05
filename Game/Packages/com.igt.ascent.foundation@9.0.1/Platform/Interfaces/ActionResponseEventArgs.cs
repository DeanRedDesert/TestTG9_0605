// -----------------------------------------------------------------------
// <copyright file = "ActionResponseEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a client initiated transaction becomes available.
    /// </summary>
    [Serializable]
    public sealed class ActionResponseEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the name of the transaction represented by this event.
        /// Null if none is specified.
        /// </summary>
        /// <remarks>
        /// This is the name specified in the ActionRequest call to which
        /// this event is responding.
        /// </remarks>
        public string TransactionName { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ActionResponseEventArgs"/>.
        /// </summary>
        /// <param name="transactionName">
        /// The name of the transaction represented by this event.
        /// </param>
        public ActionResponseEventArgs(string transactionName)
        {
            TransactionName = transactionName;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ActionResponseEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t TransactionName: " + TransactionName);

            return builder.ToString();
        }

        #endregion
    }
}