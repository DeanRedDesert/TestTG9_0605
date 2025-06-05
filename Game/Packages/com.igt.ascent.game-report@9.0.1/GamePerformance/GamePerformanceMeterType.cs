// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceMeterType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    /// <summary>
    /// This class contains the basic information of a game performance meter.
    /// </summary>
    public class GamePerformanceMeterType
    {
        /// <summary>
        /// The meter name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The meter value.
        /// </summary>
        public long Value { get; set; }
    }
}
