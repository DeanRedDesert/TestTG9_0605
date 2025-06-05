// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryWriter.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;

    /// <summary>
    /// A class that writes history data to the shell history data store.
    /// </summary>
    internal sealed class ShellHistoryWriter
    {
        #region Constants

        /// <summary>
        /// The key used to write into the critical data the history record of the last step
        /// of a coplayer's game cycle.
        /// In history mode, <see cref="ShellHistoryReader"/> will use the same key to read data out.
        /// </summary>
        public static readonly CriticalDataName LastStepName = "ShellLastStep";

        #endregion

        #region Private Fields

        private readonly IShellHistoryStore shellHistoryStore;
        private readonly CriticalDataBlock criticalDataBlock;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellHistoryWriter"/>.
        /// </summary>
        /// <param name="shellHistoryStore">
        /// The shell history store to write to.
        /// </param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal ShellHistoryWriter(IShellHistoryStore shellHistoryStore)
        {
            this.shellHistoryStore = shellHistoryStore;
            criticalDataBlock = new CriticalDataBlock();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the last step shell history record to the shell history store.
        /// </summary>
        /// <param name="coplayerId">The coplayer that is currently in history display.</param>
        /// <param name="shellRecord">The shell history record to write.</param>
        /// <remarks>
        /// This method needs a heavyweight transaction.
        /// </remarks>
        public void WriteLastStep(int coplayerId, HistoryRecord shellRecord)
        {
            criticalDataBlock.SetCriticalData(LastStepName, shellRecord);
            shellHistoryStore.Write(coplayerId, criticalDataBlock);
        }

        #endregion
    }
}