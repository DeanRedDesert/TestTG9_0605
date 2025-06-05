//-----------------------------------------------------------------------
// <copyright file = "CachedData.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Linq;
    using Transport;

    /// <summary>
    /// Class which represents cached critical data. CRC checks are performed to insure data consistency.
    /// Any data set after construction is saved as a "pending write".  Flushing pending writes commits them.
    /// </summary>
    internal class CachedData
    {
        #region fields

        /// <summary>
        /// Stored data.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// CRC calculated when the data was stored.
        /// </summary>
        private uint crc;

        /// <summary>
        /// Data store for a pending write to critical data.
        /// </summary>
        private byte[] pendingWriteData;

        /// <summary>
        /// CRC calculated when pendingWriteData was stored.
        /// </summary>
        private uint pendingWriteCrc;

        /// <summary>
        /// Tells if pendingWriteData is set, and therefore needs to be written to critical data.
        /// </summary>
        private bool pendingWriteDataExists;

        /// <summary>
        /// Tells whether to force a write to critical data, regardless of pending write status.
        /// </summary>
        public bool ForceWrite { get; set; }

        /// <summary>
        /// Property to tell if a pending write exists.
        /// </summary>
        public bool PendingWriteExists { get { return pendingWriteDataExists || ForceWrite; } }

        #endregion

        /// <summary>
        /// Gets and sets the cached data. A calculated CRC for the data will be compared against a stored CRC.
        /// Setting Data will store it as a pending write.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if(pendingWriteDataExists)
                {
                    ValidateData(pendingWriteData, pendingWriteCrc);
                    return CopyByteArray(pendingWriteData);
                }

                ValidateData(data, crc);
                return CopyByteArray(data);
            }

            set
            {
                // When setting, the value will be stored as pendingWriteData.
                if(value == null)
                {
                    pendingWriteData = null;
                    pendingWriteCrc = 0;
                    // Note: This may clear an existing pending write.
                    pendingWriteDataExists = data != null;
                }
                else
                {
                    pendingWriteCrc = Crc32.Calculate(value);

                    if(data != null &&                 // Do some quick checks before calling SequenceEqual.
                       data.Length == value.Length &&
                       pendingWriteCrc == crc &&
                       data.SequenceEqual(value))
                    {
                        // New value matches data.  Don't record a pending write.
                        // Note: This will clear any existing pending write.
                        ClearPendingWrite();
                    }
                    else
                    {
                        // Record value as a pending write.
                        pendingWriteData = CopyByteArray(value);
                        pendingWriteDataExists = true;
                    }
                }
            }
        }

        /// <summary>
        /// Construct a new cached data instance.
        /// </summary>
        /// <param name="data">Data being cached.</param>
        public CachedData(byte[] data)
        {
            this.data = CopyByteArray(data);
            crc = data == null ? 0 : Crc32.Calculate(data);
            ForceWrite = false;

            ClearPendingWrite();
        }

        /// <summary>
        /// Copy the pending write to the data buffer, and clear the pending write.
        /// </summary>
        public void FlushPendingWrite()
        {
            if(pendingWriteDataExists)
            {
                // Copy from pending to cached data.
                data = pendingWriteData;
                crc = pendingWriteCrc;

                ClearPendingWrite();
            }

            // Cached data has been written to critical data.
            // No need to force it to be written again.
            ForceWrite = false;
        }

        #region internal methods

        /// <summary>
        /// Clear a pending write.
        /// </summary>
        private void ClearPendingWrite()
        {
            pendingWriteData = null;
            pendingWriteCrc = 0;
            pendingWriteDataExists = false;
        }

        /// <summary>
        /// Make a copy of a byte array.
        /// </summary>
        /// <param name="original">The source byte array to target.</param>
        /// <returns>A copy of the byte array.</returns>
        private static byte[] CopyByteArray(byte[] original)
        {
            byte[] copy = null;

            if(original != null)
            {
                copy = new byte[original.Length];
                Array.Copy(original, copy, original.Length);
            }

            return copy;
        }

        /// <summary>
        /// Verify the contents of a local data cache.
        /// </summary>
        /// <param name="localData">The byte array of local data.</param>
        /// <param name="localDataCrc">The CRC previously stored for localData.</param>
        /// <exception cref="CacheCrcException">
        /// Thrown when the calculated CRC fails to match the calculated CRC.
        /// </exception>
        private static void ValidateData(byte[] localData, uint localDataCrc)
        {
            // If the local data is non-null, verify that its CRC is correct.
            if(localData != null)
            {
                var calculatedCrc = Crc32.Calculate(localData);
                if(localDataCrc != calculatedCrc)
                {
                    throw new CacheCrcException(localDataCrc, calculatedCrc);
                }
            }
            // If pendingWriteData is null, verify that its CRC is 0.
            else if(localDataCrc != 0)
            {
                //A CRC exists, but there is not any data, so something is corrupted.
                throw new CacheCrcException(localDataCrc, 0);
            }
        }

        #endregion
    }
}
