// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataStore.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines a set of methods for accessing critical data in a key-value store.
    /// </summary>
    public interface ICriticalDataStore
    {
        /// <summary>
        /// Reads multiple critical data items in a CriticalDataStore.
        /// </summary>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="nameList"/> is null or empty.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        ICriticalDataBlock Read(IList<CriticalDataName> nameList);

        /// <summary>
        /// Reads a single critical data item in a CriticalDataStore.
        /// </summary>
        /// <param name="name">
        /// Identifies the name of critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains the critical data having been read.
        /// </returns>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        ICriticalDataBlock Read(CriticalDataName name);

        /// <summary>
        /// For each critical data name specified in the <paramref name="criticalDataBlock"/>,
        /// reads the serialized data from the store and sets it in the block.
        /// </summary>
        /// <param name="criticalDataBlock">
        /// The critical data block that specifies the names of the critical data to read,
        /// as well as holds the critical data that have been read.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataBlock"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the name list specified in <paramref name="criticalDataBlock"/> is null or empty.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Fill(ICriticalDataBlock criticalDataBlock);

        /// <summary>
        /// Removes multiple critical data items from a CriticalDataStore.
        /// </summary>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to remove from the store.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="nameList"/> is null or empty.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Remove(IList<CriticalDataName> nameList);

        /// <summary>
        /// Removes a single critical data item from a CriticalDataStore.
        /// </summary>
        /// <param name="name">
        /// Identifies the name of critical data to remove from the store.
        /// </param>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Remove(CriticalDataName name);

        /// <summary>
        /// Writes multiple critical data items to a CriticalDataStore.
        /// </summary>
        /// <param name="criticalDataBlock">
        /// Specifies a data block interface that contains all the serialized critical data to write.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataBlock"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the name list in <paramref name="criticalDataBlock"/> is empty.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Write(ICriticalDataBlock criticalDataBlock);
    }
}