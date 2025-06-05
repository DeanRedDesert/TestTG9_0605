//-----------------------------------------------------------------------
// <copyright file = "IPresentationAutomationClient.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Defines API for a client intended to connect to a Presentation Automation
    /// Service.
    /// </summary>
    public interface IPresentationAutomationClient
    {
        /// <summary>
        /// Gets a bool indicating if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        /// Gets the next available Presentation Automation Message received, returns
        /// null if there is not an available message.
        /// </summary>
        AutomationGenericMsg GetNextMessage();

        /// <summary>
        /// Event which is raised when a Presentation Automation Message is received.
        /// </summary>
        event EventHandler MessageReceived;

        /// <summary>
        /// Send request to the connected service for all currently active
        /// buttons.
        /// </summary>
        void RequestActiveButtons();

        /// <summary>
        /// Send Request to enable or disable the FPS reporting feature of the system.
        /// </summary>
        /// <param name="enabled">Enable or disable the FPS reporting through GL2P</param>
        void RequestFpsEnable(bool enabled);

        /// <summary>
        /// Send request to the connected service to simulate a button press.
        /// </summary>
        /// <param name="simulatedButtonName">Button name to simulate being pressed.</param>
        void SimulateButtonPush(string simulatedButtonName);

        /// <summary>
        /// Send request to the connected service for an active screen shot.
        /// </summary>
        /// <param name="requestedMonitor">The monitor to take a screenshot of.</param>
        void RequestScreenShotPng(Monitor requestedMonitor);

        /// <summary>
        /// Send request to the connected service for the current monitor configuration.
        /// </summary>
        void RequestMonitorConfiguration();
    }
}
