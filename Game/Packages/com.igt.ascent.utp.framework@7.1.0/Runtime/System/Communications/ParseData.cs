// -----------------------------------------------------------------------
// <copyright file = "ParseData.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;

    /// <summary>
    /// ParseData definition.
    /// </summary>
    public class ParseData
    {
        /// <summary>
        /// FI number.
        /// </summary>
        public bool Fin { get; set; }

        /// <summary>
        /// Mask.
        /// </summary>
        public bool Mask { get; set; }

        /// <summary>
        /// Opcode.
        /// </summary>
        public Int32 Opcode { get; set; }

        /// <summary>
        /// Payload offset.
        /// </summary>
        public Int32 PayloadOffset { get; set; }

        /// <summary>
        /// Mask offset.
        /// </summary>
        public Int32 MaskOffset { get; set; }

        /// <summary>
        /// Payload length.
        /// </summary>
        public Int32 PayloadLength { get; set; }

        /// <summary>
        /// Payload size.
        /// </summary>
        public Int64 PayloadSize { get; set; }

        /// <summary>
        /// Buffer size.
        /// </summary>
        public Int32 BufferSize { get; set; }
    }
}