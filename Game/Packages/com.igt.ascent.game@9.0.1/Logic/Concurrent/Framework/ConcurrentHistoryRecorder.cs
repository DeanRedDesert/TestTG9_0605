// -----------------------------------------------------------------------
// <copyright file = "ConcurrentHistoryRecorder.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Threading;
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// The <see cref="ConcurrentHistoryRecorder"/> class records history data and creates history records from the 
    /// recorded data. It allows updates (writes) from a single thread and record creation (reads) from multiple
    /// threads simultaneously. When creating records clients can either obtain a snapshot of the accumulated data
    /// or they can provide base data to create a diff from. If they opt to create differences then clients are
    /// responsible for maintaining the base data. For example, the client should merge the differenced data
    /// from the created record into its base data to ensure that the next difference will only include data seen
    /// since the current record was created.
    /// </summary>
    internal class ConcurrentHistoryRecorder : IDisposable
    {
        private readonly DataItems accumulatedData = new DataItems();
        private readonly ReaderWriterLockSlim dataLock = new ReaderWriterLockSlim();

        // Note that we don't have to worry about power hit persistence of this state name,
        // as the recorder does not provide "diff" but snapshot of the accumulated data.
        // Also in state machine, Committed stage is not power hit tolerant, hence a call to
        // StartPresentationState is expected even after a power hit recovery.
        private string markedHistoryState;

        /// <summary>
        /// Creates a history record which contains either all accumulated data or only data accumulated which
        /// differs from the provided base data.
        /// </summary>
        /// <param name="stepNumber">
        /// The history step number for the record.
        /// </param>
        /// <param name="baseData">
        /// An (optional) base data collection to use in creating a snapshot containing the difference between the
        /// accumulated data and the base data.
        /// </param>
        /// <returns>
        /// The history record created.
        /// </returns>
        internal HistoryRecord CreateHistoryRecord(int stepNumber, DataItems baseData = null)
        {
            if(stepNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepNumber), "stepNumber must be >= 1.");
            }

            dataLock.EnterReadLock();
            try
            {
                if(markedHistoryState == null)
                {
                    throw new FrameworkRunnerException("No Shell presentation state data has been recorded by far." +
                                                       "Please make sure that at least one Shell presentation state has been started.");
                }

                DataItems snapshot;
                if(baseData != null)
                {
                    snapshot = baseData.DiffWith(accumulatedData);
                }
                else
                {
                    snapshot = new DataItems();
                    snapshot.Merge(accumulatedData);
                }
                return new HistoryRecord(stepNumber, markedHistoryState, snapshot);
            }
            finally
            {
                dataLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates the accumulated data with the provided data.
        /// </summary>
        /// <param name="data">
        /// The data to merge into the accumulated data.
        /// </param>
        /// <param name="stateName">
        /// The state which <paramref name="data"/> is for.  Null if it is for the last marked state.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is null.
        /// </exception>
        internal void Update(DataItems data, string stateName = null)
        {
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            dataLock.EnterWriteLock();
            try
            {
                accumulatedData.Merge(data);

                if(stateName != null)
                {
                    markedHistoryState = stateName;
                }
            }
            finally
            {
                dataLock.ExitWriteLock();
            }
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        /// Disposes managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// A flag which should be true if <see cref="IDisposable.Dispose"/> is called and false otherwise.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    dataLock.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
