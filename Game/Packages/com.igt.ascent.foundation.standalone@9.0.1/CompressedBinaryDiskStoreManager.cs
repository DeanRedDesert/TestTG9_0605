// -----------------------------------------------------------------------
// <copyright file = "CompressedBinaryDiskStoreManager.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// A binary backed DiskStoreManager that compresses its safe storage on disk.
    /// </summary>
    internal class CompressedBinaryDiskStoreManager : BinaryDiskStoreManager
    {
        /// <summary>
        /// Parameterless constructor creates a CompressedDiskStoreManager
        /// which is not file backed. Data saved in such a disk store
        /// will not persist between game launches.
        /// </summary>
        public CompressedBinaryDiskStoreManager()
        {
        }

        /// <inheritdoc />
        public CompressedBinaryDiskStoreManager(string modifierPath, string committedPath) : base(modifierPath, committedPath)
        {
        }

        /// <inheritdoc/>
        protected override void ReadFromArray<T>(ref T data, byte[] serializedData)
        {
            using(var stream = new MemoryStream(serializedData))
            {
                using(var gzStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    ReadDataFromStream(ref data, gzStream);
                }
            }
        }

        /// <inheritdoc/>
        protected override void WriteToArray(object data, out byte[] serializedData)
        {
            using(var stream = new MemoryStream())
            {
                using(var gzStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    WriteDataToStream(data, gzStream);
                }
                serializedData = stream.ToArray();
            }
        }
    }
}