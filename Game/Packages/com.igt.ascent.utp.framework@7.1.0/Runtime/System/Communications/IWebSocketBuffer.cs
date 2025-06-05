// -----------------------------------------------------------------------
// <copyright file = "IWebSocketBuffer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    /// <summary>
    /// Send data delegate.
    /// </summary>
    /// <param name="data">Data byte string.</param>
    public delegate void SendDataDelegate(byte[] data);

    /// <summary>
    /// Apply protocol delegate.
    /// </summary>
    /// <param name="data">Data string.</param>
    /// <param name="isFinal">Is final message flag.</param>
    /// <param name="opCode">Op code.</param>
    /// <returns>Delegate.</returns>
    public delegate byte[] ApplyProtocolDelegate(string data, bool isFinal, byte opCode);

    /// <summary>
    /// Web socket buffer interface.
    /// </summary>
    public interface IWebSocketBuffer
    {
        /// <summary>
        /// Initialize the web socket protocol.
        /// </summary>
        /// <param name="protocol">Protocol.</param>
        void Initialize(WebSocketProtocol protocol);

        /// <summary>
        /// Send string data parameter.
        /// </summary>
        /// <param name="data">Data string.</param>
        void Send(string data);

        /// <summary>
        /// Onsend.
        /// </summary>
        void OnSend();

        /// <summary>
        /// Receive raw data.
        /// </summary>
        /// <param name="data">Data byte string.</param>
        /// <param name="parseData">Parsed data.</param>
        void ReceiveRaw(byte[] data, ParseData parseData);
    }
}