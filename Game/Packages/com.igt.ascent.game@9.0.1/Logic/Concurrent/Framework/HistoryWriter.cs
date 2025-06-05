// -----------------------------------------------------------------------
// <copyright file = "HistoryWriter.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// A class that writes history records for the coplayer and shell to critical data.
    /// </summary>
    internal class HistoryWriter
    {
        public static readonly CriticalDataName StepCountName = "Steps";
        private readonly ICriticalDataStore criticalDataStore;

        /// <summary>
        /// Initializes a new history writer.
        /// </summary>
        /// <param name="criticalDataStore">The <see cref="ICriticalDataStore"/> to write to.</param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal HistoryWriter(ICriticalDataStore criticalDataStore)
        {
            this.criticalDataStore = criticalDataStore;
        }

        /// <summary>
        /// Writes the given records to critical data.
        /// </summary>
        /// <param name="coplayerRecord">The coplayer's <see cref="HistoryRecord"/>.</param>
        /// <param name="shellRecord">The shell's <see cref="HistoryRecord"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the step numbers do not match for the provided records.
        /// </exception>
        internal void WriteHistoryStep(HistoryRecord coplayerRecord, HistoryRecord shellRecord)
        {
            if(coplayerRecord.StepNumber != shellRecord.StepNumber)
            {
                throw new ArgumentException("Coplayer and shell record step numbers must match.");
            }
            var historyBlock = new CriticalDataBlock();
            var coplayerKey = new CoplayerHistoryStepKey(coplayerRecord.StepNumber).CriticalDataName;
            var shellKey = new ShellHistoryStepKey(shellRecord.StepNumber).CriticalDataName;
            historyBlock.SetCriticalData(coplayerKey, coplayerRecord);
            historyBlock.SetCriticalData(shellKey, shellRecord);
            historyBlock.SetCriticalData(StepCountName, coplayerRecord.StepNumber);
            criticalDataStore.Write(historyBlock);
        }
    }
}
