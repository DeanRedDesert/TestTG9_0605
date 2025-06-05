// -----------------------------------------------------------------------
// <copyright file = "GameMeterGroup.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using GameMeter;

    /// <summary>
    /// This class implements the <see cref="IGameMeterGroup"/> that manipulates
    /// the game meters at group base.
    /// </summary>
    internal class GameMeterGroup : IGameMeterGroup
    {
        #region Constants

        /// <summary>
        /// The delimiter for critical data path.
        /// </summary>
        private const string PathDelimiter = "/";

        /// <summary>
        /// The critical data name for meter IDs in this group.
        /// </summary>
        private const string CriticalDataNameOfMeterIds = "MeterIds";

        /// <summary>
        /// The name of critical data directory where contains all meters of this group.
        /// </summary>
        /// <remarks>
        /// Here is the more detailed description to illustrate how the critical data of the meters are
        /// organized in the meter group:
        /// <list type="bullet">
        /// <item>
        ///   "{meter_group_root_path}"                 : The critical data root path for this meter group
        /// </item>
        /// <item>
        ///   "{meter_group_root_path}/MeterIds"        : The critical data for saving the list of meter IDs
        ///                                               in this meter group
        /// </item>
        /// <item>
        ///   "{meter_group_root_path}/Meters/"         : The critical data root path under which contains
        ///                                               all the individual meters
        /// </item>
        /// <item>
        ///   "{meter_group_root_path}/Meters/{meter1}" : The critical data for saving the value of meter1
        /// </item>
        /// <item>
        ///   "{meter_group_root_path}/Meters/{meter2}" : The critical data for saving the value of meter2
        /// </item>
        /// </list>
        /// </remarks>
        private const string CriticalDataDirectoryOfMeters = "Meters";

        #endregion

        #region Private Fields

        /// <summary>
        /// The game critical data interface.
        /// </summary>
        private readonly IGameCriticalData criticalDataInterface;

        /// <summary>
        /// The critical data accessor interface.
        /// </summary>
        private readonly ICriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// The critical data scope for accessing.
        /// </summary>
        private readonly CriticalDataScope criticalDataScope;

        /// <summary>
        /// The scope identifier to access the critical data through the critical data accessor interface.
        /// </summary>
        private readonly string scopeIdentifier;

        /// <summary>
        /// The critical data path root of the group.
        /// </summary>
        private readonly string criticalDataPathOfMeterGroup;

        /// <summary>
        /// The critical data path for saving the list of game meter IDs.
        /// </summary>
        private readonly string criticalDataPathOfMeterIds;

        /// <summary>
        /// The interface to access the game meters.
        /// </summary>
        private readonly IGameMeterAccessor gameMeterAccessor;

        /// <summary>
        /// The cache of the critical data path of all the meters in the group.
        /// </summary>
        private Dictionary<string, string> gameMeterPaths;

        /// <summary>
        /// Gets the meter IDs and their critical data paths for all the game meters in the group.
        /// </summary>
        /// <devdoc>
        /// This property can be called only from within a method that requires an open transaction.
        /// </devdoc>
        private Dictionary<string, string> GameMeterPaths
        {
            get
            {
                if(gameMeterPaths == null)
                {
                    // Initially load the list of game meter IDs from the critical data.
                    List<string> meterIds;
                    if(criticalDataAccessor != null)
                    {
                        var selectors = new List<CriticalDataSelector>
                        {
                            new CriticalDataSelector(
                                        criticalDataScope,
                                        scopeIdentifier,
                                        criticalDataPathOfMeterIds),
                        };
                        meterIds = criticalDataAccessor.ReadTransactionalCriticalData(selectors)
                                        .RetrieveCriticalData<List<string>>(selectors[0]) ?? new List<string>();

                    }
                    else if(criticalDataInterface != null)
                    {
                        meterIds = criticalDataInterface.ReadCriticalData<List<string>>(
                                        criticalDataScope,
                                        criticalDataPathOfMeterIds) ?? new List<string>();
                    }
                    else
                    {
                        meterIds = new List<string>();
                    }

                    // Cache the game meter paths.
                    // ReSharper disable once ConvertClosureToMethodGroup
                    gameMeterPaths = meterIds.ToDictionary(id => id, id => CalculateMeterPathInGroup(id));
                }
                return gameMeterPaths;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GameMeterGroup" /> with the game critical data interface.
        /// </summary>
        /// <param name="criticalDataInterface">
        /// The game critical data interface.
        /// </param>
        /// <param name="criticalDataScope">
        /// The critical data scope for accessing through the game critical data interface.
        /// </param>
        /// <param name="pathOfMeterGroup">
        /// The critical data path root for the meter group.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataInterface"/> is null, or <paramref name="pathOfMeterGroup"/>
        /// is null or empty.
        /// </exception>
        public GameMeterGroup(IGameCriticalData criticalDataInterface,
                              CriticalDataScope criticalDataScope,
                              string pathOfMeterGroup)
        {
            if(string.IsNullOrEmpty(pathOfMeterGroup))
            {
                throw new ArgumentNullException(nameof(pathOfMeterGroup));
            }

            criticalDataPathOfMeterGroup = pathOfMeterGroup;
            criticalDataPathOfMeterIds = criticalDataPathOfMeterGroup + PathDelimiter + CriticalDataNameOfMeterIds;
            this.criticalDataInterface = criticalDataInterface ?? throw new ArgumentNullException(nameof(criticalDataInterface));
            this.criticalDataScope = criticalDataScope;
            gameMeterAccessor = new LegacyGameMeterAccessor(criticalDataInterface, criticalDataScope, true);
        }

        /// <summary>
        /// Initializes an instance of <see cref="GameMeterGroup" /> with the critical data accessor interface.
        /// </summary>
        /// <param name="criticalDataAccessor">
        /// The critical data accessor interface.
        /// </param>
        /// <param name="criticalDataScope">
        /// The critical data scope for accessing through the critical data accessor interface.
        /// </param>
        /// <param name="scopeIdentifier">
        /// The scope identifier to access the critical data through the critical data accessor interface.
        /// </param>
        /// <param name="pathOfMeterGroup">
        /// The critical data path root for the group.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataAccessor"/> is null, or <paramref name="scopeIdentifier"/>
        /// or <paramref name="pathOfMeterGroup"/> is null or empty.
        /// </exception>
        public GameMeterGroup(ICriticalDataAccessor criticalDataAccessor,
                              CriticalDataScope criticalDataScope,
                              string scopeIdentifier,
                              string pathOfMeterGroup)
        {
            if(string.IsNullOrEmpty(scopeIdentifier))
            {
                throw new ArgumentNullException(nameof(scopeIdentifier));
            }
            if(string.IsNullOrEmpty(pathOfMeterGroup))
            {
                throw new ArgumentNullException(nameof(pathOfMeterGroup));
            }

            criticalDataPathOfMeterGroup = pathOfMeterGroup;
            criticalDataPathOfMeterIds = criticalDataPathOfMeterGroup + PathDelimiter + CriticalDataNameOfMeterIds;
            this.criticalDataAccessor = criticalDataAccessor ?? throw new ArgumentNullException(nameof(criticalDataAccessor));
            this.criticalDataScope = criticalDataScope;
            this.scopeIdentifier = scopeIdentifier;
            gameMeterAccessor = new GameMeterAccessor(criticalDataAccessor, criticalDataScope, scopeIdentifier);
        }

        #endregion

        #region IGameMeterGroup Implementation

        /// <summary>
        /// Increments a single meter in the group.
        /// </summary>
        /// <param name="meterId">The meter ID  in the group to increment.</param>
        public void IncrementMeter(string meterId)
        {
            if(!GameMeterPaths.ContainsKey(meterId))
            {
                // Update the local cache for game meter paths.
                GameMeterPaths.Add(meterId, CalculateMeterPathInGroup(meterId));

                // Save the new list of game meter IDs to the critical data.
                UpdateMeterIdsInCriticalData(GameMeterPaths.Keys);
            }
            gameMeterAccessor.IncrementMeter(GameMeterPaths[meterId]);
        }

        /// <summary>
        /// Increments the multiple meters in the group.
        /// </summary>
        /// <param name="meterIds">The list of meter IDs in the group to increment.</param>
        public void IncrementMeters(IEnumerable<string> meterIds)
        {
            meterIds.ToList().ForEach(IncrementMeter);
        }

        /// <summary>
        /// Increments all the meters in the group.
        /// </summary>
        public void IncrementAllMeters()
        {
            IncrementMeters(GameMeterPaths.Keys);
        }

        /// <summary>
        /// Gets a single meter value in the group.
        /// </summary>
        /// <param name="meterId">The meter ID.</param>
        /// <returns>The meter value.</returns>
        public long GetMeter(string meterId)
        {
            return GameMeterPaths.ContainsKey(meterId)
                    ? gameMeterAccessor.GetMeter(GameMeterPaths[meterId])
                    : 0;
        }

        /// <summary>
        /// Gets the values of multiple meters in the group.
        /// </summary>
        /// <param name="meterIds">The list of meter IDs.</param>
        /// <returns>A dictionary containing the list of meter IDs and meter values.</returns>
        public IDictionary<string, long> GetMeters(IEnumerable<string> meterIds)
        {
            if(meterIds == null)
            {
                return null;
            }
            var ids = meterIds.ToList();
            var meterValues = gameMeterAccessor.GetMeters(ids.Select(id => GameMeterPaths[id])) ?? new List<long>();
            return ids.Select((id, index) => new {key = id, value = meterValues.ToList()[index]})
                      .ToDictionary(item => item.key, item => item.value);
        }

        /// <summary>
        /// Gets the values of all the meters in the group.
        /// </summary>
        /// <returns>A dictionary containing the list of meter IDs and meter values.</returns>
        public IDictionary<string, long> GetAllMeters()
        {
            return GetMeters(GameMeterPaths.Keys);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes sure that the game critical data interface is initialized.
        /// </summary>
        private void CheckGameCriticalDataInterface()
        {
            if(criticalDataInterface == null)
            {
                throw new InvalidOperationException(
                    "Operation is not supported since criticalDataInterface has not been initialized " +
                    "to write the critical data!");
            }
        }

        /// <summary>
        /// Calculates the critical data path of the meter in the group.
        /// </summary>
        /// <param name="meterId">The meter ID.</param>
        /// <returns>The calculated critical data path.</returns>
        private string CalculateMeterPathInGroup(string meterId)
        {
            return criticalDataPathOfMeterGroup + PathDelimiter + CriticalDataDirectoryOfMeters + PathDelimiter + meterId;
        }

        /// <summary>
        /// Updates the list of meter IDs for the group into the critical data.
        /// </summary>
        /// <param name="meterIds">The new list of meter IDs to write to the critical data.</param>
        private void UpdateMeterIdsInCriticalData(IEnumerable<string> meterIds)
        {
            // Make sure that the game critical data interface is initialized.
            CheckGameCriticalDataInterface();

            // Write the list of meter IDs to the critical data.
            criticalDataInterface.WriteCriticalData(criticalDataScope,
                                                    criticalDataPathOfMeterIds,
                                                    meterIds.ToList());
        }

        #endregion
    }
}
