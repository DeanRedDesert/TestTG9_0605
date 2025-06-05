//-----------------------------------------------------------------------
// <copyright file = "IFeatureEntry.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Interface for OutcomeList FeatureEntries
    /// Contains outcome/awards created by a theme's feature.
    /// </summary>
    public interface IFeatureEntry
    {
        /// <summary>
        /// Gets a zero based index that identifies the phase or component of the game that
        /// determined the award (IE 0 for base game, 1 for pick bonus, 2 for free spin bonus, etc)
        /// </summary>
        uint FeatureIndex { get; }

        /// <summary>
        /// Gets a bool determining if this Feature Entry contains a collection of <see cref="IFeatureAward"/>.
        /// </summary>
        /// <returns>True if the award list is of <see cref="IFeatureAward"/> type. False otherwise.</returns>
        bool ContainsFeatureAwardOutcome { get; }

        /// <summary>
        /// Gets a bool determining if this Feature Entry contains a collection of <see cref="IAncillaryAward"/>.
        /// </summary>
        /// <returns>True if the award list is of <see cref="IAncillaryAward"/> type. False otherwise.</returns>
        bool ContainsAncillaryAwardOutcome { get; }

        /// <summary>
        /// Gets a bool determining if this Feature Entry contains a collection of <see cref="IRiskAward"/>.
        /// </summary>
        /// <returns>True if the award list is of <see cref="IRiskAward"/> type. False otherwise.</returns>
        bool ContainsRiskAwardOutcome { get; }

        /// <summary>
        /// Calculates and return the total amount of the displayable awards included in this feature entry.
        /// </summary>
        /// <returns>The total amount of the displayable awards.</returns>
        long GetTotalDisplayableAmount();

        /// <summary>
        /// Gets a copy of the random numbers that resulted in this feature award as a ReadOnlyCollection.  
        /// Modification will not update contents of original list.
        /// </summary>
        ReadOnlyCollection<int> GetFeatureRngNumbers();

        /// <summary>
        /// Gets a copy of the <see cref="IAncillaryAward"/>,
        /// <see cref="IFeatureAward"/>, or <see cref="IRiskAward"/> as a ReadOnlyCollection. 
        /// Modification will not update contents of original list.
        /// </summary>
        /// <returns>A ReadOnly list of Awards objects.</returns>
        ReadOnlyCollection<T> GetAwards<T>();
    }
}