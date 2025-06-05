//-----------------------------------------------------------------------
// <copyright file = "ICategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    /// <summary>
    /// Interface for F2L schema generated categories to inherit from.
    /// Provides uniform access to versioning and messages.
    /// </summary>
    public interface ICategory
    {
        /// <summary>
        /// Get the message for the category.
        /// </summary>
        ICategoryMessage Message { get; }

        /// <summary>
        /// Gets/Sets the version associated with the category.
        /// </summary>
        IVersion Version { get; set; }
    }
}
