//-----------------------------------------------------------------------
// <copyright file = "FoundationTarget.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the bit positions for the <see cref="FoundationTarget"/> enum.
    /// Using this enumeration ensures that <see cref="FoundationTarget"/> values
    /// always use unique bits out of a contiguous range starting with bit 0 and
    /// proceeding in the order defined below.
    /// </summary>
    internal enum FoundationTargetBit
    {
        /// <summary>
        /// Defines the bit position for the FoundationTarget.UniversalController enum value.
        /// Universal Controllers precede the Ascent Foundation targets because they are special cases.
        /// </summary>
        UniversalControllerBit,

        /// <summary>Defines the bit position for the FoundationTarget.UniversalController2 enum value.</summary>
        UniversalController2Bit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentHSeriesCds enum value.</summary>
        AscentHSeriesCdsBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentISeriesCds enum value.</summary>
        AscentISeriesCdsBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentJSeriesMps enum value.</summary>
        AscentJSeriesMpsBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentKSeriesCds enum value.</summary>
        AscentKSeriesCdsBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentMSeries enum value.</summary>
        AscentMSeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentN01Series enum value.</summary>
        AscentN01SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentN01SeriesSb enum value.</summary>
        AscentN01SeriesSbBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentN03Series enum value.</summary>
        AscentN03SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentP1Dynasty enum value.</summary>
        AscentP1DynastyBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentQ1Series enum value.</summary>
        AscentQ1SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentQ2Series enum value.</summary>
        AscentQ2SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentQ3Series enum value.</summary>
        AscentQ3SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentR1Series enum value.</summary>
        AscentR1SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentR2Series enum value.</summary>
        AscentR2SeriesBit,

        /// <summary>Defines the bit position for the FoundationTarget.AscentS1Series enum value.</summary>
        AscentS1SeriesBit,

        /// <summary>
        /// Defines the bit position for the FoundationTarget.Next enum value.  This should always be the last entry.
        /// </summary>
        NextBit,
    }

    /// <summary>
    /// The foundation target of the SDK.
    /// Use <see cref="FoundationTargetBit"/> to ensure values are unique and contiguous,
    /// and to avoid having to change all definitions when one target is added or removed.
    /// </summary>
    [Flags]
    public enum FoundationTarget
    {
        /// <summary>
        /// The Universal Controller.  Universal controllers are special cases.
        /// </summary>
        UniversalController = 1 << FoundationTargetBit.UniversalControllerBit,

        /// <summary>
        /// The Universal Controller 2.
        /// </summary>
        UniversalController2 = 1 << FoundationTargetBit.UniversalController2Bit,

        /// <summary>
        /// The Ascent H Series CDS Foundation.
        /// </summary>
        AscentHSeriesCds = 1 << FoundationTargetBit.AscentHSeriesCdsBit,

        /// <summary>
        /// The Ascent I Series CDS Foundation.
        /// </summary>
        AscentISeriesCds = 1 << FoundationTargetBit.AscentISeriesCdsBit,

        /// <summary>
        /// The Ascent J Series MPS.
        /// </summary>
        AscentJSeriesMps = 1 << FoundationTargetBit.AscentJSeriesMpsBit,

        /// <summary>
        /// The Ascent K Series CDS Foundation.
        /// </summary>
        AscentKSeriesCds = 1 << FoundationTargetBit.AscentKSeriesCdsBit,

        /// <summary>
        /// The Ascent M Series.
        /// </summary>
        AscentMSeries = 1 << FoundationTargetBit.AscentMSeriesBit,

        /// <summary>
        /// The Ascent N01 Series.
        /// </summary>
        AscentN01Series = 1 << FoundationTargetBit.AscentN01SeriesBit,

        /// <summary>
        /// The Ascent N Series Sports Betting Foundation.
        /// </summary>
        AscentN01SeriesSb = 1 << FoundationTargetBit.AscentN01SeriesSbBit,

        /// <summary>
        /// The Ascent N03 Series.
        /// </summary>
        AscentN03Series = 1 << FoundationTargetBit.AscentN03SeriesBit,

        /// <summary>
        /// The Ascent P1 Dynasty Release, where Dynasty is the cabinet used for Electronic Table Games.
        /// </summary>
        AscentP1Dynasty = 1 << FoundationTargetBit.AscentP1DynastyBit,

        /// <summary>
        /// The Ascent Q1 Series.
        /// </summary>
        AscentQ1Series = 1 << FoundationTargetBit.AscentQ1SeriesBit,

        /// <summary>
        /// The Ascent Q2 Series.
        /// </summary>
        AscentQ2Series = 1 << FoundationTargetBit.AscentQ2SeriesBit,

        /// <summary>
        /// The Ascent Q3 Series.
        /// </summary>
        AscentQ3Series = 1 << FoundationTargetBit.AscentQ3SeriesBit,

        /// <summary>
        /// The Ascent R1 Series.
        /// </summary>
        AscentR1Series = 1 << FoundationTargetBit.AscentR1SeriesBit,

        /// <summary>
        /// The Ascent R2 Series.
        /// </summary>
        AscentR2Series = 1 << FoundationTargetBit.AscentR2SeriesBit,

        /// <summary>
        /// The Ascent S1 Series.
        /// </summary>
        AscentS1Series = 1 << FoundationTargetBit.AscentS1SeriesBit,

        /// <summary>
        /// The next unreleased version of the Ascent Foundation.
        /// </summary>
        Next = 1 << FoundationTargetBit.NextBit,

        /// <summary>
        /// All Foundation versions.
        /// </summary>
        All = Next | (Next - 1), // Prevents overflow when Next is the highest possible bit in the FoundationTarget.

        /// <summary>
        /// All Ascent Foundation versions.
        /// </summary>
        AllAscent = All ^ (UniversalController | UniversalController2),
    }

    /// <summary>
    /// An extension class for the FoundationTarget.
    /// </summary>
    public static class FoundationTargetExtensions
    {
        /// <summary>
        /// Gets a list of Ascent Foundation targets that are still experimental (i.e. have not yet officially released).
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static IList<FoundationTarget> GetExperimentalFoundationTargets =>
            new List<FoundationTarget>
                {
                    FoundationTarget.Next
                };

        /// <summary>
        /// Returns the first Ascent FoundationTarget.
        /// </summary>
        public static FoundationTarget FirstAscent()
        {
            return LowestTarget(FoundationTarget.AllAscent);
        }

        /// <summary>
        /// An extension method that excludes the specified foundation target(s) from the given foundation target.
        /// </summary>
        /// <param name="foundationTarget">The foundation target to exclude from.</param>
        /// <param name="excludedFoundationTarget">The foundation target(s) to exclude.</param>
        /// <returns>The excluded foundation target(s) from the given foundation target.</returns>
        public static FoundationTarget Exclude(this FoundationTarget foundationTarget,
            FoundationTarget excludedFoundationTarget)
        {
            return foundationTarget & ~excludedFoundationTarget;
        }

        /// <summary>
        /// An extension method used to determine if the foundation target is an equal or newer version than the
        /// base version. For instance if a feature is supported in version 2.6, then version 2.6 or greater is needed.
        /// </summary>
        /// <param name="foundationTarget">Foundation target to check.</param>
        /// <param name="baseVersion">The version to check against.</param>
        /// <returns>True if the version is equal or newer than the base version.</returns>
        public static bool IsEqualOrNewer(this FoundationTarget foundationTarget, FoundationTarget baseVersion)
        {
            // Return true if the lowest target in foundationTarget is
            // greater than or equal to the highest target in baseVersion.
            return (uint)LowestTarget(foundationTarget) > (uint)baseVersion >> 1;
        }

        /// <summary>
        /// An extension method that returns a mask including the given FoundationTarget and all lower FoundationTargets.
        /// </summary>
        /// <param name="maxTarget">The maximum FoundationTarget value to include in the mask.</param>
        /// <returns>A mask including the <paramref name="maxTarget"/> and all lower FoundationTargets.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="maxTarget"/> isn't a single FoundationTarget value.</exception>
        public static FoundationTarget AndLower(this FoundationTarget maxTarget)
        {
            // Ensure that maxTarget has one and only one bit set.
            if(!maxTarget.IsSingleTarget())
            {
                throw new ArgumentException("maxTarget must be a single FoundationTarget value");
            }

            return (FoundationTarget)((uint)maxTarget | ((uint)maxTarget - 1));
        }

        /// <summary>
        /// An extension method that returns a mask including the given FoundationTarget and all higher FoundationTargets.
        /// </summary>
        /// <param name="minTarget">The minimum FoundationTarget value to include in the mask.</param>
        /// <returns>A mask including the <paramref name="minTarget"/> and all higher FoundationTargets.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="minTarget"/> isn't a single FoundationTarget value.</exception>
        public static FoundationTarget AndHigher(this FoundationTarget minTarget)
        {
            // Ensure that minTarget has one and only one bit set.
            if(!minTarget.IsSingleTarget())
            {
                throw new ArgumentException("minTarget must be a single FoundationTarget value");
            }

            return (FoundationTarget)((uint)FoundationTarget.Next | ((uint)FoundationTarget.Next - (uint)minTarget));
        }

        /// <summary>
        /// An extension method that returns a mask including all foundations in the range given.
        /// </summary>
        /// <param name="minTarget">The minimum FoundationTarget value to include in the mask.</param>
        /// <param name="maxTarget">The maximum FoundationTarget value to include in the mask.</param>
        /// <returns>
        /// A mask including all foundations from <paramref name="minTarget"/> to <paramref name="maxTarget"/>, inclusive.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="minTarget"/> or <paramref name="maxTarget"/> isn't a single FoundationTarget value.
        /// Also thrown if <paramref name="minTarget"/> is greater than <paramref name="maxTarget"/>.
        /// </exception>
        public static FoundationTarget UpTo(this FoundationTarget minTarget, FoundationTarget maxTarget)
        {
            // Ensure that minTarget has one and only one bit set.
            if(!minTarget.IsSingleTarget())
            {
                throw new ArgumentException("minTarget must be a single FoundationTarget value");
            }

            // Ensure that maxTarget has one and only one bit set.
            if(!maxTarget.IsSingleTarget())
            {
                throw new ArgumentException("maxTarget must be a single FoundationTarget value");
            }

            if(minTarget > maxTarget)
            {
                throw new ArgumentException("minTarget must be less than or equal to maxTarget.");
            }

            return (FoundationTarget)((uint)maxTarget | ((uint)maxTarget - (uint)minTarget));
        }

        #region private methods

        /// <summary>
        /// Tell if the given value contains exactly one FoundationTarget.
        /// </summary>
        /// <param name="foundationTarget">The FoundationTarget to inspect.</param>
        /// <returns>
        /// True if <paramref name="foundationTarget"/> contains exactly one <see cref="FoundationTarget"/>, false otherwise.
        /// </returns>
        private static bool IsSingleTarget(this FoundationTarget foundationTarget)
        {
            return LowestTarget(foundationTarget) == foundationTarget && foundationTarget > 0;
        }

        /// <summary>
        /// Return the lowest bit set in the given value.
        /// </summary>
        /// <param name="foundationTarget">The value to return the lowest bit from.</param>
        /// <returns>The lowest bit set in <paramref name="foundationTarget"/>.</returns>
        private static FoundationTarget LowestTarget(this FoundationTarget foundationTarget)
        {
            return (FoundationTarget)((int)foundationTarget & -(int)foundationTarget);
        }

        #endregion private methods
    }
}
