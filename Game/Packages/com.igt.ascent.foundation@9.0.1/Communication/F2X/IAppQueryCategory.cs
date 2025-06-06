//-----------------------------------------------------------------------
// <copyright file = "IAppQueryCategory.cs" company = "IGT">
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
    using Schemas.Internal.AppTypes;
    using Schemas.Internal.PropertyTypes;

    /// <summary>
    /// App Query category of messages.
    /// Category: 129; Major Version: 2
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IAppQueryCategory
    {
        /// <summary>
        /// Get the list of allowed switch behaviors when switching from the current app to the specified app.
        /// </summary>
        /// <param name="target">
        /// Identifier of the app to be switched to.
        /// </param>
        /// <returns>
        /// The content of the GetAllowedSwitchBehaviorsReply message.
        /// </returns>
        AppSwitchBehaviorList GetAllowedSwitchBehaviors(AppIdentifier target);

        /// <summary>
        /// Get the property names for an app from the foundation.
        /// </summary>
        /// <param name="app">
        /// App to query.
        /// </param>
        /// <param name="section">
        /// Property section to query.
        /// </param>
        /// <returns>
        /// The content of the GetAppPropertyListReply message.
        /// </returns>
        PropertyNameList GetAppPropertyList(AppIdentifier app, string section);

        /// <summary>
        /// Get a list of property values for an app by their names from the foundation.
        /// </summary>
        /// <param name="app">
        /// App to query.
        /// </param>
        /// <param name="section">
        /// Property section to query.
        /// </param>
        /// <param name="properties">
        /// Names of the properties to get.
        /// </param>
        /// <returns>
        /// The content of the GetAppPropertyValuesReply message.
        /// </returns>
        PropertyList GetAppPropertyValues(AppIdentifier app, string section, PropertyNameList properties);

        /// <summary>
        /// Get a list of available apps and their info from the foundation.
        /// </summary>
        /// <returns>
        /// The content of the GetAvailableAppsReply message.
        /// </returns>
        AppInfoList GetAvailableApps();

        /// <summary>
        /// Get the current selected app configuration.
        /// </summary>
        /// <returns>
        /// The content of the GetSelectedAppReply message.
        /// </returns>
        AppSelector GetSelectedApp();

        /// <summary>
        /// Get a list of property values for an app configuration from the foundation.
        /// </summary>
        /// <param name="app">
        /// App configuration to query.
        /// </param>
        /// <param name="section">
        /// Property section to query.
        /// </param>
        /// <returns>
        /// The content of the GetSelectorPropertyListReply message.
        /// </returns>
        PropertyNameList GetSelectorPropertyList(AppSelector app, string section);

        /// <summary>
        /// Get a list of property values for an app configuration by their names from the foundation.
        /// </summary>
        /// <param name="app">
        /// App configuration to query.
        /// </param>
        /// <param name="section">
        /// Property section to query.
        /// </param>
        /// <param name="properties">
        /// Names of the properties to get.
        /// </param>
        /// <returns>
        /// The content of the GetSelectorPropertyValuesReply message.
        /// </returns>
        PropertyList GetSelectorPropertyValues(AppSelector app, string section, PropertyNameList properties);

    }

}

