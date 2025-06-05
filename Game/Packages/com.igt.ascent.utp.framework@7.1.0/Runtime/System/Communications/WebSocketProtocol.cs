// -----------------------------------------------------------------------
// <copyright file = "WebSocketProtocol.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Define BufferReceiveDelegate.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="parseData">ParseData</param>
    public delegate void BufferReceiveDelegate(byte[] data, ParseData parseData);

    /// <summary>
    /// Opcode enum definition.
    /// </summary>
    public enum Opcode
    {
        /// <summary>
        /// The continuation
        /// </summary>
        Continuation,

        /// <summary>
        /// The text
        /// </summary>
        Text,

        /// <summary>
        /// The binary
        /// </summary>
        Binary,

        /// <summary>
        /// The close
        /// </summary>
        Close = 8,

        /// <summary>
        /// The ping
        /// </summary>
        Ping,

        /// <summary>
        /// The pong
        /// </summary>
        Pong,
    }

    /// <summary>
    /// Define and setup a proper web socket protocol.
    /// </summary>
    public class WebSocketProtocol
    {
        #region fields

        /// <summary>
        /// WebSocket protocol during connection. It is used to validate the handshake.
        /// </summary>
        private const string magicNumber = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// WebSocket version.
        /// </summary>
        private const string strSecWebSocketVersion = "Sec-WebSocket-Version";

        /// <summary>
        /// WebSocket accept string.
        /// </summary>
        private const string strSecWebSocketAccept = "Sec-WebSocket-Accept";

        /// <summary>
        /// Socket buffer size definition.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Int32 BufferSize { get; set; }

        /// <summary>
        /// If set then perform WebSocket protocol masking. Clients mask, servers don't.
        /// </summary>
        private readonly bool masking;

        /// <summary>
        /// Web socket connection.
        /// </summary>
        private WebSocketConnection connection;

        /// <summary>
        /// Parse data protocol.
        /// </summary>
        public ParseData ProtocolParseData;

        /// <summary>
        /// Buffer receive delegate.
        /// </summary>
        private BufferReceiveDelegate bufferReceiveDelegate;

        /// <summary>
        /// Constant use for formatting string result.
        /// </summary>
        private const int formattingSpace = 2;

        #endregion

        #region Constructor/Initialize

        /// <summary>
        /// WebSocketProtocol() constructor.
        /// </summary>
        /// <param name="mask">True for client, false for server.</param>
        public WebSocketProtocol(bool mask)
        {
            masking = mask;
        }

        /// <summary>
        /// Initialize() post instantiation set up.
        /// </summary>
        /// <param name="socketConnection">The socketConnection this is attached to.</param>
        /// <param name="webSocketBuffer">The buffer object to call when data is received.</param>
        public void Initialize(WebSocketConnection socketConnection, IWebSocketBuffer webSocketBuffer)
        {
            if(webSocketBuffer == null)
            {
                throw new ArgumentNullException("webSocketBuffer");
            }

            connection = socketConnection;
            bufferReceiveDelegate += webSocketBuffer.ReceiveRaw;
        }

        #endregion Constructor/Initialize

        #region Handshake
        #region ServerHandshake

        /// <summary>
        /// PerformHandshake()
        /// Performs the WebSocket preliminary handshake needed to establish a WebSocket.
        /// The handshake is performed in a thin HTTP server. It is expecting a proper WebSocket upgrade handshake.
        /// If inappropriate HTTP commands are received, a generic 403 Forbidden response is replied.
        /// </summary>
        /// <param name="conn">The socket to use.</param>
        /// <returns>True if a WebSocket has been established.</returns>
        public static bool PerformServerHandshake(ISocket conn)
        {
            if(conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            bool retVal;
            String webSocketsVersion = "";

            using(var stream = conn.GetStream())
            using(var reader = new StreamReader(stream))
            using(var writer = new StreamWriter(stream))
            {
                var headers = new Dictionary<string, string>();
                string line;
                while((line = ReadLine(stream)) != string.Empty)
                {
                    var tokens = line.Split(new[] { ':' }, 2);
                    if(!string.IsNullOrEmpty(line) && tokens.Length > 1)
                    {
                        headers[tokens[0]] = tokens[1].Trim();
                    }
                }

                if((!headers.ContainsKey("Upgrade")) || (!headers.ContainsKey("Connection")))
                {
                    // Compose the failure.
                    string temporim = BadConnection();
                    // Write the failure to the socket.
                    writer.Write(temporim);

                    // Close up the streams, then the socket.
                    writer.Close();
                    reader.Close();
                    stream.Close();
                    conn.Close();
                    return false;
                }
                headers["Upgrade"] = headers["Upgrade"].ToLower();
                headers["Connection"] = headers["Connection"].ToLower();
                // Check if the socketConnection is proper
                if((headers["Connection"] != "upgrade") || (headers["Upgrade"] != "websocket"))
                {
                    // Compose the failure.
                    string temporim = BadConnection();
                    // Write the failure to the socket.
                    writer.Write(temporim);

                    // Close up the streams, then the socket.
                    writer.Close();
                    reader.Close();
                    stream.Close();
                    conn.Close();
                    retVal = false;
                }
                else
                {
                    // Check websockets version
                    if(headers.ContainsKey(strSecWebSocketVersion))
                    {
                        webSocketsVersion = headers[strSecWebSocketVersion];
                    }
                    // send handshake to the client according to the websockets version requested
                    switch(webSocketsVersion)
                    {
                        case "13":
                            writer.Write(HandShake13(ComputeWebSocketAccept(headers)));
                            retVal = true;
                            break;
                        default:
                            writer.Write(HandShakeDefault());
                            retVal = false;
                            break;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// BadConnection() - Compose a reply if socketConnection attempt is from a browser.
        /// It sets up an http header indicating 403 Forbidden, then an html payload of a simple web page.
        /// </summary>
        /// <returns>The string to send to the browser.</returns>
        private static String BadConnection()
        {
            string httpHeader = "HTTP/1.1 403 Forbidden" + Environment.NewLine +
                                "Date: " + DateTime.Now + Environment.NewLine +
                                "Content-Type: text/html" + Environment.NewLine +
                                "Content-Length: ";

            string htmlPayload = "<!DOCTYPE html><html><body>" + Environment.NewLine +
                                 "<h1>403 Forbidden</h1>" + Environment.NewLine +
                                 "Whoa there cowboy! You are trying to lasso a cactus. <br />" + Environment.NewLine +
                                 "This port only allows WebSocket connections - By order of SQES SDET Reno.<br />" + Environment.NewLine +
                                 "</body></html>" + Environment.NewLine;

            string response = httpHeader + htmlPayload.Length + Environment.NewLine + Environment.NewLine + htmlPayload;

            return response;
        }

        /// <summary>
        /// ComputeWebSocketAccept() returns a string that performs the SHA1 hash on the WebSocket Key, and the magic number. Then 
        /// does a base 64 conversion on the result.
        /// </summary>
        /// <param name="headers">A database of the headers in the client handshake.</param>
        /// <returns>wsAccept protocol.</returns>
        private static String ComputeWebSocketAccept(Dictionary<string, string> headers)
        {
            var key = headers["Sec-WebSocket-Key"];
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            string wsAccept = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(key + magicNumber)));
            return wsAccept;
        }

        /// <summary>
        /// HandShake13()
        /// Produces the proper response to a version 13 websocket request.
        /// </summary>
        /// <param name="wsAccept">wsAccept.</param>
        /// <returns>Resultant response.</returns>
        private static String HandShake13(string wsAccept)
        {
            String response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine +
                              "Date: " + DateTime.Now + Environment.NewLine +
                              "Upgrade: websocket" + Environment.NewLine +
                              "Connection: Upgrade" + Environment.NewLine +
                              "Sec-WebSocket-Accept: " + wsAccept + Environment.NewLine +
                              Environment.NewLine;
            return response;
        }

        /// <summary>
        /// Default handshake response.
        /// </summary>
        /// <returns>Response.</returns>
        private static String HandShakeDefault()
        {
            string httpHeader = "HTTP/1.1 400 Bad Request" + Environment.NewLine +
                                "Date: " + DateTime.Now + Environment.NewLine +
                                "Content-Type: text/html" + Environment.NewLine +
                                "Content-Length: ";

            string htmlPayload = "<!DOCTYPE html><html><body>" + Environment.NewLine +
                                 "<h1>400 Bad Request</h1>" + Environment.NewLine +
                                 "Whoa there cowboy! You are lassoing a cow, but we are expecting steers.<br />" + Environment.NewLine +
                                 "Something is wrong with your WebSocket socketConnection attempt. Fix it - By order of SQES SDET Reno.<br />" + Environment.NewLine +
                                 "</body></html>" + Environment.NewLine;

            string response = httpHeader + htmlPayload.Length + Environment.NewLine + Environment.NewLine + htmlPayload;

            return response;
        }

        #endregion ServerHandshake

        #region ClientHandshake

        /// <summary>
        /// PerformClientHandshake() performs the WebSocket preliminary handshake needed to establish a WebSocket.
        /// </summary>
        /// <param name="conn">The socket to handshake over.</param>
        /// <returns></returns>
        public static bool PerformClientHandshake(Socket conn)
        {
            bool retVal = false;
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            if(conn.RemoteEndPoint != null)
            {
                EndPoint endPoint = conn.RemoteEndPoint;

                using(var stream = new NetworkStream(conn))
                using(new StreamReader(stream))
                using(new StreamWriter(stream))
                {
                    var headers = new Dictionary<string, string>();
                    string line;
                    string webSocketKey;
                    byte[] opening = Encoding.UTF8.GetBytes(HandshakeOpening(endPoint.ToString(), webSocketKey = GenerateKey()));
                    conn.Send(opening);

                    while((line = ReadLine(stream)) != string.Empty)
                    {
                        var tokens = line.Split(new[] { ':' }, 2);
                        if(!string.IsNullOrEmpty(line) && tokens.Length > 1)
                        {
                            headers[tokens[0]] = tokens[1].Trim();
                        }
                    }

                    if((!headers.ContainsKey("Upgrade")) || (!headers.ContainsKey("Connection")))
                    {
                        return false;
                    }

                    headers["Upgrade"] = headers["Upgrade"].ToLower();
                    headers["Connection"] = headers["Connection"].ToLower();
                    // Check if the socketConnection is proper
                    if((headers["Connection"] != "upgrade") || (headers["Upgrade"] != "websocket"))
                    {
                        // Determine from RFC 6455 what to do here.
                    }
                    else
                    {
                        // Create a local hash of the key and the magic number.
                        byte[] computedHashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(webSocketKey + magicNumber));
                        byte[] convertedBytes = Convert.FromBase64String(headers[strSecWebSocketAccept]);
                        // Check the key.
                        bool match = !computedHashBytes.Where((tByte, paramInt) => tByte != convertedBytes[paramInt]).Any();
                        if(match)
                        {
                            retVal = true;
                        }
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// GenerateKey() creates a base64 encoded string from an array of 16 random bytes.
        /// Used as the validation token in the WebSocket protocol.
        /// </summary>
        /// <returns></returns>
        private static string GenerateKey()
        {
            var keySource = new byte[16];

            var rng = new Random();
            for(int rngInt = 0; rngInt < 16; rngInt++)
            {
                keySource[rngInt] = (byte)rng.Next();
            }
            string retVal = Convert.ToBase64String(keySource);
            return retVal;
        }

        /// <summary>
        /// The first communication from the client to the host to perform the WebSocket handshake.
        /// </summary>
        /// <param name="host">The IP-4 : port of the host.</param>
        /// <param name="key">A base64 encoded array of 16 random bytes.</param>
        /// <returns></returns>
        private static string HandshakeOpening(string host, string key)
        {
            string httpHeader = "GET / HTTP/1.1" + Environment.NewLine +
                                "Date: " + DateTime.Now + Environment.NewLine +
                                "Upgrade: websocket" + Environment.NewLine +
                                "Connection: Upgrade" + Environment.NewLine +
                                "Host: " + host + Environment.NewLine +
                                "Origin: null" + Environment.NewLine +
                                "Sec-WebSocket-Key: " + key + Environment.NewLine +
                                "Sec-WebSocket-Version: 13" + Environment.NewLine +
                                Environment.NewLine;

            return httpHeader;
        }

        #endregion ClientHandshake

        /// <summary>
        /// ReadLine() reads a line of data from the provided stream.
        /// </summary>
        /// <param name="stream">A stream reader stream from the NetworkStream.</param>
        /// <returns>A string of the most recent line from the string.</returns>
        static string ReadLine(Stream stream)
        {
            var buffer = new List<byte>();
            while(true)
            {
                buffer.Add((byte)stream.ReadByte());
                var line = Encoding.ASCII.GetString(buffer.ToArray());
                if(line.EndsWith(Environment.NewLine))
                {
                    return line.Substring(0, line.Length - formattingSpace);
                }
            }
        }

        #endregion Handshake

        #region Receive

        /// <summary>
        /// DataReceived() performs WebSocket command processing, or send data to buffer, stripped of the WebSocket protocol envelope.
        /// </summary>
        /// <param name="data">Data is a start frame, continuation frame, or end frame.</param>
        /// <param name="parseData">Received data.</param>
        public void DataReceived(byte[] data, ParseData parseData)
        {
            if(data == null)
            {
                throw new ArgumentNullException("data");
            }
            if(parseData == null)
            {
                throw new ArgumentNullException("parseData");
            }

            var dataStripped = new byte[parseData.PayloadSize];

            // Check if the received data is a WebSocket command.
            if((parseData.Opcode & 0x08) != 0)
            {
                var opcode = (byte)(parseData.Opcode & 0x0F);
                switch(opcode)
                {
                    case 8:
                        connection.Disconnect();
                        Thread.Sleep(1000);
                        break;
                    case 9:
                        break;
                    case 10:
                        break;
                    default:
                        // _Fail the WebSocket Connection_
                        break;
                }
            }
            else // Data is a start frame, continuation frame, or end frame. The start frame could be the end frame if the data is within one piece.
            {
                // Strip off the WebSockets protocol.
                for(Int64 dataStrippedInt = 0; dataStrippedInt < parseData.PayloadSize; dataStrippedInt++)
                {
                    dataStripped[dataStrippedInt] = data[dataStrippedInt + parseData.PayloadOffset];
                }
                // Now it should be a raw AutomationCommand.
                bufferReceiveDelegate(dataStripped, parseData);
            }
        }

        /// <summary>
        /// Parse the data buffer.
        /// </summary>
        /// <param name="dataBuffer">Data buffer.</param>
        /// <returns>Parsed data buffer.</returns>
        public long ParseDataBuffer(byte[] dataBuffer)
        {
            ProtocolParseData = new ParseData();
            return ParseDataBuffer(dataBuffer, ref ProtocolParseData);
        }

        /// <summary>
        /// ParseDataBuffer() extracts WebSocket protocol data from the envelope; fill parseData with the metadata.
        /// </summary>
        public Int64 ParseDataBuffer(byte[] dataBuffer, ref ParseData parseData)
        {
            parseData.Fin = (dataBuffer[0] & 0x80) != 0;
            parseData.Opcode = dataBuffer[0] & 0x0F;
            parseData.Mask = (dataBuffer[1] & 0x80) != 0;

            // Minimum data offset is 2
            Int32 dataOffset = 2;
            // Calculate where the mask offset starts.
            Int32 maskOffset = 0;
            if(parseData.Mask)
            {
                // If there is a mask, add four to the data offset.
                dataOffset += 4;
                // Update where the mask starts.
                maskOffset = 2;
            }
            Int32 payloadLength = dataBuffer[1] & 0x7F;
            // Assume the smallest data size. 0 - 125 bytes long.
            Int64 payloadSize = payloadLength;
            // If the data size is larger then the payloadLength encodes the length of the data.
            switch(payloadLength)
            {
                // When length is 127, then next eight bytes is the length.
                case 0x7F:
                    dataOffset += 8;
                    maskOffset += 8;
                    // Change endianess to allow conversion.
                    Array.Reverse(dataBuffer, 2, 8);
                    // Convert the 8 bytes into a Int64.
                    payloadSize = BitConverter.ToInt64(dataBuffer, 2);
                    break;
                // When length is 126, then the next two bytes is the length.
                case 0x7E:
                    dataOffset += 2;
                    maskOffset += 2;
                    // Change endianess to allow conversion.
                    Array.Reverse(dataBuffer, 2, 2);
                    // Convert the 2 bytes into a Int16
                    payloadSize = (UInt16)BitConverter.ToInt16(dataBuffer, 2);
                    break;
                // When the length is 0 - 125, the length is 0 - 125.
            }
            // Store the new values.
            parseData.PayloadOffset = dataOffset;
            parseData.MaskOffset = maskOffset;
            parseData.PayloadLength = payloadLength;
            parseData.PayloadSize = payloadSize;

            // Return the length of the WebSocket transmission.
            return (dataOffset + payloadSize);
        }

        /// <summary>
        /// DiagnosticPrint1()
        /// Used to debug protocol issues.
        /// </summary>
        /// <param name="parseData">The container that has the pertinent data.</param>
        public void DiagnosticPrint1(ParseData parseData)
        {
            string operation;
            switch(parseData.Opcode)
            {
                case 0:
                    operation = "continuation frame";
                    break;
                case 1:
                    operation = "text frame";
                    break;
                case 2:
                    operation = "binary frame";
                    break;
                case 8:
                    operation = "disconnect frame";
                    break;
                case 9:
                    operation = "ping frame";
                    break;
                case 10:
                    operation = "pong frame";
                    break;
                default:
                    operation = "undefined frame";
                    break;
            }
            Console.WriteLine("fin bit is " + parseData.Fin +
                              "\nopcode is " + parseData.Opcode + " = " + operation +
                              "\nmask is " + parseData.Mask +
                              "\nmaskOffset is " + parseData.MaskOffset +
                              "\nPayload length is " + parseData.PayloadLength +
                              "\nPayload size is " + parseData.PayloadSize +
                              "\nPayload starts at: " + parseData.PayloadOffset + "\n");
        }

        /// <summary>
        /// UnmaskPayload() - If the data is from a client, then use the mask to unmask the data.
        /// </summary>
        /// <param name="data">The data that needs unmasking.</param>
        /// <param name="mask">The mask data to use for unmasking.</param>
        public void UnmaskPayload(byte[] data, bool mask)
        {
            if(mask)
            {
                for(int ix = 0; ix < ProtocolParseData.PayloadSize; ix++)
                {
                    data[ProtocolParseData.PayloadOffset + ix] = (byte)(data[ProtocolParseData.PayloadOffset + ix] ^ data[ProtocolParseData.MaskOffset + (ix % 4)]);
                }
            }
        }

        #endregion // Receive

        #region Send

        /// <summary>
        /// Send() wraps the payload with a WebSocket envelope to conform to the WebSocket protocol.
        /// This method handles a string payload. Additional code will need to be added to handle binary data.
        /// Fortunately the WebSocket protocol specifies only a header. 
        /// </summary>
        /// <param name="payload">The payload data that needs to be stuffed into a WebSocket envelope.</param>
        /// <param name="final">A flag to indicate the last chunk of data.</param>
        /// <param name="opcode">The WebSocket protocol opcode indicating data or command, etc.</param>
        /// <returns>Buffer.</returns>
        public byte[] Send(string payload, bool final, byte opcode)
        {
            byte[] payloadBuffer = Encoding.UTF8.GetBytes(payload);
            Int32 maskIndex = 0;
            UInt16 shortLength = 0;

            byte firstByte = 0, secondByte = 0;

            Int64 length = payloadBuffer.Length;

            // Now determine the length of the header based on the payloadBuffer length.
            // Default length.
            int headerSize = 2;
            short version;

            if(length < 126)
            {
                secondByte |= (byte)(length & 0x7F);
                version = 1;
            }
            else if(length <= 0xFFFF)
            {
                secondByte |= 126;
                headerSize += 2;
                shortLength = (UInt16)length;
                version = 2;
            }
            else
            {
                secondByte |= 127;
                headerSize += 8;
                version = 3;
            }

            // If the flavor of this protocol is client then add four mask bytes, and set the mask bit in the second byte.
            if(masking)
            {
                headerSize += 4;
                secondByte |= 0x80;
            }

            // Create a buffer big enough for the header and all the data.
            var buffer = new byte[length + headerSize];

            // Set the fin bit.
            if(final)
            {
                firstByte |= 0x80;
            }

            // Set the opcode, assume text for now.
            firstByte |= opcode;

            buffer[0] = firstByte;
            buffer[1] = secondByte;

            switch(version)
            {
                case 1:
                    // Don't add the length. Part of secondByte.
                    maskIndex = 2;
                    break;
                case 2:
                    // Add the short length.
                    buffer[2] = (byte)((shortLength & 0xFF00) >> 8);
                    buffer[3] = (byte)(shortLength & 0xFF);
                    maskIndex = 4;
                    break;
                case 3:
                    // Add the long length.
                    buffer[2] = (byte)((length & 0x7F00000000000000) >> 56);
                    buffer[3] = (byte)((length & 0x00FF000000000000) >> 48);
                    buffer[4] = (byte)((length & 0x0000FF0000000000) >> 40);
                    buffer[5] = (byte)((length & 0x000000FF00000000) >> 32);
                    buffer[6] = (byte)((length & 0x00000000FF000000) >> 24);
                    buffer[7] = (byte)((length & 0x0000000000FF0000) >> 16);
                    buffer[8] = (byte)((length & 0x000000000000FF00) >> 8);
                    buffer[9] = (byte)(length & 0x00000000000000FF);
                    maskIndex = 10;
                    break;
            }

            if(masking)
            {
                // Generate the mask
                byte[] maskBytes = GenerateMask();
                for(int ix = 0; ix < 4; ix++)
                {
                    buffer[maskIndex + ix] = maskBytes[ix];
                }
            }

            // Add the payload
            for(int ix = 0; ix < length; ix++)
            {
                buffer[(headerSize + ix)] = payloadBuffer[ix];
            }

            // Perform the masking.
            if(masking)
            {
                for(int ix = 0; ix < length; ix++)
                {
                    buffer[(headerSize + ix)] ^= buffer[(maskIndex + (ix % 4))];
                }
            }
            return buffer;
        }

        /// <summary>
        /// GenerateMask()
        /// Create a 4 byte array of random numbers, to be used by clients to mask the payload data.
        /// </summary>
        /// <returns>Four byte array of random numbers.</returns>
        private byte[] GenerateMask()
        {
            var mask = new byte[4];

            var rng = new Random();
            for(int ix = 0; ix < 4; ix++)
            {
                mask[ix] = (byte)rng.Next();
            }
            return mask;
        }

        #endregion // Send

        #region Disconnect

        /// <summary>
        /// Disconnect()
        /// Creates a disconnect WebSocket command for sending.
        /// </summary>
        /// <returns></returns>
        public byte[] Disconnect()
        {
            return (Send("", true, 8));
        }

        #endregion // Disconnect
    }
}