//-----------------------------------------------------------------------
// <copyright file = "IPresentationInterceptorService.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using CommunicationLib;

    /// <summary>
    /// Defines API for a Presentation Service that relays standard Presentation
    /// communication to an external applications.
    /// </summary>
    public interface IPresentationInterceptorService : IPresentation
    {
        /// <summary>
        /// Gets a bool indicating if the Service is in a Passive Intercept Mode.
        /// </summary>
        bool IsPassiveInterceptor { get; }

        /// <summary>
        /// Sends an error message to connected clients.
        /// </summary>
        /// <param name="errorType">Error type encountered.</param>
        /// <param name="errorDescription">
        /// Error string that provides additional information about the error encountered.
        /// </param>
        void SendErrorMessage(InterceptorError errorType, string errorDescription);

        /// <summary>
        /// Sends a message to connected clients informing them that the communication
        /// mode has changed.
        /// </summary>
        /// <param name="mode">Communication Mode.</param>
        void SendCommunicationModeChanged(InterceptorCommunicationMode mode);

        /// <summary>
        /// Event which is raised when a Presentation Message is received
        /// that needs to be processed.
        /// </summary>
        event EventHandler<PresentationMessageEventArgs> MessageReceivedForProcessing;
    }
}
