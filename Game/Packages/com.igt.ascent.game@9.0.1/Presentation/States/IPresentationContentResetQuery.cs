// -----------------------------------------------------------------------
// <copyright file = "IPresentationContentResetQuery.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using Communication.Presentation.CommServices;

    /// <summary>
    /// The <see cref="IPresentationContentResetQuery"/> interface provides functions to query information
    /// on presentation content reset, such as whether it should be reset, whether it is being reset etc.
    /// </summary>
    public interface IPresentationContentResetQuery
    {
        /// <summary>
        /// Gets the flag indicating whether the content should be reset at the first opportunity.
        /// </summary>
        bool ShouldReset { get; }

        /// <summary>
        /// Flag indicating whether content is currently being reset.
        /// </summary>
        bool ResettingContent { get; }

        /// <summary>
        /// Event handler fired when the content has been reset.
        /// </summary>
        event EventHandler ContentReset;

        /// <summary>
        /// Checks if a given <see cref="PresentationTransition"/> triggers a presentation reset.
        /// </summary>
        /// <returns>
        /// True if <paramref name="presentationTransition"/> triggers a presentation reset, false otherwise.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        bool IsPresentationResetTransition(PresentationTransition presentationTransition);
    }
}