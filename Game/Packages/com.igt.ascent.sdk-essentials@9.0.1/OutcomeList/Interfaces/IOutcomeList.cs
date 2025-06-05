//-----------------------------------------------------------------------
// <copyright file = "IOutcomeList.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// This class defines outcome/award lists to communicate with the Foundation for evaluation and potential adjustment.
    /// </summary>
    public interface IOutcomeList
    {
        /// <summary>
        /// Gets a copy of the FeatureEntry list as a ReadOnlyCollection.  
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of FeatureEntry objects.</returns>
        ReadOnlyCollection<IFeatureEntry> GetFeatureEntries();

        /// <summary>
        /// Gets a copy of the GameCycleEntry list as a ReadOnlyCollection.  
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of GameCycleEntry objects.</returns>
        ReadOnlyCollection<IGameCycleEntry> GetGameCycleEntries();

        /// <summary>
        /// Calculates and returns the total amount of the displayable awards
        /// included in this outcome list.
        /// </summary>
        /// <returns>The total amount of the displayable awards.</returns>
        long GetTotalDisplayableAmount();
    }
}