// -----------------------------------------------------------------------
// <copyright file = "IGameMeterGroup.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface class defines APIs for accessing the game meter in the meter group.
    /// </summary>
    internal interface IGameMeterGroup
    {
        /// <summary>
        /// Increments a specific meter by 1 in the group.
        /// </summary>
        /// <param name="meterId">The meter ID  in the group to increment.</param>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        void IncrementMeter(string meterId);

        /// <summary>
        /// Increments multiple meters by 1 in the group.
        /// </summary>
        /// <param name="meterIds">The list of meter IDs in the group to increment.</param>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        void IncrementMeters(IEnumerable<string> meterIds);

        /// <summary>
        /// Increments all the meters by 1 in the group.
        /// </summary>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        void IncrementAllMeters();

        /// <summary>
        /// Gets a specific meter value in the group.
        /// </summary>
        /// <param name="meterId">The meter ID.</param>
        /// <returns>The meter value.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        long GetMeter(string meterId);

        /// <summary>
        /// Gets the values of multiple meters in the group.
        /// </summary>
        /// <param name="meterIds">The list of meter IDs.</param>
        /// <returns>A dictionary containing the list of meter IDs and meter values.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IDictionary<string, long> GetMeters(IEnumerable<string> meterIds);

        /// <summary>
        /// Gets the values of all the meters in the group.
        /// </summary>
        /// <returns>A dictionary containing the list of meter IDs and meter values.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IDictionary<string, long> GetAllMeters();
    }
}
