// -----------------------------------------------------------------------
// <copyright file = "DataAccess.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    /// <summary>
    /// The DataAccess enumeration is used to represent the
    /// different types of access to the data in CriticalDataStore.
    ///
    /// This is only used in Communication.Platform.
    /// </summary>
    internal enum DataAccess
    {
        /// <summary>
        /// The data is being read.
        /// </summary>
        Read,

        /// <summary>
        /// The data is being written.
        /// </summary>
        Write,

        /// <summary>
        /// The data is being removed.
        /// </summary>
        Remove,
    }
}
