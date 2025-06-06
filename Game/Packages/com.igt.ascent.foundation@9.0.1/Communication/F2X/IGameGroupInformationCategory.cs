//-----------------------------------------------------------------------
// <copyright file = "IGameGroupInformationCategory.cs" company = "IGT">
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
    using Schemas.Internal.GameGroupInformation;
    using Schemas.Internal.Types;

    /// <summary>
    /// Game Group category of messages. Category: 122, Version: 1
    /// This category is used to request game group specific data.
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IGameGroupInformationCategory
    {
        /// <summary>
        /// Message from Bin to Foundation on the FI channel requesting a list of payvar groups within a theme.
        /// </summary>
        /// <param name="theme">
        /// A theme whose payvar groups are being requested. If any invalid (unknown to the foundation or malformed)
        /// theme is sent in the request, the reply will contain an exception and no other results.
        /// Only game groups get reported. If no group exists, an empty list is reported.
        /// </param>
        /// <returns>
        /// The content of the GetPayvarGroupsForThemeReply message.
        /// </returns>
        IEnumerable<ThemePayvarGroups> GetPayvarGroupsForTheme(IEnumerable<ThemeIdentifier> theme);

    }

}

