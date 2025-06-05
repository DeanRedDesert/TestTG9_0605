//-----------------------------------------------------------------------
// <copyright file = "IGameCycleEntry.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Interface for OutcomeList GameCycleEntries
    /// Contains awards that are NOT associated with a specific Feature or payvar's win level index. 
    /// May be used for win capping or external bonusing, including mystery progressives.
    /// </summary>
    public interface IGameCycleEntry
    {
        /// <summary>
        /// Calculate and return the total amount of the displayable awards
        /// included in this class.
        /// </summary>
        /// <returns>The total amount of the displayable awards.</returns>
        long GetTotalDisplayableAmount();

        /// <summary>
        /// Gets a copy of the <see cref="IAward"/> list as a ReadOnlyCollection. 
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of <see cref="IAward"/> objects.</returns>
        ReadOnlyCollection<IAward> GetAwards();

        /// <summary>
        /// Gets a copy of the <see cref="IProgressiveAward"/> list as a ReadOnlyCollection. 
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of <see cref="IProgressiveAward"/> objects.</returns>
        ReadOnlyCollection<IProgressiveAward> GetProgressiveAwards();

        /// <summary>
        /// Gets a copy of the <see cref="IBonusExtensionAward"/> list as a ReadOnlyCollection. 
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of <see cref="IBonusExtensionAward"/> objects.</returns>
        ReadOnlyCollection<IBonusExtensionAward> GetBonusExtensionAwards();
    }
}