// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataAccessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the APIs for accessing critical data.
    /// </summary>
    public interface ICriticalDataAccessor
    {
        /// <summary>
        /// Reads critical data non-transactionally.
        /// </summary>
        /// <param name="criticalDataSelectors">
        /// A set of scope identifiers and paths for reading critical data.
        /// </param>
        /// <returns>
        /// Interface of <see cref="ICriticalDataChunk"/> that provides APIs of
        /// retrieving critical data by specified selector.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is empty.
        /// </exception>
        /// <remarks>
        /// It supports reading multiple pieces of data at a time, which is indexed
        /// by its scope identifier and path.
        /// Data should be read together for performance and data consistency.
        /// The <see cref="ICriticalDataChunk"/> provides method to retrieve data for
        /// a specific selector.
        /// </remarks>
        ICriticalDataChunk ReadNonTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors);

        /// <summary>
        /// Reads critical data transactionally.
        /// </summary>
        /// <param name="criticalDataSelectors">
        /// A set of scope identifiers and paths for reading critical data.
        /// </param>
        /// <returns>
        /// Interface of <see cref="ICriticalDataChunk"/> that provides APIs of
        /// retrieving critical data by specified selector.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is empty.
        /// </exception>
        /// <remarks>
        /// It supports reading multiple pieces of data at a time, which is indexed
        /// by its scope identifier and path.
        /// Data should be read together for performance and data consistency.
        /// The <see cref="ICriticalDataChunk"/> provides method to retrieve data for
        /// a specific selector.
        /// </remarks>
        ICriticalDataChunk ReadTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors);

        /// <summary>
        /// Removes critical data transactionally.
        /// </summary>
        /// <param name="criticalDataSelectors">
        /// A set of scope identifiers and paths for removing critical data.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="criticalDataSelectors"/> is empty.
        /// </exception>
        /// <remarks>
        /// It supports removing multiple pieces of data at a time, which is indexed
        /// by its scope identifier and path.
        /// </remarks>
        void RemoveTransactionalCriticalData(IList<CriticalDataSelector> criticalDataSelectors);

        /// <summary>
        /// Writes critical data transactionally.
        /// </summary>
        /// <param name="criticalDataObjects">
        /// A collection of critical data selectors and data to write to the critical data.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="criticalDataObjects"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="criticalDataObjects"/> is empty.
        /// </exception>
        /// <exception cref="InvalidSafeStorageTypeException">
        /// Thrown when any of the types defined in <paramref name="criticalDataObjects"/> cannot be serialized.
        /// </exception>
        /// <remarks>
        /// It supports writing multiple pieces of data at a time, which is keyed
        /// by its scope identifier and path.
        /// Data should be written together for performance and data consistency.
        /// </remarks>
        void WriteTransactionalCriticalData(IDictionary<CriticalDataSelector, object> criticalDataObjects);
    }
}