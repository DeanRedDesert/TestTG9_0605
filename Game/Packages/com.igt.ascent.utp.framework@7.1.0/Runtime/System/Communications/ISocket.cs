// -----------------------------------------------------------------------
// <copyright file = "ISocket.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System.IO;

    /// <summary>
    /// Interface to keep Socket modular and allow mock sockets for testing.
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();
    }
}
