// -----------------------------------------------------------------------
// <copyright file = "IShellHistoryStore.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for accessing shell's history critical data during Play Mode.
    /// </summary>
    /// <remarks>
    /// All APIs in this interface deal with the access to the coplayer's corresponding shell history store.
    /// A coplayer's corresponding shell history store is the shell's history store that is
    /// linked to the coplayer's current game cycle.
    /// </remarks>
    public interface IShellHistoryStore
    {
        /// <summary>
        /// Event occurs when the write permitted status of a coplayer's corresponding shell history store has changed.
        /// </summary>
        event EventHandler<ShellHistoryStoreWritePermittedChangedEventArgs> ShellHistoryStoreWritePermittedChangedEvent;

        /// <summary>
        /// The event that is triggered when the Foundation logs the end of a game cycle.
        /// </summary>
        event EventHandler<LogEndGameCycleEventArgs> LogEndGameCycleEvent;

        /// <summary>
        /// Gets the list of all coplayers whose corresponding shell history stores are write permitted.
        /// Coplayers not appearing in the list means that their corresponding shell history stores are not write permitted.
        /// </summary>
        /// <returns>
        /// The list of all coplayers whose corresponding shell history stores are write permitted.
        /// </returns>
        IReadOnlyList<int> WritePermittedCoplayers { get; }

        /// <summary>
        /// Checks if a given coplayer's corresponding shell history store is write permitted.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is to be checked.
        /// </param>
        /// <returns>
        /// True if the given coplayer's corresponding shell history store is write permitted; False otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        bool IsWritePermitted(int coplayerId);

        /// <summary>
        /// Reads multiple critical data items from the given coplayer's corresponding shell history store.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is read from.
        /// </param>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="nameList"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        ICriticalDataBlock Read(int coplayerId, IList<CriticalDataName> nameList);

        /// <summary>
        /// Removes multiple critical data items the given coplayer's corresponding shell history store.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is removed from.
        /// </param>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to remove from the store.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="nameList"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Remove(int coplayerId, IList<CriticalDataName> nameList);

        /// <summary>
        /// Writes multiple critical data items to the given coplayer's corresponding shell history store.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is written to.
        /// </param>
        /// <param name="criticalDataBlock">
        /// Specifies a data block interface that contains all the serialized critical data to write.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the name list in <paramref name="criticalDataBlock"/> is empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Write(int coplayerId, ICriticalDataBlock criticalDataBlock);
    }
}