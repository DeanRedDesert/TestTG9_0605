//-----------------------------------------------------------------------
// <copyright file = "ISystemApiControlCategoryCallbacks.cs" company = "IGT">
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
    using System;
    using System.Collections.Generic;
    using Schemas.Internal.SystemApiControl;
    using Schemas.Internal.Types;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="SystemApiControl"/> category.
    /// (F2E Only) System API Control category of messages.  Category: 3011  Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ISystemApiControlCategoryCallbacks
    {
        /// <summary>
        /// Message from the Foundation to the Bin on the FI channel to request that the recipient return a list of all
        /// API categories and their versions in the order of preference, specific to the System-Extension context.
        /// </summary>
        /// <param name="extensions">
        /// List of extensions to support.
        /// </param>
        /// <param name="callbackResult">
        /// [Out] The content of the GetSystemApiVersionsReply message.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessGetSystemApiVersions(IEnumerable<Extension> extensions, out GetSystemApiVersionsReplyContentCategoryVersions callbackResult);

        /// <summary>
        /// Message from the Foundation to the Bin on the FI channel to set the API categories and versions the bin is
        /// to support for the System-Extension context.
        /// </summary>
        /// <param name="categoryVersions">
        /// The list of categories and their versions.
        /// </param>
        /// <param name="callbackResult">
        /// [Out] The content of the SetSystemApiVersionsReply message.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessSetSystemApiVersions(SetSystemApiVersionsSendCategoryVersions categoryVersions, out bool callbackResult);

    }

}

