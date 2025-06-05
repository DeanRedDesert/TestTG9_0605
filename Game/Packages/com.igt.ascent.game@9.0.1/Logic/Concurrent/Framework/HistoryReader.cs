// -----------------------------------------------------------------------
// <copyright file = "HistoryReader.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class enumerates over the history steps contained in a critical data store.
    /// </summary>
    internal class HistoryReader : IEnumerator<HistoryReader.HistoryStep>
    {
        #region HistoryStep type

        /// <summary>
        /// Contains a single step of history for a coplayer and the shell.
        /// </summary>
        internal readonly struct HistoryStep
        {
            /// <summary>
            /// Gets the coplayer record.
            /// </summary>
            internal HistoryRecord CoplayerRecord { get; }

            /// <summary>
            /// Gets the shell record.
            /// </summary>
            internal HistoryRecord ShellRecord { get; }

            /// <summary>
            /// Initializes a new history step.
            /// </summary>
            /// <param name="coplayerRecord">The coplayer's history record.</param>
            /// <param name="shellRecord">The shell's history record.</param>
            internal HistoryStep(HistoryRecord coplayerRecord, HistoryRecord shellRecord) : this()
            {
                CoplayerRecord = coplayerRecord;
                ShellRecord = shellRecord;
            }
        }

        #endregion

        private readonly ICriticalDataStore criticalDataStore;
        private int currentStepNumber;

        /// <summary>
        /// Gets the total number of history steps that this reader can read.
        /// </summary>
        internal int TotalHistorySteps { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryReader"/> class.
        /// </summary>
        /// <param name="criticalDataStore">The critical data store to read the history data from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="criticalDataStore"/> is null.
        /// </exception>
        internal HistoryReader(ICriticalDataStore criticalDataStore)
        {
            this.criticalDataStore = criticalDataStore ?? throw new ArgumentNullException(nameof(criticalDataStore));
        }

        /// <summary>
        /// Reads the configuration for the history data from the critical data store.
        /// </summary>
        /// <remarks>
        /// Before this method is called, <see cref="TotalHistorySteps"/> will be 0, <see cref="MoveNext"/> will
        /// return false, and <see cref="Current"/> will contain an empty history record.
        /// </remarks>
        internal void ReadConfiguration()
        {
            Reset();
            var block = criticalDataStore.Read(HistoryWriter.StepCountName);
            TotalHistorySteps = block.GetCriticalData<int>(HistoryWriter.StepCountName);
        }

        #region IEnumerator implementation

        /// <inheritdoc/>
        public HistoryStep Current { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if(currentStepNumber == TotalHistorySteps)
            {
                return false;
            }
            currentStepNumber++;
            var coplayerKey = new CoplayerHistoryStepKey(currentStepNumber).CriticalDataName;
            var shellKey = new ShellHistoryStepKey(currentStepNumber).CriticalDataName;
            var block = criticalDataStore.Read(new [] { coplayerKey, shellKey });
            Current = new HistoryStep(block.GetCriticalData<HistoryRecord>(coplayerKey),
                                      block.GetCriticalData<HistoryRecord>(shellKey));
            return true;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            currentStepNumber = 0;
            Current = new HistoryStep();
        }

        /// <inheritdoc/>
        object IEnumerator.Current => Current;

        #endregion
    }
}
