//-----------------------------------------------------------------------
// <copyright file = "SocketPInterceptorService.cs" company = "IGT">
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
    using CommunicationLib;
    using Presentation.CommServices;

    /// <summary>
    /// Socket based implementation of IInterceptorServiceCommunication, IPresentationInterceptorService, and IPresentationAutomationService.
    /// </summary>
    public class SocketPInterceptorService : IInterceptorServiceCommunication, IPresentationInterceptorService, IPresentationAutomationService
    {

        #region Constructor

        /// <summary>
        /// Constructor for SocketPInterceptorService.
        /// </summary>
        /// <param name="listeningPort">Port to listen for incoming client connections.</param>
        /// <param name="localHostOnly">Bool to determine if the connection is local only.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when listeningPort is less or equal to 0.</exception>
        public SocketPInterceptorService(int listeningPort, bool localHostOnly)
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

        #region IPresentationInterceptorService

        /// <inheritDoc />
        public void UpdateAsynchData(string stateName, DataItems data)
        {
            if(IsClientConnected)
            {
                if(SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient,
                                                                  new PresentationUpDateAsynchDataMsg(stateName, data),
                                                                  MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
        }

        /// <inheritDoc />
        public void StartState(string stateName, DataItems stateData)
        {
            if(IsClientConnected)
            {
                if(SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient,
                                                                  new PresentationStartStateMsg(stateName, stateData),
                                                                  MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
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
        public void SendErrorMessage(InterceptorError errorType, string errorString)
        {
            if(IsClientConnected)
            {
                if(SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient,
                                                                  new PresentationInterceptorErrorMsg(errorType, errorString),
                                                                  MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
        }

        /// <inheritDoc />
        public void SendCommunicationModeChanged(InterceptorCommunicationMode mode)
        {
            if(IsClientConnected)
            {
                if(SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient,
                                                                  new PresentationInterceptorCommunicationModeChangedMsg(mode),
                                                                  MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
        }

        /// <inheritDoc />
        public event EventHandler<PresentationMessageEventArgs> MessageReceivedForProcessing;

        #endregion

        #region IPresentationAutomationService

        bool IPresentationAutomationService.IsMessagePending
        {
            get
            {
                lock(presentationAutomationMsgQueue)
                {
                    return presentationAutomationMsgQueue.Count > 0;
                }
            }
        }

        /// <inheritDoc />
        AutomationGenericMsg IPresentationAutomationService.GetNextMessage()
        {
           
            lock(presentationAutomationMsgQueue)
            {
                if((this as IPresentationAutomationService).IsMessagePending)
                {
                    return presentationAutomationMsgQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
            
        }

        /// <inheritDoc />
        public event EventHandler MessageReceived;

        /// <inheritDoc />
        public void SendActiveButtons(IList<string> activeButtonList)
        {
            SendMessageToConnectedClient(new PresentationAutomationSendActiveButtonsMsg(activeButtonList));            
        }


        /// <inheritDoc />
        public void SendPerformanceData(float performanceMetric, string state)
        {
            SendMessageToConnectedClient(new PresentationAutomationSendPerformanceData(performanceMetric, state));
        }

        /// <inheritDoc />
        public void SendPerformanceData(float performanceMetric)
        {
            SendMessageToConnectedClient(new PresentationAutomationSendPerformanceData(performanceMetric));
        }

        /// <inheritDoc />
        public void SendScreenShotPng(byte[] pngFile, Monitor monitor)
        {
            SendMessageToConnectedClient(new PresentationAutomationSendScreenShotPngMsg(pngFile, monitor));
        }

        /// <inheritDoc />
        public void SendMonitorConfiguration(IList<Monitor> monitors)
        {
            SendMessageToConnectedClient(new PresentationAutomationSendMonitorConfiguration(monitors));
        }

        #endregion

        /// <summary>
        /// Serializes and sends an object to connected clients.
        /// </summary>  
        /// <param name="msgObject">Object to serialize and send to connected clients.</param>
        private void SendMessageToConnectedClient(object msgObject)
        {
            if (IsClientConnected)
            {
                if (SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok !=
                    SocketGL2PInterceptorCommunicationLib.SendMessage(SocketClient, msgObject, MessageSendReceiveTimeoutMs))
                {
                    AbortClientConnection();
                }
            }
        }


        /// <summary>
        /// Sends an error message to connected clients.
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
                                                                  new PresentationInterceptorErrorMsg(errorType, errorString),
                                                                  MessageSendReceiveTimeoutMs);
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

            if(SocketClient != null && SocketClient.Connected)
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
        /// Currently only one client connection is allowed, if a no clients are connected
        /// to the service then allow the incoming connection to fully connect, otherwise
        /// shutdown the incoming socket's connection.
        /// </summary>
        /// <param name="ar">Status of the asynchronous operation.</param>
        private void OnClientConnection(IAsyncResult ar)
        {
            //Get the socket that handles the client request.
            var listener = (Socket)ar.AsyncState;
            var newClientSocket = listener.EndAccept(ar);

            if(SocketClient == null || SocketClient.Connected == false)
            {
                SocketClient = newClientSocket;
                SocketGL2PInterceptorCommunicationLib.AsyncReceiveMessages(SocketClient,
                                                                           ProcessMessageBuffer,
                                                                           SocketConnectionError,
                                                                           MessageSendReceiveTimeoutMs);
            }
            else
            {
                SendErrorMessage(newClientSocket, InterceptorError.ConnectionError, "Only active client connection is allowed to this service.");
                newClientSocket.Shutdown(SocketShutdown.Both);
                newClientSocket.Close();
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
                errorCondition == SocketGL2PInterceptorCommunicationLib.ErrorCondition.OperationTimedOut ||
                errorCondition == SocketGL2PInterceptorCommunicationLib.ErrorCondition.SocketDisposed)
            {
                AbortClientConnection();
            }
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
            catch(SerializationException)
            {
                (this as IPresentationInterceptorService).SendErrorMessage(InterceptorError.UnknownMessageReceived,
                                 "Unable to de-serialize a message");
                return;
            }

            if(msgObject is InterceptorGenericMsg)
            {
                if(msgObject is PresentationInterceptorRequestCommunicationModeMsg)
                {
                    ChangeCommunicationMode(msgObject as PresentationInterceptorRequestCommunicationModeMsg);
                }
            }
            else if(msgObject is PresentationGenericMsg)
            {
                if(!IsPassiveInterceptor)
                {
                    var messageReceivedForProcessingEvent = MessageReceivedForProcessing;
                    if(messageReceivedForProcessingEvent != null)
                    {
                        var presentationGenericMsg = msgObject as PresentationGenericMsg;
                        MessageReceivedForProcessing(this, new PresentationMessageEventArgs(presentationGenericMsg));
                    }
                }
            }
            else if(msgObject is AutomationGenericMsg)
            {
                lock(presentationAutomationMsgQueue)
                {
                    presentationAutomationMsgQueue.Enqueue(msgObject as AutomationGenericMsg);
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
        private void ChangeCommunicationMode(PresentationInterceptorRequestCommunicationModeMsg msg)
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
        /// Determine if the connection should be limited to local only.
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
        /// Property to indicate if the service is accepting incoming
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
        /// Queue to store incoming Presentation Automation Messages.
        /// </summary>
        private Queue<AutomationGenericMsg> presentationAutomationMsgQueue = new Queue<AutomationGenericMsg>();
    }
}
