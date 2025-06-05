//-----------------------------------------------------------------------
// <copyright file = "IGameLogicAutomationService.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using Core.Logic.Evaluator.Schemas;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines API for a Game Logic Automation Service that interacts with
    /// the Game Logic.
    /// </summary>
    public interface IGameLogicAutomationService
    {
        /// <summary>
        /// Gets a bool indicating if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        /// Sends an error message to connected clients.
        /// </summary>
        /// <param name="errorType">Error type encountered.</param>
        /// <param name="errorDescription">
        /// Error string that provides additional information about the error encountered.
        /// </param>
        void SendErrorMessage(InterceptorError errorType, string errorDescription);

        /// <summary>
        /// Gets the next available Game Logic Automation Message received, returns
        /// null if there is not an available message.
        /// </summary>
        AutomationGenericMsg GetNextMessage();

        /// <summary>
        /// Event which is raised when a Game Logic Automation Message is received.
        /// </summary>
        event EventHandler MessageReceived;

        /// <summary>
        /// Sends a WinOutcome to connected clients.
        /// </summary>
        /// <param name="winOutcome">WinOutcome to send to connected clients.</param>
        /// <param name="state">State where winOutcome was generated.</param>
        /// <param name="description">Description of the winOutcome e.g. Free Spin Evaluation.</param>
        void SendWinOutcome(WinOutcome winOutcome, string state, string description);

        /// <summary>
        /// Sends a CellPopulationOutcome to connected clients.
        /// </summary>
        /// <param name="cellPopulationOutcome">CellPopulationOutcome to send to connected clients.</param>
        /// <param name="state">State where cellPopulationOutcome was generated.</param>
        /// <param name="description">Description of the cellPopulationOutcome e.g. Free Spin Symbol Window.</param>
        void SendCellPopulationOutcome(CellPopulationOutcome cellPopulationOutcome, string state, string description);

        /// <summary>
        /// Sends a pay table to the connected client.
        /// </summary>
        /// <param name="paytableFileName">The pay table file name.</param>
        void SendPaytable(string paytableFileName);

        /// <summary>
        /// Send the current SDK version.
        /// </summary>
        void SendSdkVersion();

        /// <summary>
        /// Sends a message of a successful connection to the connected client.
        /// </summary>
        void SendConnectionReceived();

        /// <summary>
        /// Sends a message of successful receive of the random number values to the connected client.
        /// </summary>
        void SendRandomNumbersReceived();

        /// <summary>
        /// Sends a message containing a dictionary with the names of providers for Keys,
        /// and a dictionary with identifiers and names of services as Values.
        /// </summary>
        /// <param name="serviceNames"> 
        /// The dictionary contains the names of providers for Keys,
        /// and a dictionary with identifiers and names of services as Values.
        /// </param>
        void SendLogicDataServiceNames(IDictionary<string, IDictionary<int, string>> serviceNames);
    }
}
