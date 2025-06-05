// -----------------------------------------------------------------------
// <copyright file = "HistoryManager.cs" company = "IGT">
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
    /// The <see cref="HistoryManager"/> class is used to coordinate all aspects of tracking game history data.
    /// </summary>
    /// <remarks>
    /// All public/internal methods and property setters in this class need a Lightweight transaction.
    /// </remarks>
    internal class HistoryManager
    {
        private readonly SingleCriticalData<int> stepCountBlock = new SingleCriticalData<int>("HistoryManager.StepCount");
        private readonly SingleCriticalData<bool> historyWriteEnabledBlock = new SingleCriticalData<bool>("HistoryManager.HistoryWriteEnabled");
        private readonly SingleCriticalData<string> markedHistoryStateBlock = new SingleCriticalData<string>("HistoryManager.MarkedHistoryState");

        private readonly ICriticalDataStore gameCycleStore;
        private readonly IShellHistoryQuery shellHistoryQuery;
        private readonly HistoryRecorder historyRecorder;
        private readonly HistoryWriter historyWriter;

        private string markedHistoryState;
        private int stepCount;
        private bool historyWriteEnabled;

        /// <summary>
        /// Gets a flag which indicates if history write is currently enabled.
        /// </summary>
        /// <remarks>
        /// Call <see cref="EnableHistoryWrite"/> or <see cref="DisableHistoryWrite"/> to control this flag.
        /// </remarks>
        internal bool HistoryWriteEnabled
        {
            get => historyWriteEnabled;
            private set
            {
                historyWriteEnabledBlock.Data = value;
                gameCycleStore.Write(historyWriteEnabledBlock);
                historyWriteEnabled = value;
            }
        }

        /// <summary>
        /// Gets the number of history steps that have been written since <see cref="ClearTrackedData"/>
        /// was called.
        /// </summary>
        internal int StepCount
        {
            get => stepCount;
            private set
            {
                stepCountBlock.Data = value;
                gameCycleStore.Write(stepCountBlock);
                stepCount = value;
            }
        }

        /// <summary>
        /// Gets the name of the last marked history state.
        /// </summary>
        private string MarkedHistoryState
        {
            get => markedHistoryState;

            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    markedHistoryStateBlock.Data = value;
                    gameCycleStore.Write(markedHistoryStateBlock);
                }
                else
                {
                    gameCycleStore.Remove(markedHistoryStateBlock.Name);
                }
                markedHistoryState = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryManager"/> class.
        /// </summary>
        /// <param name="shellHistoryQuery">An interface used to query shell history information.</param>
        /// <param name="historyStore">The history store to write history steps to.</param>
        /// <param name="gameCycleStore">The game cycle store to use for recovery.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        internal HistoryManager(IShellHistoryQuery shellHistoryQuery, ICriticalDataStore historyStore, ICriticalDataStore gameCycleStore)
        {
            this.gameCycleStore = gameCycleStore?? throw new ArgumentNullException(nameof(gameCycleStore));
            this.shellHistoryQuery = shellHistoryQuery ?? throw new ArgumentNullException(nameof(shellHistoryQuery));

            if(historyStore == null)
            {
                throw new ArgumentNullException(nameof(historyStore));
            }

            historyRecorder = new HistoryRecorder(gameCycleStore);
            historyWriter = new HistoryWriter(historyStore);
        }

        /// <summary>
        /// Reads configuration from critical data.
        /// </summary>
        internal void ReadConfiguration()
        {
            var dataBlock = gameCycleStore.Read(new CriticalDataName[]
                                                    {
                                                        stepCountBlock.Name,
                                                        markedHistoryStateBlock.Name,
                                                        historyWriteEnabledBlock.Name
                                                    });

            if(dataBlock.Contains(stepCountBlock.Name))
            {
                stepCount = dataBlock.GetCriticalData<int>(stepCountBlock.Name);
            }

            if(dataBlock.Contains(markedHistoryStateBlock.Name))
            {
                markedHistoryState = dataBlock.GetCriticalData<string>(markedHistoryStateBlock.Name);
            }

            if(dataBlock.Contains(historyWriteEnabledBlock.Name))
            {
                historyWriteEnabled = dataBlock.GetCriticalData<bool>(historyWriteEnabledBlock.Name);
            }

            historyRecorder.ReadConfiguration();
        }

        /// <summary>
        /// Enables the history write flag, indicating that it is permissible to write to the history store.
        /// </summary>
        internal void EnableHistoryWrite()
        {
            HistoryWriteEnabled = true;
        }

        /// <summary>
        /// Disables the history write flag, indicating that it is not permissible to write to the history store.
        /// </summary>
        internal void DisableHistoryWrite()
        {
            HistoryWriteEnabled = false;

            // If history write is disabled after a step has begun (a presentation state has started),
            // remove the step marker.
            MarkedHistoryState = null;
        }

        /// <summary>
        /// Begins a new history step for the given state.
        /// </summary>
        /// <param name="historyState">The name of the state to begin a history step for.</param>
        internal void BeginHistoryStep(string historyState)
        {
            MarkedHistoryState = historyState;
        }

        /// <summary>
        /// Ends a history step, if one was started.
        /// </summary>
        /// <remarks>
        /// If <see cref="BeginHistoryStep(string)"/> was previously called, then calling this method will create a new
        /// history step for the marked state, containing the currently tracked data. If 
        /// <see cref="BeginHistoryStep(string)"/> has not been called, then calling this method has no effect.
        /// </remarks>
        internal void EndHistoryStep()
        {
            if(MarkedHistoryState == null)
            {
                return;
            }

            // This effectively makes HistoryRecorder.StepNumber 1-based.
            var stepNumber = StepCount + 1;

            // TODO: calculate diffs for shell history data.
            var shellRecord = shellHistoryQuery.GetHistoryRecord(stepNumber);
            var coplayerRecord = historyRecorder.CreateHistoryRecord(stepNumber, MarkedHistoryState);
            historyWriter.WriteHistoryStep(coplayerRecord, shellRecord);
            StepCount++;
            MarkedHistoryState = null;
        }

        /// <summary>
        /// Clears all tracked data from the history manager.
        /// </summary>
        /// <remarks>
        /// This method should be called, for example, at the start of a new game cycle.
        /// </remarks>
        internal void ClearTrackedData()
        {
            historyRecorder.Reset();
            StepCount = 0;
        }

        /// <summary>
        /// Instructs the history manager to track the given data.
        /// </summary>
        /// <param name="newData">The data to track for history.</param>
        internal void TrackData(DataItems newData)
        {
            historyRecorder.StageUpdate(newData);
        }
    }
}