//-----------------------------------------------------------------------
// <copyright file = "SocketGLInterceptorClient.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib.SocketCommunication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Logic.CommServices;

    /// <summary>
    /// Socket based implementation of IInterceptorClientCommunication, IGameLogicInterceptorClient, and IGameLogicAutomationClient.
    /// </summary>
    public class SocketGLInterceptorClient : IInterceptorClientCommunication, IGameLogicInterceptorClient, IGameLogicAutomationClient
    {
        #region IInterceptorClientCommunication

        /// <inheritDoc />
        public void Connect()
        {
            // If already connected ignore connection request.
            if(ServiceSocket == null || ServiceSocket.Connected == false)
            {
                ServiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ServiceSocket.Connect(ServiceIpEndPoint);

                // Begin asynchronously listening for incoming messages.
                SocketGL2PInterceptorCommunicationLib.AsyncReceiveMessages(ServiceSocket,
                                                                           ProcessMessageBuffer,
                                                                           SocketConnectionError,
                                                                           SendReceiveTimeOut);
            }
        }

        /// <inheritDoc />
        public void Disconnect()
        {
            if(ServiceSocket != null && ServiceSocket.Connected == true)
            {
                ServiceSocket.Shutdown(SocketShutdown.Both);
                ServiceSocket.Close();
            }

            var disconnectedEvent = DisconnectedFromService;
            if(disconnectedEvent != null)
            {
                disconnectedEvent(this, new EventArgs());
            }
        }

        /// <inheritDoc />
        public bool IsServiceConnected
        {
            get
            {
                if(ServiceSocket != null && ServiceSocket.Connected)
                {
                    return true;
                }

                return false;
            }
        }

        /// <inheritDoc />
        public event EventHandler DisconnectedFromService;

        /// <inheritdoc />
        public event EventHandler<CommunicationErrorEventArgs> CommunicationError;

        /// <inheritdoc />
        public SerializationBinder SerializationBinder
        {
            get;
            set;
        }

        #endregion

        #region IGameLogicInterceptorClient

        /// <inheritDoc />
        bool IGameLogicInterceptorClient.IsMessagePending
        {
            get
            {
                lock (gameLogicMsgQueue)
                {
                    return gameLogicMsgQueue.Count > 0;
                }
            }
        }

        /// <inheritDoc />
        GameLogicGenericMsg IGameLogicInterceptorClient.GetNextMessage()
        {
            lock (gameLogicMsgQueue)
            {
                if((this as IGameLogicInterceptorClient).IsMessagePending)
                {
                    return gameLogicMsgQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritDoc />
        event EventHandler IGameLogicInterceptorClient.MessageReceived
        {
            add
            {
                gameLogicMessageReceived += value;
            }
            
            remove
            {
                gameLogicMessageReceived -= value;
            }
        }

        /// <inheritDoc />
        public InterceptorCommunicationMode CommunicationMode
        {
            get
            {
                return communicationMode;
            }
            set
            {
                communicationMode = value;
            }
        }

        /// <inheritDoc />
        public void RequestCommunicationMode(InterceptorCommunicationMode mode, string key)
        {
            var sendStatus = SocketGL2PInterceptorCommunicationLib.SendMessage(ServiceSocket,
                                                                               new GameLogicInterceptorRequestCommunicationModeMsg(mode, key),
                                                                               SendReceiveTimeOut);
            if(sendStatus != SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok)
            {
                SocketConnectionError(ServiceSocket, sendStatus, null);
            }
        }

        /// <inheritDoc />
        public event EventHandler<InterceptorCommunicationModeChangedEventArgs> ChangedCommunicationMode;

        #endregion

        #region IGameLogicAutomationClient

        /// <inheritDoc />
        bool IGameLogicAutomationClient.IsMessagePending
        {
            get
            {
                lock (gameLogicAutomationMsgQueue)
                {
                    return gameLogicAutomationMsgQueue.Count > 0;
                }
            }
        }

        /// <inheritDoc />
        AutomationGenericMsg IGameLogicAutomationClient.GetNextMessage()
        {
            lock (gameLogicAutomationMsgQueue)
            {
                if((this as IGameLogicAutomationClient).IsMessagePending)
                {
                    return gameLogicAutomationMsgQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritDoc />
        event EventHandler IGameLogicAutomationClient.MessageReceived
        {
            add
            {
                gameLogicAutomationMessageReceived += value;
            }

            remove
            {
                gameLogicAutomationMessageReceived -= value;
            }
        }

        /// <inheritDoc />
        public void RequestPaytableFilename()
        {
            SendMessageToConnectedClient(new GameLogicAutomationRequestPaytableMsg());
        }

        /// <inheritDoc />
        public void RequestSetupPrepickedProvider()
        {
            SendMessageToConnectedClient(new GameLogicAutomationRequestSetupPrepickProviderMsg());
        }

        /// <inheritDoc />
        public void RequestSdkVersion()
        {
            SendMessageToConnectedClient(new GameLogicAutomationRequestSdkVersionMsg());
        }

        /// <inheritDoc />
        public void SendRandomNumbers(IEnumerable<int> numbers)
        {
            SendMessageToConnectedClient(new GameLogicAutomationRequestRandomNumberMsg(numbers));
        }

        /// <inheritDoc />
        public void RequestLogicDataServiceNames(IDictionary<string, IList<int>> serviceNamesRequests)
        {
            SendMessageToConnectedClient(
                new GameLogicAutomationRequestLogicDataServiceNamesMsg(serviceNamesRequests));
        }

        #endregion

        /// <summary>
        /// Connect to Game Logic Interceptor Service.
        /// </summary>
        /// <param name="serviceIpEndPoint">Service EndPoint</param>
        public void Connect(IPEndPoint serviceIpEndPoint)
        {
            ServiceIpEndPoint = serviceIpEndPoint;
            Connect();
        }

        /// <summary>
        /// Serializes and sends an object to connected clients.
        /// </summary>
        /// <param name="msgObject">Object to serialize and send to connected clients.</param>
        private void SendMessageToConnectedClient(object msgObject)
        {
            var sendStatus = SocketGL2PInterceptorCommunicationLib.SendMessage(ServiceSocket,
                                      msgObject,
                                      SendReceiveTimeOut);
            if(sendStatus != SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok)
            {
                SocketConnectionError(ServiceSocket, sendStatus, null);
            }
        }

        /// <summary>
        /// The socket in the parameter has encountered an error, abort the current connection.
        /// </summary>
        /// <param name="socket">Socket that encountered an error.</param>
        /// <param name="errorCondition">Error condition of the socket.</param>
        /// <param name="exception">Exception that triggered the error condition, may be null.</param>
        private void SocketConnectionError(Socket socket, SocketGL2PInterceptorCommunicationLib.ErrorCondition errorCondition, Exception exception)
        {
            if(errorCondition == SocketGL2PInterceptorCommunicationLib.ErrorCondition.SocketDisconnected ||
                errorCondition == SocketGL2PInterceptorCommunicationLib.ErrorCondition.OperationTimedOut)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        /// <param name="msgBuffer">Message buffer.</param>
        private void ProcessMessageBuffer(byte[] msgBuffer)
        {
            var binaryFormatter = new BinaryFormatter();
            object msgObject = null;

            if(SerializationBinder != null)
            {
                binaryFormatter.Binder = SerializationBinder;
            }

            try
            {
                msgObject = (object)binaryFormatter.Deserialize(new MemoryStream(msgBuffer));
            }
            catch (SerializationException exception)
            {
                SocketConnectionError(ServiceSocket,
                      SocketGL2PInterceptorCommunicationLib.ErrorCondition.SerializationError, exception);
                var handler = CommunicationError;
                if(handler != null)
                {
                    handler(this, new CommunicationErrorEventArgs(exception, msgBuffer));
                }
                return;
            }

            if(msgObject is GameLogicGenericMsg)
            {
                var gameLogicGenericMsg = msgObject as GameLogicGenericMsg;
                gameLogicMsgQueue.Enqueue(gameLogicGenericMsg);

                var messageReceievedEvent = gameLogicMessageReceived;
                if(messageReceievedEvent != null)
                {
                    messageReceievedEvent(this, new EventArgs());
                }
            }
            else if(msgObject is AutomationGenericMsg)
            {
                var automationGenericMsg = msgObject as AutomationGenericMsg;
                gameLogicAutomationMsgQueue.Enqueue(automationGenericMsg);

                var messageReceievedEvent = gameLogicAutomationMessageReceived;
                if(messageReceievedEvent != null)
                {
                    gameLogicAutomationMessageReceived(this, new EventArgs());
                }
            }
            else if(msgObject is InterceptorGenericMsg)
            {
                if(msgObject is GameLogicInterceptorCommunicationModeChangedMsg)
                {
                    var changedCommunicationModeEvent = ChangedCommunicationMode;
                    if(changedCommunicationModeEvent != null)
                    {
                        var mode = (msgObject as GameLogicInterceptorCommunicationModeChangedMsg).Mode;
                        CommunicationMode = mode;
                        changedCommunicationModeEvent(this, new InterceptorCommunicationModeChangedEventArgs(mode));
                    }
                }
            }
        }

        /// <summary>
        /// Socket to use for connection to service.
        /// </summary>
        public Socket ServiceSocket { get; private set; }

        /// <summary>
        /// Service address.
        /// </summary>
        public IPEndPoint ServiceIpEndPoint { get; set; }

        /// <summary>
        /// Gets\Sets timeout to send or receive messages.
        /// </summary>
        public uint SendReceiveTimeOut
        {
            get
            {
                return sendReceiveTimeOut;
            }
            set
            {
                sendReceiveTimeOut = value;
            }
        }

        /// <summary>
        /// Timeout in milliseconds to send or receive messages.
        /// </summary>
        private uint sendReceiveTimeOut = 2000;

        /// <summary>
        /// Queue to store incoming Game Logic Messages.
        /// </summary>
        private Queue<GameLogicGenericMsg> gameLogicMsgQueue = new Queue<GameLogicGenericMsg>();

        /// <summary>
        /// Event which is raised when a Game Logic Message is received.
        /// </summary>
        private event EventHandler gameLogicMessageReceived;

        /// <summary>
        /// Queue to store incoming Game Logic Automation Messages.
        /// </summary>
        private Queue<AutomationGenericMsg> gameLogicAutomationMsgQueue = new Queue<AutomationGenericMsg>();

        /// <summary>
        /// Event which is raised when a Game Logic Automation Message is received.
        /// </summary>
        private event EventHandler gameLogicAutomationMessageReceived;

        /// <summary>
        /// Current communication mode.
        /// </summary>
        private InterceptorCommunicationMode communicationMode = InterceptorCommunicationMode.PassiveCommunication;
    }
}
