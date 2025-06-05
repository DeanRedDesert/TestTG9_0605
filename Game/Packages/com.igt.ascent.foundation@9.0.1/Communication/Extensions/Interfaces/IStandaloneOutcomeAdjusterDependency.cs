//-----------------------------------------------------------------------
// <copyright file = "IStandaloneOutcomeAdjusterDependency.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using Ascent.OutcomeList;

    /// <summary>
    /// An interface used to extend the outcome adjustment routine.
    /// </summary>
    public interface IStandaloneOutcomeAdjusterDependency
    {
        /// <summary>
        /// Registers an adjustment to be applied whenever an outcome is adjusted.
        /// </summary>
        /// <param name="outcomeAdjustment">A delegate that applies an adjustment to an outcome list.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="outcomeAdjustment"/> is null.
        /// </exception>
        void RegisterAdjustment(Action<OutcomeList, OutcomeList, bool> outcomeAdjustment);

        /// <summary>
        /// Unregisters an adjustment.
        /// </summary>
        /// <param name="outcomeAdjustment">A delegate that applies an adjustment to an outcome list.</param>
        /// <remarks>
        /// If the lifetime of your object is shorter than that of the 
        /// <see cref="IStandaloneOutcomeAdjusterDependency"/> then you'll need to call this method before
        /// your object can be reclaimed by the garbage collector. Otherwise the registered outcome adjustment
        /// delegate will keep your object alive.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="outcomeAdjustment"/> is null.
        /// </exception>
        void UnregisterAdjustment(Action<OutcomeList, OutcomeList, bool> outcomeAdjustment);
    }
}
