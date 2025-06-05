//-----------------------------------------------------------------------
// <copyright file = "IGameCriticalData.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Interface for providing critical data access.
    /// </summary>
    public interface IGameCriticalData
    {
        /// <summary>
        /// Write data to safe storage. A transaction must
        /// be open before calling this function.
        /// </summary>
        /// <remarks>
        /// CAUTION: Persistent scopes will survive both E2 and RAM clear.  Do not use persistent scopes
        /// to store information that might depend on any configurations stored in E2 or RAM,
        /// such as currency, credit formatter etc.
        /// </remarks>
        /// <param name="scope">The critical data scope to write the data to.</param>
        /// <param name="path">The path to write the data to within the scope.</param>
        /// <param name="data">The data to write. The data must be serializable.</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when there is no open transaction available to use for write.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the current game mode.
        /// </exception>
        void WriteCriticalData(CriticalDataScope scope, CriticalDataName path, object data);

        /// <summary>
        /// Read critical data as the specified type.
        /// </summary>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <typeparam name="T">The element type of the data read.</typeparam>
        /// <returns>
        /// The data read from safe storage. If no data exists, then
        /// default(T) will be returned. For nullable types default(T)
        /// will be null.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the current game mode.
        /// </exception>
        T ReadCriticalData<T>(CriticalDataScope scope, CriticalDataName path);

        /// <summary>
        /// Read the critical data and fill it into the target data.
        /// </summary>
        /// <param name="data">The target data to be filled.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the current game mode.
        /// </exception>
        /// <remarks>
        /// The interface is added for less garbage collection, if the input object implements
        /// ICompactSerializable and has already allocated. 
        /// If critical data does not exist, <paramref name="data"/> will be set to default value. 
        /// If this is not desired, use <see cref="TryReadCriticalData{T}"/> instead. 
        /// </remarks>
        void ReadCriticalData<T>(ref T data, CriticalDataScope scope, CriticalDataName path);

        /// <summary>
        /// Read the critical data and fill it into the target data if it exists.
        /// </summary>
        /// <param name="data">The target data to be filled.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// Returns true if reading and filling target data was successful, or false if not.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the current game mode.
        /// </exception>
        /// <remarks>
        /// The interface is added for less garbage collection, if the input object implements ICompactSerializable
        /// and has already allocated. 
        /// </remarks>
        bool TryReadCriticalData<T>(ref T data, CriticalDataScope scope, CriticalDataName path);

        /// <summary>
        /// Remove a critical data item from the safe storage.
        /// </summary>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// True if the removal is success.  False otherwise.
        /// It could fail because the data does not exist, or it is read only.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when there is no open transaction available to use for write.
        /// </exception>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the current game mode.
        /// </exception>
        bool RemoveCriticalData(CriticalDataScope scope, CriticalDataName path);
    }
}