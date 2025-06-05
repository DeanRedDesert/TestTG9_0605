// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataChunk.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface defines a container of multiple pieces of critical data,
    /// which is indexed by <see cref="CriticalDataSelector"/>.
    /// </summary>
    /// <remarks>
    /// For now, it only supports retrieving critical data.
    /// New APIS can be added for writing/removing critical data in future.
    /// </remarks>
    public interface ICriticalDataChunk
    {
        /// <summary>
        /// Retrieves the critical data indexed by <paramref name="criticalDataSelector"/>.
        /// </summary>
        /// <param name="criticalDataSelector">
        /// The key to retrieve critical data from the chunk.
        /// </param>
        /// <typeparam name="T">The type of the data to be de-serialized.</typeparam>
        /// <returns>
        /// The data after de-serialization.
        /// If the critical data for <paramref name="criticalDataSelector"/> does not
        /// exist, default(T) will be returned. For nullable types, it would be null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="criticalDataSelector"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="criticalDataSelector"/> is not in the chunk.
        /// </exception>
        T RetrieveCriticalData<T>(CriticalDataSelector criticalDataSelector);

        /// <summary>
        /// Checks if a critical data selector presents in the chunk. The absence of the
        /// selector means that this critical data chunk has no knowledge of the status of
        /// the critical data, including whether the critical data exists or not.
        /// </summary>
        /// <param name="criticalDataSelector">
        /// The selector identifying the critical data.
        /// </param>
        /// <returns>
        /// True if <paramref name="criticalDataSelector"/> is in the chunk, false otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when the <paramref name="criticalDataSelector"/> is null.
        /// </exception>
        bool ContainsSelector(CriticalDataSelector criticalDataSelector);
    }
}