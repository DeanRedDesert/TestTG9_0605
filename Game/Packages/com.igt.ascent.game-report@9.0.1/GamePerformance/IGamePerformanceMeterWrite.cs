// -----------------------------------------------------------------------
// <copyright file = "IGamePerformanceMeterWrite.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    /// <summary>
    /// The interface to allow updating the game performance meters in the critical data.
    /// </summary>
    public interface IGamePerformanceMeterWrite
    {
        /// <summary>
        /// Generates a 2-dimension meter ID by combining two labels.
        /// This is useful for meters identified by two different attributes,
        /// such as NumberOfPlaylines + BetPerLine for a bet definition.
        /// </summary>
        /// <param name="label1">The first dimensional label.</param>
        /// <param name="label2">The second dimensional label.</param>
        /// <returns>The formatted meter ID.</returns>
        /// <example>
        /// This example shows how to combine the label "NumOfLines_5" and "BetPerLine_2" to
        /// get a bet definition string by calling this method:
        /// <code>
        /// var betDefinitionStr = FormatMeterIdWith2DLabel("NumOfLines_5", "BetPerLine_2");
        /// </code>
        /// </example>
        string FormatMeterIdWith2DLabel(string label1, string label2);

        /// <summary>
        /// Generates a meter Id by combining a label with a value.
        /// This is useful for meters identified by the changing values of a variable,
        /// such as games played with progressive hit level of 1, 2 and 3.
        /// </summary>
        /// <param name="prefix">The prefix label.</param>
        /// <param name="value">The value to format with the label.</param>
        /// <returns>The formatted meter ID.</returns>
        /// <example>
        /// This example shows how to combine the label "progressive" and the level value 3
        /// to get a progressive hit string by calling this method:
        /// <code>
        /// var progressiveId = FormatMeterIdWithPrefixLabel("progressive", 3);
        /// </code>
        /// </example>
        string FormatMeterIdWithPrefixLabel(string prefix, long value);

        /// <summary>
        /// Increments the specified meter in the predefined group of games played meters.
        /// </summary>
        /// <param name="meterId">
        /// The string representing the meter ID of a games played meter, for example,
        /// "GamesPlayed", or "GamesPlayedWithSidebet". If the meter doesn't exist, it
        /// shall be created automatically.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="meterId"/> must start with alphabetic [a-zA-Z] and the
        /// character set for the <paramref name="meterId"/> is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the characters '/',
        /// '.', '_', and '-'.
        /// </para>
        /// </remarks>
        void IncrementGamesPlayed(string meterId);

        /// <summary>
        /// Increments the specified meter in the predefined group of games played with certain
        /// bet definition.
        /// </summary>
        /// <param name="betDefinition">
        /// The string representing the meter ID of a bet definition meter, for example,
        /// "Lines_5.BetPerLine_4". If the meter doesn't exist, it shall be created automatically.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="betDefinition"/> must start with alphabetic [a-zA-Z] and the
        /// character set for the <paramref name="betDefinition"/> is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the characters '/',
        /// '.', '_', and '-'.
        /// </para>
        /// </remarks>
        void IncrementGamesPlayedWithBetDefinition(string betDefinition);

        /// <summary>
        /// Increments the specified meter in the predefined group of games played with certain
        /// bet definition.
        /// </summary>
        /// <param name="betPatternPrefix">
        /// The prefix of bet pattern, for example, "NumOfLines".
        /// </param>
        /// <param name="betPatternValue">
        /// The value of bet pattern, for example, 9 - means number of lines 9.
        /// </param>
        /// <param name="creditPerPatternPrefix">
        /// The prefix of bet credit per pattern, for example, "BetPerLine".
        /// </param>
        /// <param name="creditPerPatternValue">
        /// The bet value per each pattern, for example, 10 - means bet 10 credit per line.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="betPatternPrefix"/> and the <paramref name="creditPerPatternPrefix"/>
        /// must consist of alphabetic letter [a-zA-Z] only, any other characters will be removed.
        /// </para>
        /// </remarks>
        void IncrementGamesPlayedWithBetDefinition(
                            string betPatternPrefix,
                            long betPatternValue,
                            string creditPerPatternPrefix,
                            long creditPerPatternValue);

        /// <summary>
        /// Increments the specified meter in the predefined group of games played with
        /// progressive hit.
        /// </summary>
        /// <param name="progressiveLevelId">
        /// The string representing the meter ID of a progressive hit meter, for example,
        /// "Progressive_1", "Progressive_2". If the meter doesn't exist, it shall be created
        /// automatically.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="progressiveLevelId"/> must start with alphabetic [a-zA-Z] and the
        /// character set for the <paramref name="progressiveLevelId"/> is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the characters '/',
        /// '.', '_', and '-'.
        /// </para>
        /// </remarks>
        void IncrementGamesPlayedWithProgressiveHit(string progressiveLevelId);

        /// <summary>
        /// Increments the specified meter in the group of games played with progressive hit.
        /// </summary>
        /// <param name="progressiveLevel">
        /// The specified progressive level that has been hit. If the meter doesn't exist, it
        /// shall be created automatically.
        /// </param>
        /// <remarks>
        /// This method can be called only from within a method that provides an open transaction.
        /// </remarks>
        void IncrementGamesPlayedWithProgressiveHit(long progressiveLevel);

        /// <summary>
        /// Increments the specified meter in the predefined group of games miscellaneous
        /// performance meters.
        /// </summary>
        /// <param name="meterId">
        /// The string representing the meter ID of a miscellaneous performance meter,
        /// for example, "PickBonus". If the meter doesn't exist, it shall be created
        /// automatically.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="meterId"/> must start with alphabetic [a-zA-Z] and the
        /// character set for the <paramref name="meterId"/> is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the characters '/',
        /// '.', '_', and '-'.
        /// </para>
        /// </remarks>
        void IncrementMiscellaneousPerformanceMeter(string meterId);

        /// <summary>
        /// Increments a specified meter in a custom group.
        /// </summary>
        /// <param name="groupId">
        /// The string representing a custom group of meters. If the group doesn't exist, it
        /// shall be created automatically.
        /// </param>
        /// <param name="meterId">
        /// The string representing as the ID of a meter in the specified group. If the meter
        /// doesn't exist, it shall be created automatically.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method can be called only from within a method that provides an open transaction.
        /// </para>
        /// <para>
        /// The <paramref name="groupId"/> and <paramref name="meterId"/> must start with
        /// alphabetic [a-zA-Z] and the character set for the <paramref name="groupId"/> and
        /// <paramref name="meterId"/> is limited to a subset of ASCII characters that include
        /// numeric, alphabetic and the characters '/', '.', '_', and '-'.
        /// </para>
        /// </remarks>
        void IncrementPerformanceMeter(string groupId, string meterId);
    }
}
