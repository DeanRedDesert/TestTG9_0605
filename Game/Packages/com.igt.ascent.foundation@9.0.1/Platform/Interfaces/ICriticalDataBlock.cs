// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataBlock.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines a container of multiple pieces of critical data from a key-value store.
    /// </summary>
    public interface ICriticalDataBlock
    {
        /// <summary>
        /// Gets the critical data indexed by <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the data to be deserialized.
        /// </typeparam>
        /// <param name="name">
        /// The key to retrieve critical data from the block.
        /// </param>
        /// <returns>
        /// The data after deserialization.
        /// If <paramref name="name"/> is present in the block, but no critical data is available for it,
        /// default(T) will be returned. For nullable types, it would be null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is not present in the block.
        /// </exception>
        T GetCriticalData<T>(CriticalDataName name);

        /// <summary>
        /// Gets the serialized critical data indexed by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The key to retrieve critical data from the block.
        /// </param>
        /// <returns>
        /// The serialized critical data (the bytes before being deserialized).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is not present in the block..
        /// </exception>
        byte[] GetSerializedData(CriticalDataName name);

        /// <summary>
        /// Sets the critical data indexed by <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the data to be serialized and written to the block.
        /// </typeparam>
        /// <param name="name">
        /// The key to set the critical data in the block.
        /// If the name of the critical data exists, its data in the block will be replaced;
        /// otherwise, it will create a new critical data item in the block.
        /// </param>
        /// <param name="data">
        /// The data to set.
        /// </param>
        void SetCriticalData<T>(CriticalDataName name, T data);

        /// <summary>
        /// Sets the serialized critical data indexed by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The key to set the critical data in the block.
        /// If the name of the critical data exists, its data in the block will be replaced;
        /// otherwise, it will create a new critical data item in the block.
        /// </param>
        /// <param name="data">
        /// The serialized critical data to set (the bytes before being deserialized).
        /// </param>
        void SetSerializedData(CriticalDataName name, byte[] data);

        /// <summary>
        /// Deletes the specific critical data item from the block.
        /// </summary>
        /// <param name="name">
        /// The key of the critical data to be deleted from the block.
        /// </param>
        void DeleteCriticalData(CriticalDataName name);

        /// <summary>
        /// Checks if a critical data item presents in the block. The absence of the
        /// item means that this critical data block has no knowledge of the status of
        /// the critical data, including whether the critical data exists or not.
        /// </summary>
        /// <param name="name">
        /// The key of the critical data.
        /// </param>
        /// <returns>
        /// True if <paramref name="name"/> is in the block, false otherwise.
        /// </returns>
        bool Contains(CriticalDataName name);

        /// <summary>
        /// Retrieves a name list of all the critical data in the block.
        /// </summary>
        /// <returns>The name list of all the critical data in the block.</returns>
        IList<CriticalDataName> GetNameList();

        /// <summary>
        /// Gets the flag indicating whether the critical data block has any data,
        /// i.e. at least one piece of critical data name has been set.
        /// </summary>
        bool HasData { get; }
    }
}