//-----------------------------------------------------------------------
// <copyright file = "StorageData.Extensions.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extension of StorageData that implements compact serialization.
    /// </summary>
    partial class StorageData : ICompactSerializable
    {
        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, DataPath);
            CompactSerializer.Write(stream, Data);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            DataPath = CompactSerializer.ReadString(stream);
            Data = CompactSerializer.ReadByteArray(stream);
        }
    }
}