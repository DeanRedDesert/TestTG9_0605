// -----------------------------------------------------------------------
// <copyright file = "StandardFactory.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Game.Core.Communication.Foundation.Transport;
    using Game.Core.Communication.Foundation.Transport.Sessions;

    /// <summary>
    /// This factory class creates components to be used by Shell Runner in Standard run mode.
    /// </summary>
    public class StandardFactory : IPlatformFactory, IDisposable
    {
        #region Private Fields

        private readonly FoundationTarget foundationTarget;
        private readonly ISessionManager sessionManager;

        private readonly DisposableCollection disposableCollection = new DisposableCollection();

        private bool isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StandardFactory"/>.
        /// </summary>
        /// <param name="foundationTarget">The Foundation target.</param>
        /// <param name="transportConfiguration">The configuration of transport.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transportConfiguration"/> is null.
        /// </exception>
        public StandardFactory(FoundationTarget foundationTarget, TransportConfiguration transportConfiguration)
        {
            if(transportConfiguration == null)
            {
                throw new ArgumentNullException(nameof(transportConfiguration));
            }

            this.foundationTarget = foundationTarget;

            // Session manager must use a Socket Transport of version 2.0
            var socketTransport = new SocketTransport(transportConfiguration.Address,
                                                      transportConfiguration.Port,
                                                      2, 0);
            disposableCollection.Add(socketTransport);

            sessionManager = new SessionManager(socketTransport);
        }

        #endregion

        #region IPlatformFactory Implementation

        /// <inheritdoc/>
        /// <devdoc>
        /// Objects created by CreateXXX methods are to be disposed by the caller.
        /// </devdoc>
        public IShellLibRestricted CreateShellLib(IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations,
                                                  IGameParcelComm parcelCommMock = null)
        {
            return new Communication.Platform.ShellLib.Standard.ShellLib(foundationTarget,
                                                                         sessionManager.CreateSession(0),
                                                                         interfaceExtensionConfigurations);
        }

        /// <inheritdoc/>
        /// <devdoc>
        /// Objects created by CreateXXX methods are to be disposed by the caller.
        /// </devdoc>
        public ICoplayerLibRestricted CreateCoplayerLib(int coplayerId,
                                                        int sessionId,
                                                        IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations)
        {
            return new Communication.Platform.CoplayerLib.Standard.CoplayerLib(coplayerId,
                                                                               foundationTarget,
                                                                               sessionManager.CreateSession(sessionId),
                                                                               interfaceExtensionConfigurations);
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be isDisposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(isDisposed)
            {
                return;
            }

            if(disposing)
            {
                disposableCollection.Dispose();
            }

            isDisposed = true;
        }

        #endregion
    }
}