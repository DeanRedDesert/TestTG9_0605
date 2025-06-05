//-----------------------------------------------------------------------
// <copyright file = "SocketGLInterceptorService.cs" company = "IGT">
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
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Logging;
    using Core.Logic.Evaluator.Schemas;
    using Logic.CommServices;
    using Tilts;

    /// <summary>
    /// Socket based implementation of IInterceptorServiceCommunication, IGameLogicInterceptorService, and IGameLogicAutomationService.
    /// </summary>
    public class SocketGLInterceptorService : IInterceptorServiceCommunication, IGameLogicInterceptorService, IGameLogicAutomationService
    {
        #region Constructor

        /// <summary>
        /// Constructor for SocketGLInterceptorService.
        /// </summary>
        /// <param name="listeningPort">Port to listen for incoming client connections.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when listeningPort is less or equal to 0.</exception>
        /// <param name="localHostOnly">Bool to determine ifthe connection is local only.</param>
        public SocketGLInterceptorService(int listeningPort, bool localHostOnly)
        {
            if(listeningPort <= 0)
            {
                throw new ArgumentOutOfRangeException("listeningPort", "listeningPort parameter must be greater than 0.");
            }

            ListeningPort = listeningPort;
            this.localHostOnly = localHostOnly;
        }

        #endregion

        #region IInterceptorServiceCommunication

        /// <inheritDoc />
        public void AcceptClientConnections()
        {
            AcceptConnectionState = true;
            ListenForConnections();
        }

        /// <inheritDoc />
        public void CloseClientConnections()
        {
            AcceptConnectionState = false;

            if(ListenerSocket != null)
            {
                ListenerSocket.Close();
            }

            AbortClientConnection();
        }

        /// <inheritDoc />
        public bool IsClientConnected
        {
            get
            {
                if(SocketClient != null && SocketClient.Connected)
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region IGameLogicInterceptorService

        /// <inheritDoc />
        public void PresentationStateComplete(string stateName,
                                              string actionRequest,
                                              Dictionary<string, object> data)
        {
            SendMessageToConnectedClient(new GameLogicPresentationStateCompleteMsg(stateName, actionRequest, data));
        }

        /// <inheritdoc/>
        public void PostPresentationTilt(string tiltKey, ITilt presentationTilt,
                                         object[] titleFormatArgs, object[] messageFormatArgs)
        {
            SendMessageToConnectedClient(new GameLogicPostPresentationTiltMsg(tiltKey, presentationTilt,
                                                                              titleFormatArgs, messageFormatArgs));
        }

        /// <inheritdoc/>
        public void ClearPresentationTilt(string tiltKey)
        {
            SendMessageToConnectedClient(new GameLogicClearPresentationTiltMsg(tiltKey));
        }

        /// <inheritDoc />
        public bool IsPassiveInterceptor
        {
            get
            {
                if(communicationMode == InterceptorCommunicationMode.InterceptCommunication)
                {
                    return false;
                }

                return true;
            }
        }

        /// <inheritDoc />
        void IGameLogicInterceptorService.SendErrorMessage(InterceptorError errorType, string errorString)
        {
            SendMessageToConnectedClient(new GameLogicInterceptorErrorMsg(errorType, errorString));
        }

        /// <inheritDoc />
        public void SendCommunicationModeChanged(InterceptorCommunicationMode mode)
        {
            SendMessageToConnectedClient(new GameLogicInterceptorCommunicationModeChangedMsg(mode));
        }

        /// <inheritDoc />
        public event EventHandler<GameLogicMessageEventArgs> MessageReceivedForProcessing;

        #endregion

        #region IGameLogicAutomationService

        bool IGameLogicAutomationService.IsMessagePending
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
        void IGameLogicAutomationService.SendErrorMessage(InterceptorError errorType, string errorString)
        {
            SendMessageToConnectedClient(new GameLogicAutomationErrorMsg(errorType, errorString));
        }

        /// <inheritDoc />
        public void SendWinOutcome(WinOutcome winOutcome, string state, string description)
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendWinOutcomeMsg(winOutcome, state, description));
        }

        /// <inheritDoc />
        public void SendCellPopulationOutcome(CellPopulationOutcome cellPopulationOutcome, string state, string description)
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendCellPopulationOutcomeMsg(cellPopulationOutcome,
                                                                                              state,
                                                                                              description));
        }

        /// <inheritDoc />
        public void SendPaytable(string paytableFileName)
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendPaytableMsg(paytableFileName));
        }

        /// <inheritDoc />
        public void SendSdkVersion()
        {
            var sdkVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SendMessageToConnectedClient(new GameLogicAutomationSendSdkVersionMsg(sdkVersion));
        }

        /// <inheritDoc />
        public void SendConnectionReceived()
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendConnectionMsg());
        }

        /// <inheritDoc />
        public void SendRandomNumbersReceived()
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendRandomNumberMsg());
        }

        /// <inheritDoc />
        public void SendLogicDataServiceNames(IDictionary<string, IDictionary<int, string>> serviceNames)
        {
            SendMessageToConnectedClient(new GameLogicAutomationSendLogicDataServiceNamesMsg(serviceNames));
        }

        /// <inheritDoc />
        AutomationGenericMsg IGameLogicAutomationService.GetNextMessage()
        {
            
            lock (gameLogicAutomationMsgQueue)
            {
                if((this as IGameLogicAutomationService).IsMessagePending)
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
        public event EventHandler MessageReceived;

        #endregion

        /// <summary>
        /// Sends an error message to a specific socket.
        /// </summary>
        /// <param name="socket">Socket to send error message.</param>
        /// <param name="errorType">Error type encountered.</param>
        /// <param name="errorString">
        /// Error string that provides additional information about the error encountered.
        /// </param>
        private void SendErrorMessage(Socket socket, InterceptorError errorType, string errorString)
        {
            if(socket != null && socket.Connected)
            {
                SocketGL2PInterceptorCommunicationLib.SendMessage(socket,
                                                                  new GameLogicInterceptorErrorMsg(errorType, errorString),
                                                                  MessageSendReceiveTimeoutMs);
            }
        }

        /// <summary>
        /// Serializes and sends an object to connected clients.
        /// </summary>
        /// <param name="msgObject">Object to serialize and send to connected clients.</param>
        private void SendMessageToConnectedClient(object msgObject)
        {
            if(IsClientConnected)
            {
                if(SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient,
                                                                      msgObject,
                                                                      MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
        }

        /// <summary>
        /// Asynchronously begin listening for client connection requests.
        /// </summary>
        private void ListenForConnections()
        {
            if(AcceptConnectionState)
            {
                // Create a TCP/IP socket.
                ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = false
                };
                var connectionType = localHostOnly ? IPAddress.Loopback : IPAddress.Any;
                ListenerSocket.Bind(new IPEndPoint(connectionType, ListeningPort));
                ListenerSocket.Listen(MaxListenerSocketBacklog);
                ListenerSocket.BeginAccept(new AsyncCallback(OnClientConnection), ListenerSocket);
            }
        }

        /// <summary>
        /// Abort current client connection and begin listening for a new client connection.
        /// </summary>
        private void AbortClientConnection()
        {
            communicationMode = InterceptorCommunicationMode.PassiveCommunication;

            if(IsClientConnected && SocketClient != null)
            {
                SocketClient.Shutdown(SocketShutdown.Both);
                SocketClient.Close();
            }

            if(AcceptConnectionState)
            {
                ListenerSocket.BeginAccept(new AsyncCallback(OnClientConnection), ListenerSocket);
            }
        }

        /// <summary>
        /// Currently only one client connection is allowed, ifa no clients are connected
        /// to the service then allow the incoming connection to fully connect, otherwise
        /// shutdown the incoming socket's connection.
        /// </summary>
        /// <param name="ar">Status of the asynchronous operation.</param>
        private void OnClientConnection(IAsyncResult ar)
        {
            //Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            Socket newSocket = listener.EndAccept(ar);

            if(SocketClient == null || SocketClient.Connected == false)
            {
                SocketClient = newSocket;
                SocketGL2PInterceptorCommunicationLib.AsyncReceiveMessages(SocketClient, ProcessMessageBuffer, SocketConnectionError, 5000);
            }
            else
            {
                SendErrorMessage(newSocket, InterceptorError.ConnectionError, "Only active client connection is allowed to this service.");
                newSocket.Shutdown(SocketShutdown.Both);
                newSocket.Close();
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
            if(exception != null)
            {
                Log.Write(String.Format("Game Logic Connection Error: {0} at {1}.", exception.Message, exception.Source));
            }
            else
            {
                Log.Write(String.Format("Game Logic Connection Error: {0}.", errorCondition));
            }

            AbortClientConnection();
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        /// <param name="msgBuffer">Message buffer.</param>
        private void ProcessMessageBuffer(byte[] msgBuffer)
        {
            var binaryFormatter = new BinaryFormatter();
            object msgObject;

            try
            {
                msgObject = binaryFormatter.Deserialize(new MemoryStream(msgBuffer));
            }
            catch (SerializationException ex)
            {
                (this as IGameLogicInterceptorService).SendErrorMessage(InterceptorError.UnknownMessageReceived, "Unable to de-serialize a message");
                Log.Write(String.Format("Serialization error {0}: Aborting client connection.", ex.Message));
                return;
            }

            if(msgObject is InterceptorGenericMsg)
            {
                if(msgObject is GameLogicInterceptorRequestCommunicationModeMsg)
                {
                    ChangeCommunicationMode(msgObject as GameLogicInterceptorRequestCommunicationModeMsg);
                }
            }
            else if(msgObject is GameLogicGenericMsg)
            {
                if(!IsPassiveInterceptor)
                {
                    var messageReceivedForProcessingEvent = MessageReceivedForProcessing;
                    if(messageReceivedForProcessingEvent != null)
                    {
                        var gameLogicGenericMsg = msgObject as GameLogicGenericMsg;
                        messageReceivedForProcessingEvent(this, new GameLogicMessageEventArgs(gameLogicGenericMsg));
                    }
                }
            }
            else if(msgObject is AutomationGenericMsg)
            {
                lock (gameLogicAutomationMsgQueue)
                {
                    gameLogicAutomationMsgQueue.Enqueue(msgObject as AutomationGenericMsg);
                }

                var automationMessageReceived = MessageReceived;
                if(automationMessageReceived != null)
                {
                    automationMessageReceived(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Process request to change communication modes.
        /// </summary>
        /// <param name="msg">GameLogicInterceptorRequestCommunicationModeMsg Message</param>
        private void ChangeCommunicationMode(GameLogicInterceptorRequestCommunicationModeMsg msg)
        {
            // If the requested communication mode is Intercept, check the security key
            // to make sure it is valid for changing the communication mode to intercept.
            if(msg.Mode == InterceptorCommunicationMode.InterceptCommunication &&
                msg.Key.GetHashCode() != ChangeModeToInterceptHashKey)
            {
                return;
            }

            communicationMode = msg.Mode;
            SendCommunicationModeChanged(communicationMode);
        }

        /// <summary>
        /// Port to listen for incoming connections.
        /// </summary>
        public int ListeningPort { get; set; }

        /// <summary>
        /// Determine ifthe connection should be limited to local only.
        /// </summary>
        private bool localHostOnly;

        /// <summary>
        /// Socket to listen for incoming connections.
        /// </summary>
        private Socket ListenerSocket { get; set; }

        /// <summary>
        /// Client socket connection.
        /// </summary>
        private Socket SocketClient { get; set; }

        /// <summary>
        /// Property to indicate ifthe service is accepting incoming
        /// client connections.
        /// </summary>
        private bool AcceptConnectionState { get; set; }

        /// <summary>
        /// Max number of concurrent clients that can attempt to connect
        /// to the service.
        /// </summary>
        private const int MaxListenerSocketBacklog = 0;

        /// <summary>
        /// Max time in milliseconds to send a message.
        /// </summary>
        private const int MessageSendReceiveTimeoutMs = 2000;

        /// <summary>
        /// Communication Mode.
        /// </summary>
        private InterceptorCommunicationMode communicationMode = InterceptorCommunicationMode.PassiveCommunication;

        /// <summary>
        /// Key used to validate Requests to change interceptor mode to intercept.
        /// </summary>
        private const int ChangeModeToInterceptHashKey = 1732914579;

        /// <summary>
        /// Queue to store incoming Game Logic Automation Messages.
        /// </summary>
        private Queue<AutomationGenericMsg> gameLogicAutomationMsgQueue = new Queue<AutomationGenericMsg>();

    }
}
