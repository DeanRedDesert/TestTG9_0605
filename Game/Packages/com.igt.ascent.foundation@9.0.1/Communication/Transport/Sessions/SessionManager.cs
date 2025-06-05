// -----------------------------------------------------------------------
// <copyright file = "SessionManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport.Sessions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// An implementation of <see cref="ISessionManager"/> that virtualizes an instance of <see cref="ITransport"/>
    /// in order to provide multi-session communications.
    /// </summary>
    /// <remarks>
    /// This implementation of <see cref="ISessionManager"/> is not thread-safe. Creation and destruction should happen
    /// from the same thread.
    /// </remarks>
    public class SessionManager : ISessionManager
    {
        #region Nested TransportProxy class

        /// <summary>
        /// Virtualizes an instance of <see cref="ITransport"/>, ensuring that accesses are thread-safe, and only the
        /// first call to <see cref="ITransport.Connect"/> and the last call to <see cref="ITransport.Disconnect"/>
        /// are seen by the underlying <see cref="ITransport"/> instance.
        /// </summary>
        private class TransportProxy : ITransport
        {
            private readonly ITransport transport;
            private readonly object transportLocker = new object();
            private int connectedClients;

            /// <summary>
            /// Initializes a new <see cref="TransportProxy"/> with the given <see cref="ITransport"/>.
            /// </summary>
            /// <param name="transport">The underlying <see cref="ITransport"/> to virtualize.</param>
            internal TransportProxy(ITransport transport)
            {
                this.transport = transport;
            }

            /// <inheritdoc />
            public WaitHandle ExceptionSignalHandle => transport.ExceptionSignalHandle;

            /// <inheritdoc />
            public int MajorVersion => transport.MajorVersion;

            /// <inheritdoc />
            public int MinorVersion => transport.MinorVersion;

            /// <inheritdoc />
            public Exception CheckException()
            {
                lock(transportLocker)
                {
                    return transport.CheckException();
                }
            }

            /// <inheritdoc />
            public void Connect()
            {
                lock(transportLocker)
                {
                    if(connectedClients == 0)
                    {
                        transport.Connect();
                    }
                    ++connectedClients;
                }
            }

            /// <inheritdoc />
            public void Disconnect()
            {
                lock(transportLocker)
                {
                    if(connectedClients > 0 && --connectedClients == 0)
                    {
                        transport.Disconnect();
                    }
                }
            }

            /// <inheritdoc />
            public void PrepareToDisconnect()
            {
                // do nothing
            }

            /// <inheritdoc />
            public void SendMessage(IBinaryMessage message)
            {
                lock(transportLocker)
                {
                    transport.SendMessage(message);
                }
            }

            /// <inheritdoc />
            /// <devdoc>
            /// Hide this from the <see cref="TransportProxy"/> API as its clients cache their own handler.
            /// </devdoc>
            void ITransport.SetMessageHandler(HandleMessageDelegate handler)
            {
                // do nothing
            }
        }

        #endregion

        #region Nested Session class

        /// <summary>
        /// Implements the <see cref="ISession"/> interface using a shared <see cref="TransportProxy"/> for the
        /// virtualized transport.
        /// </summary>
        private class Session : ISession
        {
            private TransportProxy proxy;
            private bool connected;

            /// <summary>
            /// Initializes a new <see cref="Session"/> with the given proxy and session ID.
            /// </summary>
            /// <param name="proxy">The <see cref="TransportProxy"/> representing the virtualized transport.</param>
            /// <param name="sessionIdentifier">The unique identifier for this session.</param>
            internal Session(TransportProxy proxy, int sessionIdentifier)
            {
                this.proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));

                SessionIdentifier = sessionIdentifier;
            }

            /// <summary>
            /// Gets the message handler for this session.
            /// </summary>
            internal HandleMessageDelegate MessageHandler { get; private set; }

            /// <summary>
            /// Destroys this session, rendering it unusable.
            /// </summary>
            internal void Destroy()
            {
                Disconnect();
                proxy = null;
            }

            /// <inheritdoc />
            public WaitHandle ExceptionSignalHandle => proxy.ExceptionSignalHandle;

            /// <inheritdoc />
            public int MajorVersion => proxy.MajorVersion;

            /// <inheritdoc />
            public int MinorVersion => proxy.MinorVersion;

            /// <inheritdoc />
            public Exception CheckException()
            {
                return proxy.CheckException();
            }

            /// <inheritdoc />
            public void Connect()
            {
                if(connected)
                {
                    return;
                }
                proxy.Connect();
                connected = true;
            }

            /// <inheritdoc />
            public void Disconnect()
            {
                if(!connected)
                {
                    return;
                }
                proxy.Disconnect();
                connected = false;
            }

            /// <inheritdoc />
            public void PrepareToDisconnect()
            {
                proxy.PrepareToDisconnect();
            }

            /// <inheritdoc />
            public void SendMessage(IBinaryMessage message)
            {
                var sessionHeaderSegment = new SessionHeaderSegment {SessionId = SessionIdentifier};
                message.Prepend(sessionHeaderSegment);
                proxy.SendMessage(message);
            }

            /// <inheritdoc />
            public void SetMessageHandler(HandleMessageDelegate handler)
            {
                MessageHandler = handler;
            }

            /// <inheritdoc />
            public int SessionIdentifier { get; }
        }

        #endregion

        private readonly Dictionary<int, Session> sessions = new Dictionary<int, Session>();
        private readonly TransportProxy transport;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager"/> class.
        /// </summary>
        /// <param name="transport">
        /// The underlying <see cref="ITransport"/> to virtualize. The session manager owns this transport, and installs its own message handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="transport"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="transport"/> does not have a major version of at least 2.
        /// </exception>
        public SessionManager(ITransport transport)
        {
            if(transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }
            if(transport.MajorVersion < 2)
            {
                throw new ArgumentException("Transport version 2 or greater required.", nameof(transport));
            }

            this.transport = new TransportProxy(transport);
            transport.SetMessageHandler(HandleMessage);
        }

        /// <inheritdoc />
        public ISession CreateSession(int sessionId)
        {
            if(sessions.ContainsKey(sessionId))
            {
                throw new SessionNotAvailableException(sessionId);
            }

            var session = new Session(transport, sessionId);
            sessions[sessionId] = session;
            return session;
        }

        /// <inheritdoc />
        public void DestroySession(ISession session)
        {
            if(session == null)
            {
                return;
            }
            if(!sessions.ContainsKey(session.SessionIdentifier))
            {
                return;
            }

            try
            {
                if(session is Session sessionImpl)
                {
                    sessionImpl.Destroy();
                }
            }
            finally
            {
                sessions.Remove(session.SessionIdentifier);
            }
        }

        /// <summary>
        /// The message handler that is installed into the underlying <see cref="ITransport"/> instance.
        /// </summary>
        /// <param name="messageReader">The <see cref="IBinaryMessageReader"/> containing the message.</param>
        private void HandleMessage(IBinaryMessageReader messageReader)
        {
            var sessionHeader = messageReader.Read<SessionHeaderSegment>();
            if(sessions.TryGetValue(sessionHeader.SessionId, out var session))
            {
                var handler = session.MessageHandler;
                handler?.Invoke(messageReader);
            }
        }
    }
}