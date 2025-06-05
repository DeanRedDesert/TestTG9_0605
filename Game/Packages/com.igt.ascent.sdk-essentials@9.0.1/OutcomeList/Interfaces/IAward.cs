//-----------------------------------------------------------------------
// <copyright file = "IAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Used as a base type for other types of extended awards such as <see cref="IFeatureAward"/>
    /// and <see cref="IProgressiveAward"/>
    /// </summary>
    public interface IAward : IOutcome
    {
        /// <summary>
        /// Gets the associated amount in base units.
        /// </summary>
        long AmountValue { get; }

        /// <summary>
        /// Gets a bool determining of this award is displayable to the user.
        /// </summary>
        /// <remarks>
        /// The Foundation may change this value to false during outcome evaluation. 
        /// If false then the Bin should indicate a generic "Win", but without specifying a win amount or prize. 
        /// This applies to all amounts and prize strings of all Award fields. 
        /// If false, the amount value and the amount values of all Award fields should 
        /// not be included in any totaling win displayed to the player
        /// </remarks>
        bool IsDisplayable { get; }

        /// <summary>
        /// Gets the reason for the award displayed to the player.
        /// </summary>
        /// <remarks>
        /// The Bin is to display this string to the player to indicate the reason for the award. 
        /// This attribute is set by the original creator of the element. 
        /// Suggested uses including external bonus won, or win-capping by Foundation. 
        /// The string is to be displayed regardless of the value of IsDisplayable
        /// </remarks>
        string DisplayableReason { get; }

        /// <summary>
        /// Gets the displayable string indicating the prize won
        /// </summary>
        string PrizeString { get; }

        /// <summary>
        /// Gets the bool determining if this is the highest award.
        /// </summary>
        bool IsTopAward { get; }

        /// <summary>
        /// Calculate and return the total award amount of the outcome list.
        /// </summary>
        /// <returns>The total award amount of the outcome list.</returns>
        long GetDisplayableAmount();
    }
}