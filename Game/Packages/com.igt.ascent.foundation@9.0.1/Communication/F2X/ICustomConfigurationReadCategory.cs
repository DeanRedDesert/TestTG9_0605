//-----------------------------------------------------------------------
// <copyright file = "ICustomConfigurationReadCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System.Collections.Generic;
    using Schemas.Internal.CustomConfigurationRead;

    /// <summary>
    /// Category of messages.  Category: 107   Version: 1. This category is used to request information about custom
    /// configuration items.  None of the messages in this category can modify custom configuration items.
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ICustomConfigurationReadCategory
    {
        /// <summary>
        /// Sent from a Bin to the foundation on the FI channel.  A request for the enumeration lists in which the given
        /// custom configuration items reference.
        /// </summary>
        /// <param name="customConfigItemSelection">
        /// The custom configuration item selector to a configuration item that references an enumeration list.  More
        /// than one referenced enumeration list can be requested.
        /// </param>
        /// <returns>
        /// There will be an element for each custom configuration item selector that was sent in the request.
        /// </returns>
        IEnumerable<CustomConfigItemReferencedEnumeration> GetCustomConfigItemReferencedEnumerations(IEnumerable<CustomConfigurationItemSelector> customConfigItemSelection);

        /// <summary>
        /// Sent from a Bin to the foundation on the FI channel.  A request for the types of a custom configuration
        /// items.
        /// </summary>
        /// <param name="customConfigItemSelection">
        /// Contains custom configuration items selector for which the type is being requested.  More than one custom
        /// configuration item type can be requested.
        /// </param>
        /// <returns>
        /// There will be an element for each custom configuration item selector that was sent in the request.
        /// </returns>
        IEnumerable<GetCustomConfigItemTypesReplyResult> GetCustomConfigItemTypes(IEnumerable<CustomConfigurationItemSelector> customConfigItemSelection);

        /// <summary>
        /// Sent from a Bin to foundation on the FI channel.  A request for custom configuration item values.
        /// </summary>
        /// <param name="customConfigItemSelection">
        /// Contains the custom configuration item selector and expected type for the configuration item data being
        /// requested.  More than one custom configuration item can be requested in the message.
        /// </param>
        /// <returns>
        /// There will be an element for each custom configuration item selector that was sent in the request.
        /// </returns>
        IEnumerable<GetCustomConfigItemValueDataReplyResult> GetCustomConfigItemValueData(IEnumerable<GetCustomConfigItemValueDataSendSelector> customConfigItemSelection);

        /// <summary>
        /// Sent from a Bin to the foundation on the FI channel. Requests all the custom configuration item names and
        /// types within a payvar, extension or theme scope.
        /// </summary>
        /// <param name="customConfigItemScopeSelection">
        /// Contains the scope of the request for which the custom config item names and types are being requested.
        /// </param>
        /// <returns>
        /// Contains a name and type for a configuration item in the requested scope.  This element will not be present
        /// when there are no custom configuration items in the scope.
        /// </returns>
        IEnumerable<CustomConfigItemNameAndType> GetScopedCustomConfigItemNames(CustomConfigurationItemScopeSelector customConfigItemScopeSelection);

    }

}

