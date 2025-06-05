//-----------------------------------------------------------------------
// <copyright file = "VirtualRange.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// This struct which exposes from and to points of a virtual range.
    /// </summary>
    [Serializable]
    public struct VirtualRange : ICompactSerializable
    {
        /// <summary>
        /// The begin position of this range.
        /// </summary>
        /// <remarks>The value is included in the range.</remarks>
        public int From;

        /// <summary>
        /// The end position of this range.
        /// </summary>
        /// <remarks>The value is included in the range.</remarks>
        public int To;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualRange" /> struct.
        /// </summary>
        /// <param name="from">The begin position of this range.</param>
        /// <param name="to">The end position of this range.</param>
        public VirtualRange(int from, int to)
        {
            From = from;
            To = to;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, From);
            CompactSerializer.Write(stream, To);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            From = CompactSerializer.ReadInt(stream);
            To = CompactSerializer.ReadInt(stream);
        }

        #endregion
    }
}
