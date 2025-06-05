//-----------------------------------------------------------------------
// <copyright file = "IFeatureAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Type used for awards associated with a game feature and payvar.
    /// </summary>
    /// <remarks>
    /// Used to declare payvar wins and progressive wins associated with a payvar win index.
    /// </remarks>
    public interface IFeatureAward : IAward
    {
        /// <summary>
        /// Gets the associated win level index of the payvar registry
        /// </summary>
        uint WinLevelIndex { get; }

        /// <summary>
        /// Gets a copy of the <see cref="IFeatureProgressiveAward"/> list associated with this 
        /// game feature win as a ReadOnlyCollection.  
        /// Modification will not update contents of original list.
        /// </summary>
        ReadOnlyCollection<IFeatureProgressiveAward> GetFeatureProgressiveAwards();

        /// <summary>
        /// Gets a copy of the <see cref="IProgressiveNearHit"/> list associated with this 
        /// game feature win as a ReadOnlyCollection.  
        /// Modification will not update contents of original list.
        /// </summary>
        ReadOnlyCollection<IProgressiveNearHit> GetProgressiveNearHits();
    }
}