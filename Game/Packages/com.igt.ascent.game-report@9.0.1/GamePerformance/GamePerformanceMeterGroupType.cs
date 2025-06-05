// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceMeterGroupType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Collections.Generic;

    /// <summary>
    /// This class contains the information of a game performance meter group.
    /// </summary>
    public class GamePerformanceMeterGroupType
    {
        /// <summary>
        /// The name of meter group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of meters within the meter group.
        /// </summary>
        public List<GamePerformanceMeterType> MeterList { get; set; } = new List<GamePerformanceMeterType>();
    }
}
