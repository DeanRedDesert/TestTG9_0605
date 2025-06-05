// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceReportTypeConverters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Linq;

    /// <summary>
    /// This utility class contains static extension methods to convert game performance data between
    /// public and internal types.
    /// </summary>
    internal static class GamePerformanceReportTypeConverters
    {
        /// <summary>
        /// Converts an instance of the public <see cref="GamePerformanceReportType"/> class to
        /// a new instance of the internal <see cref="GamePerformanceReport"/> class.
        /// </summary>
        /// <param name="publicData">
        /// The instance of public <see cref="GamePerformanceReportType"/> class.
        /// </param>
        /// <returns>
        /// The instance of internal <see cref="GamePerformanceReport"/> class.
        /// </returns>
        public static GamePerformanceReport ToInternal(this GamePerformanceReportType publicData)
        {
            return publicData == null ? null : new GamePerformanceReport
            {
                GamePerformancePerDenom =
                    publicData.GamePerformancePerDenomList.Select(dataPerDenom => new GamePerformancePerDenom
                    {
                        Denom = dataPerDenom.Denom,
                        MeterGroup = dataPerDenom.MeterGroupList.Select(meterGroup => new MeterGroupType
                        {
                            Name = meterGroup.Name,
                            Meter = meterGroup.MeterList.Select(meter => new MeterValueType
                            {
                                Name = meter.Name,
                                Value = meter.Value.ToString("G"),
                            }).ToList(),
                        }).ToList(),
                    }).ToList(),
            };
        }
    }
}
