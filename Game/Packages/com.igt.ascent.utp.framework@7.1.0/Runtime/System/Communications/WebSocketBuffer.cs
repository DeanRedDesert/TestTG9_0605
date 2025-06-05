// -----------------------------------------------------------------------
// <copyright file = "WebSocketBuffer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Dispatch raw delegate with byte string and hash.
    /// </summary>
    /// <param name="data">Data byte string.</param>
    /// <param name="hash">Hash.</param>
    public delegate void DispatchRawDelegate(byte[] data, Int32 hash);

    /// <summary>
    /// WebSocketBuffer buffers received data and composes an AutomationCommand from several chunks.
    /// Divides large data transmissions into smaller chunks and queues them for sending.
    /// </summary>
    internal class WebSocketBuffer : IWebSocketBuffer
    {
        /// <summary>
        /// Byte queue.
        /// </summary>
        private readonly Queue<byte[]> bufferSend;

        /// <summary>
        /// Raw container queue.
        /// </summary>
        private readonly Queue<RawContainer> bufferReceiveRaw;

        /// <summary>
        /// List of bytes.
        /// </summary>
        private List<byte> rawBuffer;

        /// <summary>
        /// Send data delegate.
        /// </summary>
        private event SendDataDelegate SendDataEvent;

        /// <summary>
        /// Apply protocol event.
        /// </summary>
        private event ApplyProtocolDelegate ApplyProtocolEvent;

        /// <summary>
        /// Received to go delegate.
        /// </summary>
        private readonly DispatchRawDelegate receivedToGo;

        /// <summary>
        /// Web socket connection.
        /// </summary>
        private readonly WebSocketConnection connection;

        /// <summary>
        /// The busy flag is set if the socket is currently sending something and cleared when the message is complete.
        /// </summary>
        public bool Busy { get; set; }

        /// <summary>
        /// WebSocketBuffer() Constructor.
        /// </summary>
        /// <param name="webSocketLib">The object that has the public gateway to send receive data.</param>
        /// <param name="connection">The connection object.</param>
        public WebSocketBuffer(WebSocketLib webSocketLib, WebSocketConnection connection)
        {
            if(webSocketLib == null)
            {
                throw new ArgumentNullException("webSocketLib");
            }
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bufferSend = new Queue<byte[]>();

            bufferReceiveRaw = new Queue<RawContainer>();

            this.connection = connection;

            SendDataEvent += connection.Send;

            receivedToGo = webSocketLib.Dispatch;

            Busy = false;

            rawBuffer = new List<byte>();
        }

        /// <summary>
        /// Initialize()
        /// Initialize the delegate to apply the WebSocket protocol.
        /// This is done later due to the WebSocketProtocol not instantiated when this is instantiated.
        /// </summary>
        /// <param name="protocol">The protocol object used by this WebSocketConnection.</param>
        public void Initialize(WebSocketProtocol protocol)
        {
            if(protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            ApplyProtocolEvent += protocol.Send;
        }

        /// <summary>
        /// Send()
        /// Queues up the serialized AutomationCommand for sending, and if not busy it sends it.
        /// The WebSocket protocol is applied to the incoming data, and is converted to a byte[] simultaneously.
        /// </summary>
        /// <param name="data">The serialized AutomationCommand. It's a string.</param>
        public void Send(string data)
        {
            if(data == null)
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataX = null;

            // The data coming in at this point is a serialized AutomationCommand.
            Int32 size = data.Length;
            Int32 numberOfChunks = (size / (WebSocketConnection.BufferCapacity - 12)) + 1;
            Int32 sizeOfChunk = size / numberOfChunks;
            Int32 chunkSpan = sizeOfChunk;
            Int32 oddBit = size - (sizeOfChunk * numberOfChunks);

            // Do some splitting.
            for(int chunkIndex = 0; chunkIndex < numberOfChunks; chunkIndex++)
            {
                bool final = false;
                var opcode = (byte)Opcode.Continuation;
                // If this is the first piece then indicate the kind of data transmitted.
                if(chunkIndex == 0)
                {
                    opcode = (byte)Opcode.Text;
                }
                // If it is the last piece, then add the odd bit to the size of the last chunk.
                if(chunkIndex == (numberOfChunks - 1))
                {
                    sizeOfChunk += oddBit;
                    final = true;
                }

                string chunk = data.Substring((chunkIndex * chunkSpan), sizeOfChunk);

                // Send each part of the data to the protocol to wrap it up in an WebSockets envelope.
                if(ApplyProtocolEvent != null)
                {
                    dataX = ApplyProtocolEvent(chunk, final, opcode);
                }

                // Queue up the data fragment.
                bufferSend.Enqueue(dataX);
            }

            // Start to send the queued up data.
            if(!Busy)
            {
                OnSend();
            }
        }

        /// <summary>
        /// OnSend() performs send processing.
        /// Gets called when something is first put on the queue, and subsequently when the send is complete.
        /// </summary>
        public void OnSend()
        {
            // Anything in the buffer?
            if(bufferSend.Count > 0)
            {
                // Yes so go busy and send it.
                if(SendDataEvent != null)
                {
                    Busy = true;
                    SendDataEvent(bufferSend.Dequeue());
                }
            }
            else
            {
                // Nothing was in the buffer so we can't be busy.
                Busy = false;
            }
        }

        /// <summary>
        /// Receive raw data and parsedata. 
        /// </summary>
        /// <param name="data">Data to add to raw buffer.</param>
        /// <param name="parseDatad">Parse data.</param>
        public void ReceiveRaw(byte[] data, ParseData parseDatad)
        {
            if(data == null)
            {
                throw new ArgumentNullException("data");
            }
            if(parseDatad == null)
            {
                throw new ArgumentNullException("parseDatad");
            }

            // Append whatever data into a buffer.
            rawBuffer.AddRange(data);
            // Check if pd.Fin is true.
            if(parseDatad.Fin)
            {
                // If true, then enqueue the buffer, and dispatch it.
                // Create a holding byte array.
                var container = new byte[rawBuffer.Count];
                // Fill it with the buffer data.
                rawBuffer.CopyTo(container);
                // Buffer data into a queue.
                bufferReceiveRaw.Enqueue(new RawContainer(container));
                // Send it to whomever wanted it.
                DispatchRaw();
                // Clear out the buffer for new transmissions.
                rawBuffer = new List<byte>();
            }
        }

        /// <summary>
        /// DispatchRaw raw data.
        /// </summary>
        private void DispatchRaw()
        {
            receivedToGo(bufferReceiveRaw.Dequeue().Data, connection.GetHashCode());
        }
    }
}