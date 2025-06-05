// -----------------------------------------------------------------------
// <copyright file = "ICommPortal.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;

    /// <summary>
    /// ICommPortal interface.
    /// </summary>
    public interface ICommPortal
    {
        /// <summary>
        /// Eventhandler for AutomationCommandArgs.
        /// </summary>
        event EventHandler<AutomationCommandArgs> AutomationCommandReceived;

        /// <summary>
        /// CreateServer definition.
        /// </summary>
        /// <param name="address">URL address.</param>
        /// <param name="port">Port number.</param>
        /// <param name="loopback">Loopback flag.</param>
        void CreateServer(string address, Int32 port, bool loopback);

        /// <summary>
        /// Create client mechanism.
        /// </summary>
        /// <param name="address">URL address.</param>
        /// <param name="port">Port number.</param>
        /// <returns>Success/Failure.</returns>
        bool CreateClient(string address, Int32 port);

        /// <summary>
        /// Automation command definition.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="hash">Hash.</param>
        /// <returns>Success/Failure.</returns>
        bool Send(AutomationCommand data, Int32 hash);

        /// <summary>
        /// Disconnect connection.
        /// </summary>
        void Disconnect();
    }
}