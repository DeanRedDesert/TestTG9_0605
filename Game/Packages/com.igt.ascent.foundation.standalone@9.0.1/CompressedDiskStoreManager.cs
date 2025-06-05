//-----------------------------------------------------------------------
// <copyright file = "CompressedDiskStoreManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// A disk store manager that compresses/decompresses the data written to and read from it.
    /// </summary>
    internal class CompressedDiskStoreManager : DiskStoreManager
    {
        /// <summary>
        /// Parameterless constructor creates a CompressedDiskStoreManager
        /// which is not file backed. Data saved in such a disk store
        /// will not persist between game launches.
        /// </summary>
        public CompressedDiskStoreManager()
        {
        }

        /// <summary>
        /// Compressed file backed storage DiskStoreManager which provides basic safe
        /// storage. This storage does not guarantee data consistency.
        /// </summary>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        public CompressedDiskStoreManager(string modifierPath, string committedPath) : base(modifierPath, committedPath)
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