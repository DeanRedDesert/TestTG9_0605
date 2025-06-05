// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceServiceHandlerBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using GamePerformance;

    /// <summary>
    /// The implementation of <see cref="IGamePerformanceServiceHandler"/>.
    /// </summary>
    public class GamePerformanceServiceHandlerBase : IGamePerformanceServiceHandler
    {
        #region Private Fields

        /// <summary>
        /// Cache the interface of report lib.
        /// </summary>
        protected readonly IReportLib CachedReportLib;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the service handler with <paramref name="reportLib"/>.
        /// </summary>
        /// <param name="reportLib">
        /// The game report interface to the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reportLib"/> is null.
        /// </exception>
        public GamePerformanceServiceHandlerBase(IReportLib reportLib)
        {
            CachedReportLib = reportLib ?? throw new ArgumentNullException(nameof(reportLib));
        }

        #endregion

        #region IGamePerformanceServiceHandler Implementation

        /// <inheritdoc/>
        public GamePerformanceReportType GenerateGamePerformanceReport(
                                            GamePerformanceReportContext gamePerformanceReportContext)
        {
            return new GamePerformanceReportType
            {
                GamePerformancePerDenomList = (
                    from denom in CachedReportLib.GameInformation.GetSupportedDenominations(
                                            gamePerformanceReportContext.PaytableIdentifier)
                    let meterReader = new GamePerformanceMeterManager(
                                            CachedReportLib.CriticalDataAccessor,
                                            gamePerformanceReportContext.PaytableIdentifier,
                                            denom) as IGamePerformanceMeterRead
                    let meterGroupList = meterReader
                                            .GetAllMetersInAllGroups()
                                            .Where(meterGroup => meterGroup.Value.Any())
                                            .Select(meterGroup => new GamePerformanceMeterGroupType
                                                {
                                                    Name = meterGroup.Key,
                                                    MeterList = meterGroup.Value.OrderBy(meter => meter.Name).ToList()
                                                })
                                            .OrderBy(meterGroup => meterGroup.Name)
                    where meterGroupList.Any()
                    select new GamePerformanceReportPerDenomType
                    {
                        Denom = denom,
                        MeterGroupList = meterGroupList.ToList(),
                    }).ToList()
            };
        }

        #endregion

        #region IReportResourceCleanUp Implementation

        /// <inheritdoc />
        public virtual void CleanUpResources(IReportLib reportLib)
        {
        }

        #endregion
    }
}
