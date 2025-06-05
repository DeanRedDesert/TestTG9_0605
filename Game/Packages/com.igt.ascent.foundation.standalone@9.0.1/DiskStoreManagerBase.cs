// -----------------------------------------------------------------------
// <copyright file = "DiskStoreManagerBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// Base implementation of DiskStoreManager.
    /// </summary>
    /// <remarks>
    /// The implementation of this abstract class should set up the ModifierStore class within its own constructor.
    /// </remarks>
    internal abstract class DiskStoreManagerBase : IDiskStoreManager
    {
        #region Fields

        /// <summary>
        /// Writes are maintained in the modiferStore until the
        /// transaction is closed.
        /// </summary>
        protected DiskStore ModifierStore = new DiskStore();

        /// <summary>
        /// If the store is file backed then it has a
        /// path for the modifier file.
        /// </summary>
        protected string ModifierPath;

        /// <summary>
        /// If the store is file backed then it has a
        /// path for the committed file.
        /// </summary>
        protected string CommittedPath;

        /// <summary>
        /// This flag indicates if the "Safe" storage
        /// is backed by a file. If it is not then storage
        /// will be maintained in memory.
        /// </summary>
        protected bool FileBackedStore;

        #endregion

        #region Events

        /// <summary>
        /// Event which indicates that a transaction has been committed.
        /// </summary>
        public event EventHandler<TransactionCommittedEventArgs> TransactionCommittedEvent;

        #endregion

        #region IDiskStoreManager Methods

        /// <inheritdoc />
        public void ClearScope(DiskStoreSection section, int scope)
        {
            ModifierStore.ClearScope(section, scope);
        }

        /// <inheritdoc />
        public abstract void Commit();

        /// <inheritdoc />
        public bool Contains(DiskStoreSection section, int scope, string path)
        {
            return ModifierStore.Contains(section, scope, path);
        }

        /// <inheritdoc />
        public byte[] ReadRaw(DiskStoreSection section, int scope, string path)
        {
            return ModifierStore.ReadData(section, scope, path);
        }

        /// <inheritdoc />
        public T Read<T>(DiskStoreSection section, int scope, string path)
        {
            var returnData = default(T);
            Read(ref returnData, section, scope, path);

            return returnData;
        }

        /// <inheritdoc />
        public void Read<T>(ref T data, DiskStoreSection section, int scope, string path)
        {
            var readData = ReadRaw(section, scope, path);

            // Only attempt to deserialize it if the read data is not null.
            if(readData != null && readData.Length != 0)
            {
                ReadFromArray(ref data, readData);
            }
            else
            {
                data = default;
            }
        }

        /// <inheritdoc />
        /// <devdoc>
        /// The Foundation starts to support overwriting the critical data through 
        /// subdirectories since G Series. In order to be consistent with the behavior 
        /// of the Foundation, we remove the directory path check. However,
        /// it's not encouraged to overwrite a directory and its subdirectories.
        /// </devdoc>
        public void Write(DiskStoreSection section, int scope, string path, object data)
        {
            WriteToArray(data, out var serializedData);
            ModifierStore.WriteData(section, scope, path, serializedData);
        }

        /// <inheritdoc />
        public bool Remove(DiskStoreSection section, int scope, string path)
        {
            return ModifierStore.RemoveData(section, scope, path);
        }

        /// <inheritdoc />
        public void SwapScopes(DiskStoreSection section1, int scope1, DiskStoreSection section2, int scope2)
        {
            ModifierStore.SwapScopes(section1, scope1, section2, scope2);
        }

        /// <inheritdoc />
        public void CopyScope(DiskStoreSection sourceSection, int sourceScope,
                              DiskStoreSection destinationSection, int destinationScope)
        {
            ModifierStore.CopyScope(sourceSection, sourceScope, destinationSection, destinationScope);
        }

        /// <inheritdoc />
        public ICollection<string> GetManifest(DiskStoreSection section, int scope)
        {
            return ModifierStore.GetManifest(section, scope);
        }

        /// <inheritdoc />
        public int GetScopeUsage(DiskStoreSection section, int scope)
        {
            return ModifierStore.GetScopeUsage(section, scope);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Read from the given array to fill the given data.
        /// </summary>
        /// <typeparam name="T">The required type of the data to be read.</typeparam>
        /// <param name="data">The given data to be filled.</param>
        /// <param name="serializedData">A <see cref="byte"/> array containing serialized data.</param>
        protected virtual void ReadFromArray<T>(ref T data, byte[] serializedData)
        {
            using(var memoryStream = new MemoryStream(serializedData))
            {
                ReadDataFromStream(ref data, memoryStream);
            }
        }

        /// <summary>
        /// Writes the given data to a <see cref="byte"/> array.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="serializedData">A byte array containing the data that was written.</param>
        protected virtual void WriteToArray(object data, out byte[] serializedData)
        {
            using(var stream = new MemoryStream())
            {
                WriteDataToStream(data, stream);
                serializedData = stream.ToArray();
            }
        }

        /// <summary>
        /// Read data from the stream as the specified type.
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        /// <param name="data">The data read from the stream.</param>
        protected static void ReadDataFromStream<T>(ref T data, Stream stream)
        {
            if(CompactSerializer.Supports(typeof(T)))
            {
                CompactSerializer.Deserialize(ref data, stream);
            }
            else
            {
                var binaryFormatter = new BinaryFormatter();
                data = (T)binaryFormatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Write the given data to the given stream.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <param name="stream">Stream to write the data.</param>
        protected static void WriteDataToStream(object data, Stream stream)
        {
            if(CompactSerializer.Supports(data.GetType()))
            {
                CompactSerializer.Serialize(stream, data);
            }
            else
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                }
                catch(SerializationException serializationException)
                {
                    throw new InvalidSafeStorageTypeException(
                        "Type must be serializable to be written to safe storage.",
                        serializationException);
                }
                catch(Exception exception)
                {
                    //Exceptions for web or mobile may be type load exceptions, or security exceptions. These
                    //exceptions to not provide a clear understanding that there was a failure in serialization.
                    //The above clause would catch serialization issues related to the type itself.
                    throw new SerializationException("Failed to binary format: " + data.GetType(), exception);
                }
            }
            stream.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SendTransactionCommittedEvent()
        {
            TransactionCommittedEvent?.Invoke(this, new TransactionCommittedEventArgs());
        }

        #endregion
    }
}