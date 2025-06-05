// -----------------------------------------------------------------------
// <copyright file = "IDiskStoreManager.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Interface for a disk store manager, which manages critical data on disk.
    /// </summary>
    internal interface IDiskStoreManager
    {
        /// <summary>
        /// Event which indicates that a transaction has been committed.
        /// </summary>
        event EventHandler<TransactionCommittedEventArgs> TransactionCommittedEvent;

        /// <summary>
        /// Remove all the critical data entries from the specified scope.
        /// </summary>
        /// <param name="section">The section to remove the data from.</param>
        /// <param name="scope">The scope to clear in the specified section.</param>
        void ClearScope(DiskStoreSection section, int scope);

        /// <summary>
        /// Commit current writes to the committed store.
        /// </summary>
        void Commit();

        /// <summary>
        /// Check if an item already exists in the disk store.
        /// </summary>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>True if the item already exists in the disk store.  False otherwise.</returns>
        bool Contains(DiskStoreSection section, int scope, string path);

        /// <summary>
        /// Read data from the disk store as a byte array.
        /// </summary>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// The data read from safe storage. If no data exists, then null
        /// will be returned.
        /// </returns>
        byte[] ReadRaw(DiskStoreSection section, int scope, string path);

        /// <summary>
        /// Read data from the disk store as the specified type.
        /// </summary>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>
        /// The data read from safe storage. If no data exists, then default
        /// will be returned.
        /// </returns>
        T Read<T>(DiskStoreSection section, int scope, string path);

        /// <summary>
        /// Read data from the disk store to fill the input data.
        /// </summary>
        /// <param name="data">
        /// The data to be filled. If the data does not exist this value will be default.
        /// </param>
        /// <param name="section">The section to read the data from.</param>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        void Read<T>(ref T data, DiskStoreSection section, int scope, string path);

        /// <summary>
        /// Write data to safe storage.
        /// </summary>
        /// <param name="section">The section to write the data to.</param>
        /// <param name="scope">The scope of the section to write the data.</param>
        /// <param name="path">The path to write the data to within the scope.</param>
        /// <param name="data">The data to write. The data must be serializable.</param>
        /// <exception cref="InvalidSafeStorageTypeException">
        /// Thrown when <paramref name="data"/> is not serializable.
        /// </exception>
        /// <devdoc>
        /// The Foundation starts to support overwriting the critical data through 
        /// subdirectories since G Series. In order to consistent with the behavior 
        /// of the Foundation, we remove the directory path check. However,
        /// it's not encouraged to overwrite a directory and its subdirectories.
        /// </devdoc>
        void Write(DiskStoreSection section, int scope, string path, object data);

        /// <summary>
        /// Remove data from the safe storage.
        /// </summary>
        /// <param name="section">The section to write the data to.</param>
        /// <param name="scope">The scope of the section to write the data.</param>
        /// <param name="path">The path to write the data to within the scope.</param>
        /// <returns>True if the removal is success.  False otherwise</returns>
        bool Remove(DiskStoreSection section, int scope, string path);

        /// <summary>
        /// Swap the storage lists of two safe storage scopes.
        /// </summary>
        /// <param name="section1">The section of the first storage list.</param>
        /// <param name="scope1">The scope of the first storage list.</param>
        /// <param name="section2">The section of the second storage list.</param>
        /// <param name="scope2">The scope of the second storage list.</param>
        void SwapScopes(DiskStoreSection section1, int scope1, DiskStoreSection section2, int scope2);

        /// <summary>
        /// Make a deep copy of the source storage list, and assign to the
        /// destination storage list.  The original content of the destination
        /// storage list will be discarded.
        /// </summary>
        /// <param name="sourceSection">The section of the source storage list.</param>
        /// <param name="sourceScope">The scope of the source storage list.</param>
        /// <param name="destinationSection">The section of the destination storage list.</param>
        /// <param name="destinationScope">The scope of the destination storage list.</param>
        void CopyScope(DiskStoreSection sourceSection, int sourceScope, 
            DiskStoreSection destinationSection, int destinationScope);

        /// <summary>
        /// Get the name list of items stored in a given data storage scope.
        /// </summary>
        /// <param name="section">The section to read from.</param>
        /// <param name="scope">The scope for which the manifest is requested.</param>
        /// <returns>Name list of the items stored the given scope.</returns>
        ICollection<string> GetManifest(DiskStoreSection section, int scope);

        /// <summary>
        /// Get the current usage of the specified scope in bytes.
        /// </summary>
        /// <param name="section">The section containing the scope.</param>
        /// <param name="scope">The scope to get the usage for.</param>
        /// <returns>The current usage of the specified scope in bytes.</returns>
        int GetScopeUsage(DiskStoreSection section, int scope);
    }
}