// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceMeterManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class implements both the <see cref="IGamePerformanceMeterWrite"/> which uses legacy
    /// F2L critical data interface and allows updating the game performance meters in the critical
    /// data, and the <see cref="IGamePerformanceMeterRead"/> which uses F2X critical data accessor
    /// and allows reading the game performance meters from the critical data.<br />
    /// In this implementation, all the critical data related to game performance meters are stored
    /// in the PayvarAnalytics scope.
    /// </summary>
    /// <remarks>
    /// Note that this class will not work if an extension wants to write some performance meters.
    /// </remarks>
    public class GamePerformanceMeterManager : IGamePerformanceMeterWrite, IGamePerformanceMeterRead
    {
        #region Private Fields

        /// <summary>
        /// The critical data scope for accessing game performance data.
        /// </summary>
        private const CriticalDataScope CriticalDataScope = Ascent.Communication.Platform.Interfaces.CriticalDataScope.PayvarAnalytics;

        /// <summary>
        /// The prefix string of the critical data path for game performance data.
        /// </summary>
        private const string GamePerformanceDataPrefix = "GamePerformanceData";

        /// <summary>
        /// The critical data path for storing the list of all group IDs, including the predefined
        /// group IDs as defined in this class.
        /// </summary>
        private const string CriticalDataPathOfMeterGroupIds = GamePerformanceDataPrefix + "/MeterGroupIDs";

        /// <summary>
        /// The meter ID format with prefix label, separate with "_".
        /// </summary>
        private const string MeterIdFormatWithPrefixLabel = @"{0}_{1}";

        /// <summary>
        /// The meter ID format with 2D labels, separate the labels with ".".
        /// </summary>
        private const string MeterIdFormatWith2DLabel = @"{0}.{1}";

        /// <summary>
        /// The prefix filter for meter ID that allows alphabetic letters only.
        /// </summary>
        private const string MeterIdPrefixFilter = @"[^a-zA-Z]";

        /// <summary>
        /// The interface to access the critical data from legacy F2L.
        /// </summary>
        private readonly IGameCriticalData criticalDataInterface;

        /// <summary>
        /// The interface to access the critical data from F2X.
        /// </summary>
        private readonly ICriticalDataAccessor criticalDataAccessor;

        /// <summary>
        /// The paytable identifier to access the critical data from F2X.
        /// </summary>
        private readonly string paytableIdentifier;

        /// <summary>
        /// The denomination.
        /// </summary>
        private readonly long denom;

        /// <summary>
        /// The local cache of all the game meter groups.
        /// </summary>
        private Dictionary<string, IGameMeterGroup> meterGroups;

        /// <summary>
        /// Gets all the game meter groups owned by this performance meter manager.
        /// </summary>
        /// <devdoc>
        /// This property can be called only from within a method that requires an open transaction.
        /// </devdoc>
        private Dictionary<string, IGameMeterGroup> MeterGroups
        {
            get
            {
                if(meterGroups == null)
                {
                    // Initially load the list of meter group IDs from the critical data.
                    List<string> groupIds;
                    if(criticalDataAccessor != null)
                    {
                        var selectors = new List<CriticalDataSelector>
                        {
                            new CriticalDataSelector(CriticalDataScope,
                                                     paytableIdentifier,
                                                     CriticalDataPathOfMeterGroupIds),
                        };
                        groupIds = criticalDataAccessor.ReadTransactionalCriticalData(selectors)
                                        .RetrieveCriticalData<List<string>>(selectors[0]) ?? new List<string>();

                    }
                    else if(criticalDataInterface != null)
                    {
                        groupIds = criticalDataInterface.ReadCriticalData<List<string>>(
                                        CriticalDataScope,
                                        CriticalDataPathOfMeterGroupIds) ?? new List<string>();
                    }
                    else
                    {
                        groupIds = new List<string>();
                    }

                    meterGroups = groupIds.ToDictionary(id => id, id =>
                    {
                        if(criticalDataAccessor != null)
                        {
                            return new GameMeterGroup(
                                        criticalDataAccessor,
                                        CriticalDataScope,
                                        paytableIdentifier,
                                        CalculateCriticalDataPathOfGroup(id));
                        }
                        if(criticalDataInterface != null)
                        {
                            return new GameMeterGroup(
                                        criticalDataInterface,
                                        CriticalDataScope,
                                        CalculateCriticalDataPathOfGroup(id));
                        }
                        return (IGameMeterGroup)null;
                    });
                }
                return meterGroups;
            }
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// The predefined group ID for all meters of games played.
        /// </summary>
        public const string GroupIdGamesPlayedCount = "GamesPlayedCount";

        /// <summary>
        /// The predefined group ID for all meters of games played with different bet definitions.
        /// </summary>
        public const string GroupIdGamesPlayedWithBetDefinition = "GamesPlayedWithBetDefinition";

        /// <summary>
        /// The predefined group ID for all meters of games played with progressive hit.
        /// </summary>
        public const string GroupIdGamesPlayedWithProgressiveHit = "GamesPlayedWithProgressiveHit";

        /// <summary>
        /// The predefined group ID for all meters of game miscellaneous performance data, which allows
        /// game to record its own performance data without a specific group.
        /// </summary>
        public const string GroupIdMiscellaneousPerformanceMeters = "MiscellaneousPerformanceMeter";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GamePerformanceMeterManager"/> for legacy F2L application.
        /// </summary>
        /// <param name="criticalDataInterface">The game critical data interface.</param>
        /// <param name="denom">The denomination.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataInterface"/> is null.
        /// </exception>
        public GamePerformanceMeterManager(IGameCriticalData criticalDataInterface, long denom)
        {
            this.criticalDataInterface = criticalDataInterface ?? throw new ArgumentNullException(nameof(criticalDataInterface));
            this.denom = denom;
        }

        /// <summary>
        /// Initializes an instance of <see cref="GamePerformanceMeterManager"/> for F2X application.
        /// </summary>
        /// <param name="criticalDataAccessor">The critical data accessor interface.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <param name="denom">The denomination.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataAccessor"/> is null, or <paramref name="paytableIdentifier"/>
        /// is null or empty.
        /// </exception>
        public GamePerformanceMeterManager(ICriticalDataAccessor criticalDataAccessor,
                                           string paytableIdentifier,
                                           long denom)
        {
            if(string.IsNullOrEmpty(paytableIdentifier))
            {
                throw new ArgumentNullException(nameof(paytableIdentifier));
            }

            this.criticalDataAccessor = criticalDataAccessor ?? throw new ArgumentNullException(nameof(criticalDataAccessor));
            this.paytableIdentifier = paytableIdentifier;
            this.denom = denom;
        }

        #endregion

        #region IGamePerformanceMeterWrite Implementation

        /// <inheritdoc />
        public string FormatMeterIdWith2DLabel(string label1, string label2)
        {
            return string.Format(MeterIdFormatWith2DLabel, label1, label2);
        }

        /// <inheritdoc />
        public string FormatMeterIdWithPrefixLabel(string prefix, long value)
        {
            return string.Format(MeterIdFormatWithPrefixLabel, prefix, value);
        }

        /// <inheritdoc />
        public void IncrementGamesPlayed(string meterId)
        {
            // Increments a performance meter in predefined group.
            IncrementPerformanceMeter(GroupIdGamesPlayedCount, meterId);
        }

        /// <inheritdoc />
        public void IncrementGamesPlayedWithBetDefinition(string betDefinition)
        {
            // Increments a performance meter in predefined group.
            IncrementPerformanceMeter(GroupIdGamesPlayedWithBetDefinition, betDefinition);
        }

        /// <inheritdoc />
        public void IncrementGamesPlayedWithBetDefinition(
                                        string betPatternPrefix,
                                        long betPatternValue,
                                        string creditPerPatternPrefix,
                                        long creditPerPatternValue)
        {
            var filteredBetPatternPrefix = Regex.Replace(betPatternPrefix, MeterIdPrefixFilter, "");
            var filteredCreditPerPatternPrefix = Regex.Replace(creditPerPatternPrefix, MeterIdPrefixFilter, "");

            // Format the bet definition.
            var betDefinition = FormatMeterIdWith2DLabel(
                FormatMeterIdWithPrefixLabel(filteredBetPatternPrefix, betPatternValue),
                FormatMeterIdWithPrefixLabel(filteredCreditPerPatternPrefix, creditPerPatternValue));

            // Increments a performance meter in predefined group.
            IncrementGamesPlayedWithBetDefinition(betDefinition);
        }

        /// <inheritdoc />
        public void IncrementGamesPlayedWithProgressiveHit(string progressiveLevelId)
        {
            // Increments a performance meter in predefined group.
            IncrementPerformanceMeter(GroupIdGamesPlayedWithProgressiveHit, progressiveLevelId);
        }

        /// <inheritdoc />
        public void IncrementGamesPlayedWithProgressiveHit(long progressiveLevel)
        {
            // Increments a performance meter in predefined group.
            IncrementPerformanceMeter(GroupIdGamesPlayedWithProgressiveHit,
                                FormatMeterIdWithPrefixLabel("Progressive", progressiveLevel));
        }

        /// <inheritdoc />
        public void IncrementMiscellaneousPerformanceMeter(string meterId)
        {
            // Increments a performance meter in predefined group.
            IncrementPerformanceMeter(GroupIdMiscellaneousPerformanceMeters, meterId);
        }

        /// <inheritdoc />
        public void IncrementPerformanceMeter(string groupId, string meterId)
        {
            IGameMeterGroup gameMeterGroup;
            if(MeterGroups.ContainsKey(groupId))
            {
                gameMeterGroup = MeterGroups[groupId];
            }
            else
            {
                // Create a new meter group if not exist.
                gameMeterGroup = new GameMeterGroup(criticalDataInterface,
                                                    CriticalDataScope,
                                                    CalculateCriticalDataPathOfGroup(groupId));

                // Add the group ID to the list.
                MeterGroups.Add(groupId, gameMeterGroup);

                // Save the new list of meter group IDs to the critical data.
                criticalDataInterface.WriteCriticalData(CriticalDataScope,
                                                        CriticalDataPathOfMeterGroupIds,
                                                        meterGroups.Keys.ToList());
            }

            gameMeterGroup.IncrementMeter(meterId);
        }

        #endregion

        #region IGamePerformanceMeterRead Implementation

        /// <inheritdoc />
        public long GetMeterInGroup(string groupId, string meterId)
        {
            return MeterGroups.ContainsKey(groupId) ? MeterGroups[groupId].GetMeter(meterId) : 0;
        }

        /// <inheritdoc />
        public IList<GamePerformanceMeterType> GetAllMetersInGroup(string groupId)
        {
            return !MeterGroups.ContainsKey(groupId)
                    ? null
                    : MeterGroups[groupId].GetAllMeters()
                                          .Select(meter => new GamePerformanceMeterType
                                              {
                                                  Name = meter.Key,
                                                  Value = meter.Value
                                              })
                                          .ToList();
        }

        /// <inheritdoc />
        public IDictionary<string, IList<GamePerformanceMeterType>> GetAllMetersInAllGroups()
        {
            return MeterGroups.Keys.Select(id => new { key = id, value = GetAllMetersInGroup(id) })
                                   .ToDictionary(item => item.key, item => item.value);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllGroupIds()
        {
            return MeterGroups.Keys;
        }

        /// <inheritdoc />
        public IList<GamePerformanceMeterType> GetGamesPlayed()
        {
            // Retrieve all meters from a predefined group.
            return GetAllMetersInGroup(GroupIdGamesPlayedCount);
        }

        /// <inheritdoc />
        public IList<GamePerformanceMeterType> GetGamesPlayedWithProgressiveHit()
        {
            // Retrieve all meters from a predefined group.
            return GetAllMetersInGroup(GroupIdGamesPlayedWithProgressiveHit);
        }

        /// <inheritdoc />
        public IList<GamePerformanceMeterType> GetMiscellaneousPerformanceMeters()
        {
            // Retrieve all meters from a predefined group.
            return GetAllMetersInGroup(GroupIdMiscellaneousPerformanceMeters);
        }

        /// <inheritdoc />
        public IList<GamePerformanceMeterType> GetGamesPlayedWithBetDefinition()
        {
            // Retrieve all meters from a predefined group.
            return GetAllMetersInGroup(GroupIdGamesPlayedWithBetDefinition);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates and gets the critical data path of a meter group.
        /// </summary>
        /// <param name="groupId">The group ID to get.</param>
        /// <returns>The critical data path for the specified meter group.</returns>
        private string CalculateCriticalDataPathOfGroup(string groupId)
        {
            return GamePerformanceDataPrefix + "/D" + denom + "/" + groupId;
        }

        #endregion
    }
}
