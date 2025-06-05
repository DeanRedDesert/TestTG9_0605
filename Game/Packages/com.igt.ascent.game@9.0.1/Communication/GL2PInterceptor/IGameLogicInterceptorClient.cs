//-----------------------------------------------------------------------
// <copyright file = "IGameLogicInterceptorClient.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Logic.CommServices;

    /// <summary>
    /// Defines API for a client intended to connect to a Game Logic Interceptor
    /// Service.
    /// </summary>
    public interface IGameLogicInterceptorClient
    {
        /// <summary>
        /// Gets a bool indicating if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        /// Gets the next available Game Logic Message received, returns
        /// null if there is not an available message.
        /// </summary>
        GameLogicGenericMsg GetNextMessage();

        /// <summary>
        /// Event which is raised when a Game Logic Message is received.
        /// </summary>
        event EventHandler MessageReceived;

        /// <summary>
        /// Gets the current communication mode.
        /// </summary>
        InterceptorCommunicationMode CommunicationMode { get; }

        /// <summary>
        /// Request that the service switches communication modes.
        /// </summary>
        /// <param name="mode">Communication Mode.</param>
        /// <param name="key">Security Key.</param>
        void RequestCommunicationMode(InterceptorCommunicationMode mode, string key);

        /// <summary>
        /// Event that is raised when the communication mode has been changed.
        /// </summary>
        event EventHandler<InterceptorCommunicationModeChangedEventArgs> ChangedCommunicationMode;
    }
}
