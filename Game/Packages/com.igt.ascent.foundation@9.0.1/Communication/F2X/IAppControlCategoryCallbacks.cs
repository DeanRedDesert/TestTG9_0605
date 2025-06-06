//-----------------------------------------------------------------------
// <copyright file = "IAppControlCategoryCallbacks.cs" company = "IGT">
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
    using Schemas.Internal.AppControl;
    using Schemas.Internal.AppTypes;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="AppControl"/> category.
    /// App Control category of messages.
    /// Category: 2000; Major Version: 2
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IAppControlCategoryCallbacks
    {
        /// <summary>
        /// Message to the client requesting it display a specified app configuration.
        /// </summary>
        /// <param name="app">
        /// The requested app configuration to select.
        /// </param>
        /// <param name="switchBehavior">
        /// Desired behavior for the switch.
        /// </param>
        /// <param name="required">
        /// Flag indicating the requested app must be displayed before reply.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessAppRequested(AppSelector app, AppSwitchBehavior switchBehavior, bool required);

        /// <summary>
        /// Sent when the selected app's state changes.
        /// </summary>
        /// <param name="state">
        /// State of the selected app.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessAppStateChanged(AppState state);

        /// <summary>
        /// Indicates the Chooser has been requested.
        /// </summary>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessChooserRequested();

        /// <summary>
        /// Message to the client indicating the context's display state has changed.
        /// </summary>
        /// <param name="displayState">
        /// The Display Control state the user context should be set to.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessDisplayStateChanged(DisplayControlState displayState);

    }

}

