//-----------------------------------------------------------------------
// <copyright file = "HistorySettings.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.CommunicationLib;

    /// <summary>
    /// This class is used to set history related settings when create a state in state machine.
    /// </summary>
    public class HistorySettings
    {
        #region Constructors

        /// <summary>
        /// Create history settings.
        /// </summary>
        public HistorySettings()
        {
        }

        /// <summary>
        /// Create history settings with the priority of the state.
        /// </summary>
        /// <param name="priority">
        /// The priority of the state that determines whether this state should be deleted
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </param>
        public HistorySettings(uint priority)
        {
            historyStepPriority = priority;
        }

        /// <summary>
        /// Create history settings by specifying start state and update data event handlers.
        /// </summary>
        /// <param name="startStateWriter">
        /// A delegate which handles writing of start state message data for history.
        /// If the argument is null, then standard serialization will be used for 
        /// StartState messages.
        /// </param>
        /// <param name="updateDataWriter">
        /// A delegate which handles writing of asynchronous update messages for history.
        /// If the argument is null, then no history will be written for updates.
        /// </param>
        /// <exception cref="ArgumentException">
        /// At least one of the handlers should not be null, or the 
        /// exception will be thrown.
        /// </exception>
        public HistorySettings(WriteStartStateHistoryHandler startStateWriter,
                               WriteUpdateDataHistoryHandler updateDataWriter)
        {
            if(startStateWriter == null && updateDataWriter == null)
            {
                throw new ArgumentException("StartStateWriter and UpdateDataWriter cannot both be null.");
            }

            if(startStateWriter != null)
            {
                StartStateHistoryHandler = startStateWriter;
            }
            UpdateDataHistoryHandler = updateDataWriter;
        }

        /// <summary>
        /// Create history settings by specifying start state, update data event handlers
        /// and the priority of the state.
        /// </summary>
        /// <param name="startStateWriter">
        /// A delegate which handles writing of start state message data for history.
        /// If the argument is null, then standard serialization will be used for 
        /// StartState messages.
        /// </param>
        /// <param name="updateDataWriter">
        /// A delegate which handles writing of asynchronous update messages for history.
        /// If the argument is null, then no history will be written for updates.
        /// </param>
        /// <param name="priority">
        /// The priority of the state that determines whether this state should be deleted
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </param>
        public HistorySettings(WriteStartStateHistoryHandler startStateWriter,
                               WriteUpdateDataHistoryHandler updateDataWriter,
                               uint priority)
            : this(startStateWriter, updateDataWriter)
        {
            historyStepPriority = priority;
        }

        /// <summary>
        /// Create history settings by specifying the service which should be 
        /// saved to history in async update.
        /// </summary>
        /// <param name="providerNameToSaveAsyncHistoryData">
        /// Provider name of the async update service to save to history data.
        /// </param>
        /// <param name="serviceNameToSaveAsyncHistoryData">
        /// Service name of the async update service to save to history data.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="providerNameToSaveAsyncHistoryData"/> or 
        /// <paramref name="serviceNameToSaveAsyncHistoryData"/> is null or empty,
        /// then this exception will be thrown.
        /// </exception>
        public HistorySettings(string providerNameToSaveAsyncHistoryData,
                               string serviceNameToSaveAsyncHistoryData)
        {
            if(string.IsNullOrEmpty(providerNameToSaveAsyncHistoryData))
            {
                throw new ArgumentException("Argument may not be null or empty.",
                                            "providerNameToSaveAsyncHistoryData");
            }

            if(string.IsNullOrEmpty(serviceNameToSaveAsyncHistoryData))
            {
                throw new ArgumentException("Argument may not be null or empty.",
                                            "serviceNameToSaveAsyncHistoryData");
            }

            servicePairList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(providerNameToSaveAsyncHistoryData,
                                                     serviceNameToSaveAsyncHistoryData)
                };
        }

        /// <summary>
        /// Create history settings by specifying the services which should be
        /// saved to history in async update.
        /// </summary>
        /// <param name="asyncProviderServicePairToSaveToHistory">
        /// Provider and service pair of the async update to save to history data.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="asyncProviderServicePairToSaveToHistory"/> is null or empty list,
        /// then this exception will be thrown.
        /// </exception>
        public HistorySettings(List<KeyValuePair<string, string>> asyncProviderServicePairToSaveToHistory)
        {
            if(asyncProviderServicePairToSaveToHistory == null || asyncProviderServicePairToSaveToHistory.Count == 0)
            {
                throw new ArgumentException("Argument may not be null or empty list.",
                                            "asyncProviderServicePairToSaveToHistory");
            }

            servicePairList = asyncProviderServicePairToSaveToHistory;
        }

        /// <summary>
        /// Create history settings by specifying the services which should be
        /// saved to history in async update as well as the priority of this state.
        /// </summary>
        /// <param name="asyncProviderServicePairToSaveToHistory">
        /// Provider and service pair of the async update to save to history data.
        /// </param>
        /// <param name="priority">
        /// The priority of the state that determines whether this state should be deleted 
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </param>
        public HistorySettings(List<KeyValuePair<string, string>> asyncProviderServicePairToSaveToHistory, uint priority)
            : this(asyncProviderServicePairToSaveToHistory)
        {
            historyStepPriority = priority;
        }

        /// <summary>
        /// Create history settings by specifying start state event handler and
        /// services which should be saved to history in async update.
        /// </summary>
        /// <param name="startStateWriter">
        /// A delegate which handles writing of start state message data for history.
        /// </param>
        /// <param name="asyncProviderServicePairToSaveToHistory">
        /// Provider and service pair of the async update to save to history data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="startStateWriter"/> is null, then this exception will be thrown.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="asyncProviderServicePairToSaveToHistory"/> is null or empty list,
        /// then this exception will be thrown.
        /// </exception>
        public HistorySettings(WriteStartStateHistoryHandler startStateWriter,
                               List<KeyValuePair<string, string>> asyncProviderServicePairToSaveToHistory)
        {
            if(startStateWriter == null)
            {
                throw new ArgumentNullException("startStateWriter", "Argument may not be null");
            }

            if(asyncProviderServicePairToSaveToHistory == null || asyncProviderServicePairToSaveToHistory.Count == 0)
            {
                throw new ArgumentException("Argument may not be null or empty list.",
                                            "asyncProviderServicePairToSaveToHistory");
            }

            servicePairList = asyncProviderServicePairToSaveToHistory;

            StartStateHistoryHandler = startStateWriter;
        }

        /// <summary>
        /// Create history settings by specifying start state event handler and
        /// services which should be saved to history in async update 
        /// as well as the priority of this state.
        /// </summary>
        /// <param name="startStateWriter">
        /// A delegate which handles writing of start state message data for history.
        /// </param>
        /// <param name="asyncProviderServicePairToSaveToHistory">
        /// Provider and service pair of the async update to save to history data.
        /// </param>
        /// <param name="priority">
        /// The priority of the state that determines whether this state should be deleted
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </param>
        public HistorySettings(WriteStartStateHistoryHandler startStateWriter,
                               List<KeyValuePair<string, string>> asyncProviderServicePairToSaveToHistory,
                               uint priority)
            : this(startStateWriter, asyncProviderServicePairToSaveToHistory)
        {
            historyStepPriority = priority;
        }

        #endregion

        #region Private field

        /// <summary>
        /// Default write start state history handler.
        /// </summary>
        private WriteStartStateHistoryHandler startStateHistoryHandler = DefaultWriteStartStateHistoryHandler;

        private Dictionary<string, Dictionary<string, int[]>> asyncServicesToSaveToHistory;
        
        /// <summary>
        /// Provider and service pair of the async update to save to history data.
        /// </summary>
        private readonly List<KeyValuePair<string, string>> servicePairList;

        /// <summary>
        /// The priority of the state that determines whether this state should be deleted 
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </summary>
        private readonly uint historyStepPriority;

        #endregion

        #region Private methods

        /// <summary>
        /// Default start state history handler.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="data">Start state data.</param>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <returns>Block data for saving history.</returns>
        private static CommonHistoryBlock DefaultWriteStartStateHistoryHandler(string stateName, DataItems data,
                                                                 IStateMachineFramework framework)
        {
            return new CommonHistoryBlock(stateName, data);
        }

        #endregion

        #region Public properties and fields

        /// <summary>
        /// Callback used for writing history for start state messages.
        /// </summary>
        public WriteStartStateHistoryHandler StartStateHistoryHandler
        {
            get
            {
                return startStateHistoryHandler;
            }
            private set
            {
                startStateHistoryHandler = value;
            }
        }

        /// <summary>
        /// Callback used for writing history for asynchronous update messages.
        /// </summary>
        public WriteUpdateDataHistoryHandler UpdateDataHistoryHandler { get; private set; }

        /// <summary>
        /// A Dictionary to save the services that should be written
        /// to history data during async update.
        /// Key: is the service provider name.
        /// Value: is a Dictionary of service name and identifier array pair.
        /// </summary>
        /// <remarks>
        /// This property is made internal for testing.
        /// </remarks>
        internal Dictionary<string, Dictionary<string, int[]>> AsyncServicesToSaveToHistory
        {
            get
            {
                if(servicePairList == null)
                {
                    asyncServicesToSaveToHistory = new Dictionary<string, Dictionary<string, int[]>>();
                }

                return asyncServicesToSaveToHistory ??
                       (asyncServicesToSaveToHistory = servicePairList.GroupBy(pair => pair.Key)
                                                                      .ToDictionary(group => group.Key,
                                                                                    group =>
                                                                                    group.ToDictionary(
                                                                                       pair => pair.Value,
                                                                                       pair => (int[])null)));
            }
        }

        /// <summary>
        /// Check if save asynchronous update data to history.
        /// </summary>
        public bool IsAsynchronousHistory
        {
            get
            {
                return UpdateDataHistoryHandler != null || AsyncServicesToSaveToHistory.Count > 0;
            }
        }

        /// <summary>
        /// The priority of the state that determines whether this state should be deleted 
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </summary>
        public uint HistoryStepPriority
        {
            get { return historyStepPriority; }
        }

        #endregion

        /// <summary>
        /// Check if has async services to update.
        /// </summary>
        internal bool HasAsyncServicesToUpdate
        {
            get
            {
                return AsyncServicesToSaveToHistory.Any();
            }
        }

        /// <summary>
        /// Get a collection of identifiers corresponding to the services whose value should be updated
        /// to history data during async update.
        /// </summary>
        /// <param name="provider">The provider name.</param>
        /// <param name="getIdentifiers">A delegate for history settings to retrieve identifiers.</param>
        /// <returns>
        /// A collection of identifiers corresponding to the services whose value should be updated
        /// to history data during async update.
        /// </returns>
        internal ICollection<int> GetAsyncServicesToUpdate(string provider, Func<string, int[]> getIdentifiers)
        {
            var result = new List<int>();
            if(AsyncServicesToSaveToHistory.ContainsKey(provider))
            {
                var servicesToUpdate =
                    AsyncServicesToSaveToHistory[provider].Where(entry => entry.Value == null)
                                                          .Select(entry => entry.Key)
                                                          .ToList();
                foreach(var serviceName in servicesToUpdate)
                {
                    AsyncServicesToSaveToHistory[provider][serviceName] = getIdentifiers(serviceName);
                }

                foreach(var identifiers in AsyncServicesToSaveToHistory[provider].Values)
                {
                    if(identifiers != null)
                    {
                        result.AddRange(identifiers);
                    }
                }
            }
            return result;
        }
    }
}
