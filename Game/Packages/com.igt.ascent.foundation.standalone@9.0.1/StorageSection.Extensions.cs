//-----------------------------------------------------------------------
// <copyright file = "StorageSection.Extensions.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extension of StorageSection that implement compact serialization.
    /// </summary>
    partial class StorageSection : ICompactSerializable
    {
        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Scopes);
            CompactSerializer.Write(stream, Name);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            Scopes = CompactSerializer.ReadListSerializable<StorageScope>(stream);
            Name = CompactSerializer.ReadString(stream);
        }
    }
}
