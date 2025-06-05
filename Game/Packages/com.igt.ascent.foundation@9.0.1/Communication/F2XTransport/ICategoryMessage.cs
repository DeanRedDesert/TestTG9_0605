//-----------------------------------------------------------------------
// <copyright file = "ICategoryMessage.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    /// <summary>
    /// Interface for category messages to inherit from. This adds a common interface to the generated code.
    /// </summary>
    public interface ICategoryMessage
    {
        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        object Item { get; set; }
    }
}
