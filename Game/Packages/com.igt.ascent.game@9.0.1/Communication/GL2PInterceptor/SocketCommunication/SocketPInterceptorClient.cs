//-----------------------------------------------------------------------
// <copyright file = "SocketPInterceptorClient.cs" company = "IGT">
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
    using GL2PInterceptorLib;
    using Presentation.CommServices;

    /// <summary>
    /// Socket based implementation of IInterceptorClientCommunication, IPresentationInterceptorClient, and IPresentationAutomationClient.
    /// </summary>
    public class SocketPInterceptorClient : IInterceptorClientCommunication, IPresentationInterceptorClient, IPresentationAutomationClient
    {
        #region IInterceptorClientCommunication

        /// <inheritDoc />
        public void Connect()
        {
            // If already connected ignore connection request.
            if(SocketService == null || SocketService.Connected == false)
            {
                SocketService = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketService.Connect(ServiceIpEndPoint);

                // Begin asynchronously listening for incoming messages.
                SocketGL2PInterceptorCommunicationLib.AsyncReceiveMessages(SocketService,
                                                                           ProcessMessageBuffer,
                                                                           SocketConnectionError,
                                                                           SendReceiveTimeOut);
            }
        }

        /// <inheritDoc />
        public void Disconnect()
        {
            // If not already disconnected then disconnect.
            if(SocketService != null && SocketService.Connected == true)
            {
                SocketService.Shutdown(SocketShutdown.Both);
                SocketService.Close();
            }

            // Raise event that the client has become disconnected from the service.
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
                if(SocketService != null && SocketService.Connected)
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

        #region IPresentationInterceptorClient

        /// <inheritDoc />
        bool IPresentationInterceptorClient.IsMessagePending
        {
            get
            {
                lock(presentationMsgQueue)
                {
                    return presentationMsgQueue.Count > 0;
                }
            }
        }

        /// <inheritDoc />
        PresentationGenericMsg IPresentationInterceptorClient.GetNextMessage()
        {
            lock(presentationMsgQueue)
            {
                if((this as IPresentationInterceptorClient).IsMessagePending)
                {
                    return presentationMsgQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritDoc />
        event EventHandler IPresentationInterceptorClient.MessageReceived
        {
            add
            {
                presentationMessageReceived += value;
            }

            remove
            {
                presentationMessageReceived -= value;
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
            var sendStatus = SocketGL2PInterceptorCommunicationLib.SendMessage(SocketService,
                                                                  new PresentationInterceptorRequestCommunicationModeMsg(mode, key),
                                                                  SendReceiveTimeOut);
            if(sendStatus != SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok)
            {
                SocketConnectionError(SocketService, sendStatus, null);
            }
        }

        /// <inheritDoc />
        public event EventHandler<InterceptorCommunicationModeChangedEventArgs> ChangedCommunicationMode;

        #endregion

        #region IPresentationAutomationClient

        /// <inheritDoc />
        bool IPresentationAutomationClient.IsMessagePending
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
        AutomationGenericMsg IPresentationAutomationClient.GetNextMessage()
        {
            lock(presentationAutomationMsgQueue)
            {
                if((this as IPresentationAutomationClient).IsMessagePending)
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
        event EventHandler IPresentationAutomationClient.MessageReceived
        {
            add
            {
                presentationAutomationMessageReceived += value;
            }

            remove
            {
                presentationAutomationMessageReceived -= value;
            }
        }

        /// <inheritDoc />
        public void RequestActiveButtons()
        {
            SendMessageToConnectedClient(new PresentationAutomationRequestActiveButtonsMsg());
        }

        /// <inheritDoc />
        public void RequestFpsEnable(bool enabled)
        {
            SendMessageToConnectedClient(new PresentationAutomationEnableFpsMsg(enabled));
        }

        /// <inheritDoc />
        public void SimulateButtonPush(string buttonName)
        {
            SendMessageToConnectedClient(new PresentationAutomationSimulateButtonPushMsg(buttonName));
        }

        /// <inheritDoc />
        public void RequestScreenShotPng(Monitor requestedMonitor)
        {
            SendMessageToConnectedClient(new PresentationAutomationRequestScreenShotPngMsg(requestedMonitor));
        }

        /// <inheritDoc />
        public void RequestMonitorConfiguration()
        {
            SendMessageToConnectedClient(new PresentationAutomationRequestMonitorConfiguration());
        }

        #endregion

        /// <summary>
        /// Connect to Presentation Interceptor Service.
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
            var sendStatus = SocketGL2PInterceptorCommunicationLib.SendMessage(SocketService,
                                      msgObject,
                                      SendReceiveTimeOut);
            if (sendStatus != SocketGL2PInterceptorCommunicationLib.ErrorCondition.Ok)
            {
                SocketConnectionError(SocketService, sendStatus, null);
            }
        }
      
        /// <summary>
        /// The socket in the parameter has encountered an error.
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
            object msgObject;

            if(SerializationBinder != null)
            {
                binaryFormatter.Binder = SerializationBinder;
            }

            try
            {
                msgObject = (object)binaryFormatter.Deserialize(new MemoryStream(msgBuffer));
            }
            catch(SerializationException exception)
            {
                SocketConnectionError(SocketService,
                                      SocketGL2PInterceptorCommunicationLib.ErrorCondition.SerializationError, exception);
                var handler = CommunicationError;
                if(handler != null)
                {
                    handler(this, new CommunicationErrorEventArgs(exception, msgBuffer));
                }
                return;
            }

            if(msgObject is PresentationGenericMsg)
            {
                var presentationGenericMsg = msgObject as PresentationGenericMsg;
                presentationMsgQueue.Enqueue(presentationGenericMsg);

                var messageReceievedEvent = presentationMessageReceived;
                if(messageReceievedEvent != null)
                {
                    messageReceievedEvent(this, new EventArgs());
                }
            }
            else if(msgObject is AutomationGenericMsg)
            {
                var automationGenericMsg = msgObject as AutomationGenericMsg;
                presentationAutomationMsgQueue.Enqueue(automationGenericMsg);

                var messageReceievedEvent = presentationAutomationMessageReceived;
                if(messageReceievedEvent != null)
                {
                    messageReceievedEvent(this, new EventArgs());
                }
            }
            else if(msgObject is InterceptorGenericMsg)
            {
                if(msgObject is PresentationInterceptorCommunicationModeChangedMsg)
                {
                    var changedCommunicationModeEvent = ChangedCommunicationMode;
                    if(changedCommunicationModeEvent != null)
                    {
                        var mode = (msgObject as PresentationInterceptorCommunicationModeChangedMsg).Mode;
                        CommunicationMode = mode;
                        changedCommunicationModeEvent(this, new InterceptorCommunicationModeChangedEventArgs(mode));
                    }
                }
            }
        }

        /// <summary>
        /// Gets socket to use for connection to service.
        /// </summary>
        public Socket SocketService { get; private set; }

        /// <summary>
        /// Gets\Sets service endpoint address.
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
        /// Queue to store incoming Presentation Messages.
        /// </summary>
        private Queue<PresentationGenericMsg> presentationMsgQueue = new Queue<PresentationGenericMsg>();

        /// <summary>
        /// Event which is raised when a Presentation Message is received.
        /// </summary>
        private event EventHandler presentationMessageReceived;

        /// <summary>
        /// Queue to store incoming Presentation Automation Messages.
        /// </summary>
        private Queue<AutomationGenericMsg> presentationAutomationMsgQueue = new Queue<AutomationGenericMsg>();

        /// <summary>
        /// Event which is raised when a Presentation Automation Message is received.
        /// </summary>
        private event EventHandler presentationAutomationMessageReceived;

        /// <summary>
        /// Current communication mode.
        /// </summary>
        private InterceptorCommunicationMode communicationMode = InterceptorCommunicationMode.PassiveCommunication;
    }
}
