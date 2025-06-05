//-----------------------------------------------------------------------
// <copyright file = "IF2XInterfaceExtensionConfiguration.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System.Collections.Generic;
    using F2XTransport;

    /// <summary>
    /// Interface which represents an extended interface configuration compatible with the F2X.
    /// </summary>
    public interface IF2XInterfaceExtensionConfiguration : IInterfaceExtensionConfiguration
    {
        /// <summary>
        /// Gets a collection of category-version requests needed by the interface extension.
        /// </summary>
        /// <returns>
        /// All the supported versions for a specific category required by the interface extension.
        /// </returns>
        /// <remarks>
        /// Only one of the category-version requests will be used when creating the interface extension.
        /// </remarks>
        IEnumerable<CategoryRequest> GetCategoryRequests();

        /// <summary>
        /// Gets the negotiation level of the specific category required by the interface extension
        /// </summary>
        CategoryNegotiationLevel NegotiationLevel { get; }
    }
}
