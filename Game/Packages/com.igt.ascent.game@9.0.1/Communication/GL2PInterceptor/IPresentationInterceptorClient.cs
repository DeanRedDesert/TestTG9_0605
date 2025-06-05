//-----------------------------------------------------------------------
// <copyright file = "IPresentationInterceptorClient.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Presentation.CommServices;

    /// <summary>
    /// Defines API for a client intended to connect to a Presentation Interceptor
    /// Service.
    /// </summary>
    public interface IPresentationInterceptorClient
    {
        /// <summary>
        /// Gets a bool indicates if there are any pending messages.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        /// Gets the next available Presentation Message received, returns
        /// null if there is not an available message.
        /// </summary>
        PresentationGenericMsg GetNextMessage();

        /// <summary>
        /// Event which is raised when a Presentation Message is received.
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
