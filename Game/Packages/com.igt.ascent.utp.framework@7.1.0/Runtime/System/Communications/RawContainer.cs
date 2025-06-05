// -----------------------------------------------------------------------
// <copyright file = "RawContainer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    /// <summary>
    /// RawContainer class to hold data string.
    /// </summary>
    public class RawContainer
    {
        /// <summary>
        /// Data byte definition.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Data container for incoming data.
        /// </summary>
        /// <param name="data"></param>
        public RawContainer(byte[] data)
        {
            Data = data;
        }
    }
}