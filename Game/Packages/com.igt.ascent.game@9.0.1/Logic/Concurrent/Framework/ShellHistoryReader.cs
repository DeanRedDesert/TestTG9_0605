// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryReader.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.Collections.Generic;
    using Communication.Platform.ShellLib.Interfaces;

    /// <summary>
    /// A class that reads history data from the shell history data store.
    /// </summary>
    internal sealed class ShellHistoryReader
    {
        #region Private Fields

        private readonly IShellHistoryControl shellHistoryControl;

        #endregion

        #region Properties

        public bool Initialized { get; private set; }

        public HistoryRecord LastStepRecord { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellHistoryReader"/>.
        /// </summary>
        /// <param name="shellHistoryControl">
        /// The object to read history critical data from.
        /// </param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal ShellHistoryReader(IShellHistoryControl shellHistoryControl)
        {
            this.shellHistoryControl = shellHistoryControl;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the last step shell history record from the shell history store.
        /// </summary>
        /// <remarks>
        /// This method needs a heavyweight transaction.
        /// </remarks>
        public void ReadLastStep()
        {
            var criticalDataBlock = shellHistoryControl.ReadCriticalData(new List<string> { ShellHistoryWriter.LastStepName });
            LastStepRecord = criticalDataBlock.GetCriticalData<HistoryRecord>(ShellHistoryWriter.LastStepName);
            Initialized = true;
        }

        #endregion
    }
}