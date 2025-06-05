// -----------------------------------------------------------------------
// <copyright file = "IGamePerformanceMeterRead.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface to allow reading the game performance meters from the critical data in group base.
    /// </summary>
    public interface IGamePerformanceMeterRead
    {
        /// <summary>
        /// Gets a specific meter in a specific meter group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="meterId">The meter ID.</param>
        /// <returns>
        /// The retrieved meter value in the specified meter group.
        /// A value of O is returned if the specific meter in the group does not exist.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        long GetMeterInGroup(string groupId, string meterId);

        /// <summary>
        /// Gets all meters in a specific meter group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns>A list of game performance meters retrieved.</returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IList<GamePerformanceMeterType> GetAllMetersInGroup(string groupId);

        /// <summary>
        /// Gets all meters in all the performance meter groups.
        /// </summary>
        /// <returns>The retrieved meters and values in all the performance meter groups.</returns>
        /// <returns>
        /// A dictionary contains the retrieved game performance meters for all the
        /// meter groups.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IDictionary<string, IList<GamePerformanceMeterType>> GetAllMetersInAllGroups();

        /// <summary>
        /// Gets all game performance meter group IDs.
        /// </summary>
        /// <returns>
        /// A collection of group IDs for all the existing game performance meter groups.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IEnumerable<string> GetAllGroupIds();

        /// <summary>
        /// Gets all meters in the predefined performance meter group that records games played count,
        /// such as total games played, games played with side bet, games played with extra credit, etc.
        /// </summary>
        /// <returns>
        /// A list of game performance meters retrieved.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IList<GamePerformanceMeterType> GetGamesPlayed();

        /// <summary>
        /// Gets all meters in the predefined performance meter group that records for games played
        /// with progressive hit.
        /// </summary>
        /// <returns>
        /// A list of game performance meters retrieved.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IList<GamePerformanceMeterType> GetGamesPlayedWithProgressiveHit();

        /// <summary>
        /// Gets all meters in the predefined performance meter group that records for games played
        /// with different bet definition.
        /// </summary>
        /// <returns>
        /// A list of game performance meters retrieved.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IList<GamePerformanceMeterType> GetGamesPlayedWithBetDefinition();

        /// <summary>
        /// Gets all meters in the predefined performance meter group that records game miscellaneous
        /// performance meters.
        /// </summary>
        /// <returns>
        /// A list of game performance meters retrieved.
        /// </returns>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        IList<GamePerformanceMeterType> GetMiscellaneousPerformanceMeters();
    }
}
