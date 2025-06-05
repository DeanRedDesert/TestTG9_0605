// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataStoreAccessValidator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using Interfaces;

    /// <summary>
    /// This interface defines the methods for validating the data access to the key-value store.
    /// </summary>
    internal interface ICriticalDataStoreAccessValidator
    {
        /// <summary>
        /// Validates the access to the critical data store, such as ShellStore, ThemeStore, etc.
        /// </summary>
        /// <param name="dataAccess">
        /// Specifies the access type, e.g. Read/Write/Remove.
        /// </param>
        /// <param name="storeName">
        /// Specifies the critical data store name.  This is useful when composing the exception message.
        /// </param>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the critical data store is not accessible.
        /// </exception>
        void Validate(DataAccess dataAccess, string storeName);
    }
}
