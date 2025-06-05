//-----------------------------------------------------------------------
// <copyright file = "IGameLogicAutomationClient.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines API for a client intended to connect to a Game Logic Automation
    /// Service.
    /// </summary>
    public interface IGameLogicAutomationClient
    {
        /// <summary>
        /// Gets a bool indicating if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }

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
        /// Requests the filename for the currently active pay table.
        /// </summary>
        void RequestPaytableFilename();

        /// <summary>
        /// Request to setup a <see cref="RNGSeederValueProvider"/> to allow for seeding of the RNG values.
        /// The request responds with a <see cref="GameLogicAutomationSendConnectionMsg"/>.
        /// </summary>
        /// <remarks>This function must be called first in order for the RNG values to be seeded.</remarks>
        void RequestSetupPrepickedProvider();

        /// <summary>
        /// Request the current version of the SDK. 
        /// </summary>
        void RequestSdkVersion();

        /// <summary>
        /// Sends a list of numbers to be used to seed the RNG.
        /// </summary>
        /// <param name="numbers">The list of numbers to send.</param>
        void SendRandomNumbers(IEnumerable<int> numbers);

        /// <summary>
        /// Request the logic data service names by provdier names and service identifiers.
        /// </summary>
        /// <param name="serviceNamesRequests">
        /// The dictionary contains provider names for Keys,
        /// and a list of service identifiers for Values.
        /// </param>
        void RequestLogicDataServiceNames(IDictionary<string, IList<int>> serviceNamesRequests);
    }
}
