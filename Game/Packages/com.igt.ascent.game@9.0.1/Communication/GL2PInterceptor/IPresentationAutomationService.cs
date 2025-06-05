//-----------------------------------------------------------------------
// <copyright file = "IPresentationAutomationService.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines API for a Presentation Automation Service that interacts with
    /// the Presentation.
    /// </summary>
    public interface IPresentationAutomationService
    {
        /// <summary>
        /// Gets a bool indicating if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }
        
        /// <summary>
        /// Event which is raised when a Presentation Automation Message is received.
        /// </summary>
        event EventHandler MessageReceived;

        /// <summary>
        /// Gets the next available Presentation Automation Message received, returns
        /// null if there is not an available message.
        /// </summary>
        AutomationGenericMsg GetNextMessage();

        /// <summary>
        /// Sends a list of active button names.
        /// </summary>
        /// <param name="activeButtonList">List of active button names.</param>
        void SendActiveButtons(IList<string> activeButtonList);

        /// <summary>
        /// Sends the performance metric and state information.
        /// </summary>
        /// <param name="performanceMetric">The raw counter value.</param>
        /// <param name="state">Sends the game state along with the performance metric.</param>
        void SendPerformanceData(float performanceMetric, string state);

        /// <summary>
        /// Sends the performance metric and state information.
        /// </summary>
        /// <param name="performanceMetric">The raw counter value.</param>
        void SendPerformanceData(float performanceMetric);

        /// <summary>
        /// Sends an error message to connected clients.
        /// </summary>
        /// <param name="errorType">Error type encountered.</param>
        /// <param name="errorDescription">
        /// Error string that provides additional information about the error encountered.
        /// </param>
        void SendErrorMessage(InterceptorError errorType, string errorDescription);

        /// <summary>
        /// Send an active screen shot to connected clients.
        /// </summary>
        /// <param name="pngFile">Byte buffer containing png file contents.</param>
        /// <param name="monitor">The monitor <paramref name="pngFile"/> represents.</param>
        void SendScreenShotPng(byte[] pngFile, Monitor monitor);

        /// <summary>
        /// Send the monitor configuration.
        /// </summary>
        /// <param name="monitors">The list of currently connected monitors.</param>
        void SendMonitorConfiguration(IList<Monitor> monitors);
    }
}
