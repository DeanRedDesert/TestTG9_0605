// -----------------------------------------------------------------------
// <copyright file = "HistoryRecorder.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// A recorder for history data.
    /// </summary>
    /// <remarks>
    /// This class tracks history data which can be used to create a sequence of history records. Each record has a 
    /// step number assigned to it by the client, which is responsible for ensuring that the step numbers of the 
    /// saved records form a monotonically increasing sequence (i.e. the client must maintain and increment the step 
    /// number.)
    /// 
    /// In addition to creating steps, which happens by calling <see cref="CreateHistoryRecord(int, string, DataItems)"/>, this class
    /// can also stage updates to be recorded when the next call to <see cref="CreateHistoryRecord(int, string, DataItems)"/> is made.
    /// To stage an update, call the <see cref="StageUpdate(DataItems)"/> method with the data to stage. This method
    /// can be called repeatedly, and the staged updates will be accumulated. This can be done, for example, to
    /// track asynchronous provider updates as they are sent to the presentation. Once the <see cref="CreateHistoryRecord(int, string, DataItems)"/>
    /// method is called, all staged updates (including the data passed to <see cref="CreateHistoryRecord(int, string, DataItems)"/>)
    /// will be used to create a history record.
    /// 
    /// Each history record only includes the data that differs from the previously recorded step. To accomplish this, the
    /// history recorder maintains a cache of the accumulated data.
    /// 
    /// Calling the <see cref="Reset"/> method will clear any accumulated or staged data. Do this in order to start 
    /// recording a new game cycle.
    /// </remarks>
    /// <devdocs>
    /// This class is not thread-safe.
    /// </devdocs>
    internal class HistoryRecorder
    {
        private readonly ICriticalDataStore dataStore;
        private readonly SingleCriticalData<DataItems> baseDataBlock = new SingleCriticalData<DataItems>("HistoryRecorder.BaseData");
        private readonly SingleCriticalData<DataItems> stagedDataBlock = new SingleCriticalData<DataItems>("HistoryRecorder.StagedData");
        private readonly DataItems baseData = new DataItems();
        private readonly DataItems stagedData = new DataItems();

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryRecorder"/> class.
        /// </summary>
        /// <param name="dataStore">The critical data store to use for data persistence.</param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal HistoryRecorder(ICriticalDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        /// <summary>
        /// Creates a history record containing only the staged data that has changed since the last history record 
        /// was created.
        /// </summary>
        /// <param name="stepNumber">The history step number for the record.</param>
        /// <param name="stateName">The state that this step is being recorded during.</param>
        /// <param name="data">The data to record (in addition to any staged commits.) Can be null.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the step number is not a positive integer.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="stateName"/> is null or empty.
        /// </exception>
        /// <devdoc>
        /// Passing null for <paramref name="data"/> requires one fewer lock acquisition, and is thus faster in most
        /// cases.
        /// </devdoc>
        internal HistoryRecord CreateHistoryRecord(int stepNumber, string stateName, DataItems data = null)
        {
            if(stepNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepNumber), "stepNumber must be >= 1.");
            }
            if(string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(stateName));
            }
            if(data != null)
            {
                StageUpdate(data);
            }

            var diff = baseData.DiffWith(stagedData);
            var record = new HistoryRecord(stepNumber, stateName, diff);

            // take care of some bookkeeping
            stagedData.Clear();
            baseData.Merge(diff);

            baseDataBlock.Data = baseData;
            dataStore.Write(baseDataBlock);

            return record;
        }

        /// <summary>
        /// Reads any persisted configuration from critical data.
        /// </summary>
        internal void ReadConfiguration()
        {
            var keys = new[] { baseDataBlock.Name, stagedDataBlock.Name };
            var dataBlock = dataStore.Read(keys);

            if(dataBlock.Contains(baseDataBlock.Name))
            {
                var storedBaseData = dataBlock.GetCriticalData<DataItems>(baseDataBlock.Name);
                baseData.Merge(storedBaseData);
            }

            if(dataBlock.Contains(stagedDataBlock.Name))
            {
                var storedStagedData = dataBlock.GetCriticalData<DataItems>(stagedDataBlock.Name);
                stagedData.Merge(storedStagedData);
            }
        }

        /// <summary>
        /// Resets this history recorder, which clears the accumulated data and any staged commits.
        /// </summary>
        /// <remarks>
        /// This method should be called at the start of the game cycle.
        /// </remarks>
        internal void Reset()
        {
            baseData.Clear();
            stagedData.Clear();
            var keys = new[] { baseDataBlock.Name, stagedDataBlock.Name };
            dataStore.Remove(keys);
        }

        /// <summary>
        /// Stages an update, which merges the given data into any previously staged updates that have not yet been recorded.
        /// </summary>
        /// <param name="data">The data to stage.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is null.
        /// </exception>
        internal void StageUpdate(DataItems data)
        {
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            stagedData.Merge(data);

            stagedDataBlock.Data = stagedData;
            dataStore.Write(stagedDataBlock);
        }
    }
}
