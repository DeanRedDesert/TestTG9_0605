// -----------------------------------------------------------------------
// <copyright file = "ServiceRequestDataMaps.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.Collections.Generic;
    using Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// This class manages service request data maps based on game modes and service notification types.
    /// </summary>
    internal sealed class ServiceRequestDataMaps
    {
        #region Private Fields

        /// <summary>
        /// Service request data per state names for both Synchronous and Asynchronous services, in Play and Utility modes.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> playMap = new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// Service request data per state names for Asynchronous services, in Play and Utility modes.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> playAsyncMap = new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// Service request data per state names for both Synchronous and Asynchronous services, in History mode.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> historyMap = new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// Service request data per state names for Asynchronous services, in History mode.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> historyAsyncMap = new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// An empty service request data instance.
        /// </summary>
        private readonly ServiceRequestData emptyServiceRequestData = new ServiceRequestData();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceRequestDataMaps"/>.
        /// </summary>
        /// <remarks>
        /// During the construction time, <paramref name="masterMap"/> is split into multiple maps,
        /// each holding data for a specific game mode and service notification type(s).
        /// </remarks>
        /// <param name="masterMap">
        /// The master service request data map that contains data for all game modes and service notification types.
        /// </param>
        public ServiceRequestDataMaps(IDictionary<string, ServiceRequestData> masterMap)
        {
            if(masterMap == null)
            {
                return;
            }

            foreach(var stateEntry in masterMap)
            {
                var stateName = stateEntry.Key;

                foreach(var providerEntry in stateEntry.Value)
                {
                    var providerName = providerEntry.Key;

                    foreach(var serviceAccessor in providerEntry.Value)
                    {
                        var isHistory = serviceAccessor.NotificationType.IsHistory();
                        var isAsync = serviceAccessor.NotificationType.IsAsynchronous();

                        // First add both sync and async ones to the total map.
                        var map = GetMap(isHistory, false);
                        AddToMap(map, stateName, providerName, serviceAccessor);

                        // Then add async ones to the async only map.
                        if(isAsync)
                        {
                            map = GetMap(isHistory, true);
                            AddToMap(map, stateName, providerName, serviceAccessor);
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the service request data of the specified state that are
        /// for both Synchronous and Asynchronous services, in Play and Utility modes.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state whose service request data is to get.
        /// </param>
        /// <returns>
        /// The service request data as requested.
        /// If no data is found, an empty instance will be returned.
        /// </returns>
        public ServiceRequestData GetPlayRequestData(string stateName)
        {
            return GetServiceRequestData(stateName, false, false);
        }

        /// <summary>
        /// Gets the service request data of the specified state that are
        /// for Asynchronous services, in Play and Utility modes.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state whose service request data is to get.
        /// </param>
        /// <returns>
        /// The service request data as requested.
        /// If no data is found, an empty instance will be returned.
        /// </returns>
        public ServiceRequestData GetPlayAsyncRequestData(string stateName)
        {
            return GetServiceRequestData(stateName, false, true);
        }

        /// <summary>
        /// Gets the service request data of the specified state that are
        /// for both Synchronous and Asynchronous services, in History mode.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state whose service request data is to get.
        /// </param>
        /// <returns>
        /// The service request data as requested.
        /// If no data is found, an empty instance will be returned.
        /// </returns>
        public ServiceRequestData GetHistoryRequestData(string stateName)
        {
            return GetServiceRequestData(stateName, true, false);
        }

        /// <summary>
        /// Gets the service request data of the specified state that are
        /// for Asynchronous services, in History mode.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state whose service request data is to get.
        /// </param>
        /// <returns>
        /// The service request data as requested.
        /// If no data is found, an empty instance will be returned.
        /// </returns>
        public ServiceRequestData GetHistoryAsyncRequestData(string stateName)
        {
            return GetServiceRequestData(stateName, true, true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the appropriate map for the given game mode and service notification types.
        /// </summary>
        /// <param name="forHistory">
        /// Whether the map is for history mode.
        /// </param>
        /// <param name="asyncOnly">
        /// Whether the map is for async services only.
        /// </param>
        /// <returns>
        /// The map requested.
        /// </returns>
        private IDictionary<string, ServiceRequestData> GetMap(bool forHistory, bool asyncOnly)
        {
            IDictionary<string, ServiceRequestData> result;

            if(forHistory)
            {
                result = asyncOnly ? historyAsyncMap : historyMap;
            }
            else
            {
                result = asyncOnly ? playAsyncMap : playMap;
            }

            return result;
        }

        /// <summary>
        /// Adds a service accessor to a given map, creating new entries in the dictionaries as needed.
        /// </summary>
        /// <param name="map">
        /// The map to which the service accessor is added.
        /// </param>
        /// <param name="stateName">
        /// The name of the state where the service accessor belongs.
        /// </param>
        /// <param name="providerName">
        /// The name of the service provider.
        /// </param>
        /// <param name="serviceAccessor">
        /// The service accessor to add.
        /// </param>
        private static void AddToMap(IDictionary<string, ServiceRequestData> map, string stateName,
                                     string providerName, ServiceAccessor serviceAccessor)
        {
            if(!map.TryGetValue(stateName, out var serviceRequestData))
            {
                serviceRequestData = new ServiceRequestData();
                map[stateName] = serviceRequestData;
            }

            if(!serviceRequestData.TryGetValue(providerName, out var serviceAccessors))
            {
                serviceAccessors = new List<ServiceAccessor>();
                serviceRequestData[providerName] = serviceAccessors;
            }

            serviceAccessors.Add(serviceAccessor);
        }

        /// <summary>
        /// Gets the service request data per state.
        /// </summary>
        /// <param name="stateName">
        /// The state name.
        /// </param>
        /// <param name="isHistory">
        /// The flag indicating if the request is for history mode.
        /// </param>
        /// <param name="onlyAsync">
        /// The flag indicating if only the data of async services should be returned.
        /// </param>
        /// <returns>
        /// The service request data as requested.  Empty instance if none is found.
        /// </returns>
        private ServiceRequestData GetServiceRequestData(string stateName, bool isHistory, bool onlyAsync)
        {
            var map = GetMap(isHistory, onlyAsync);

            return map.TryGetValue(stateName, out var result) ? result : EmptyServiceRequestData();
        }

        /// <summary>
        /// Makes sure the empty instance is empty and returns it.
        /// </summary>
        /// <returns>
        /// An empty instance of service request data.
        /// </returns>
        private ServiceRequestData EmptyServiceRequestData()
        {
            emptyServiceRequestData.Clear();
            return emptyServiceRequestData;
        }

        #endregion
    }
}