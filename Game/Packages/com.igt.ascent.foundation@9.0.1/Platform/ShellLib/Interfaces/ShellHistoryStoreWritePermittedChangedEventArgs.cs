// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryStoreWritePermittedChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for when the write permitted status of a coplayer's corresponding shell history store has changed.
    /// </summary>
    /// <remarks>
    /// A coplayer's corresponding shell history store is the shell's history store that is
    /// linked to the coplayer's current game cycle.
    /// </remarks>
    [Serializable]
    public sealed class ShellHistoryStoreWritePermittedChangedEventArgs : TransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the Coplayer ID whose corresponding shell history store's write permitted status has changed.
        /// </summary>
        public int CoplayerId { get; private set; }

        /// <summary>
        /// Gets the new write permitted status for the specified coplayer's corresponding shell history store.
        /// </summary>
        public bool WritePermitted { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellHistoryStoreWritePermittedChangedEventArgs"/>.
        /// </summary>
        /// <param name="coplayerId">
        /// The Coplayer ID whose corresponding shell history store's write permitted status has changed.
        /// </param>
        /// <param name="writePermitted">
        /// The new write permitted status for the specified coplayer's corresponding shell history store.
        /// </param>
        public ShellHistoryStoreWritePermittedChangedEventArgs(int coplayerId, bool writePermitted)
        {
            CoplayerId = coplayerId;
            WritePermitted = writePermitted;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ShellHistoryStoreWritePermittedChangedEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t CoplayerId: " + CoplayerId);
            builder.AppendLine("\t WritePermitted: " + WritePermitted);

            return builder.ToString();
        }

        #endregion
    }
}