//-----------------------------------------------------------------------
// <copyright file = "IConfigurationModeMonitoringCategoryCallbacks.cs" company = "IGT">
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
    using Schemas.Internal.ConfigurationModeMonitoring;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="ConfigurationModeMonitoring"/> category.
    /// This cateogry provides messages for system level clients to monitor foundation's configuration mode.
    /// Category: 139; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IConfigurationModeMonitoringCategoryCallbacks
    {
        /// <summary>
        /// Message from the foundation to bin notifying clients of configuration mode state.  B2F: FI channel.
        /// </summary>
        /// <param name="configModeState">
        /// State of configuration mode.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessConfigModeStateChanged(ConfigModeState configModeState);

    }

}

