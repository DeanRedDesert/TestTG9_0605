// -----------------------------------------------------------------------
// <copyright file = "CombinedCommServices.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using CommunicationLib;
    using Logic.CommServices;
    using Presentation.CommServices;

    /// <summary>
    /// This class encapsulates two message queues and exposes their functionality
    /// via different interfaces for different users.
    /// </summary>
    /// <inheritdoc cref="ILogicCommServices"/>
    /// <inheritdoc cref="IPresentationCommServices"/>
    public sealed class CombinedCommServices : ILogicCommServices, IPresentationCommServices, IDisposable
    {
        #region Private Fields

        private readonly LogicMessageQueue logicMessageQueue = new LogicMessageQueue();

        private readonly PresentationMessageQueue presentationMessageQueue = new PresentationMessageQueue();

        private bool isDisposed;

        #endregion

        #region ILogicCommServices Implementation

        /// <inheritdoc/>
        public ILogicHostControl LogicHostControl => logicMessageQueue;

        /// <inheritdoc/>
        public IEventQueue PresentationEventQueue => logicMessageQueue;

        /// <inheritdoc/>
        public IEventDispatcher PresentationEventDispatcher => logicMessageQueue;

        /// <inheritdoc/>
        public IPresentation PresentationClient => presentationMessageQueue;

        /// <inheritdoc/>
        public IPresentationTransition PresentationTransition => presentationMessageQueue;

        #endregion

        #region IPresentationCommServices Implementation

        /// <inheritdoc/>
        public IPresentationServiceHost PresentationHost => presentationMessageQueue;

        /// <inheritdoc/>
        public IGameLogic GameLogicClient => logicMessageQueue;

        /// <inheritdoc/>
        public IPlayerSessionLogic PlayerSessionLogicClient => logicMessageQueue;

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    logicMessageQueue.Dispose();
                }

                isDisposed = true;
            }
        }

        #endregion
    }
}