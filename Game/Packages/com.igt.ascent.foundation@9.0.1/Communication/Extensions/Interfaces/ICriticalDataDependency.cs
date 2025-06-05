// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataDependency.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// This interface defines APIs for interface extensions to access critical data.
    /// </summary>
    /// <devdoc>
    /// By far the only usage is writing/reading a single piece of data.
    /// New APIs can be added if writing/reading multiple pieces of data at a time is needed.
    /// </devdoc>
    public interface ICriticalDataDependency
    {
        /// <summary>
        /// Writes critical data which is specific to an interface extension.
        /// </summary>
        /// <typeparam name="TData">
        /// The type of the data being written.
        /// </typeparam>
        /// <param name="scope">
        /// The scope to write the data to.
        /// </param>
        /// <param name="path">
        /// The path in the specified scope to write the data.
        /// The caller must provide a unique path, e.g. prefixing the path with a unique string.
        /// </param>
        /// <param name="data">
        /// The data to write in the specified scope at the specified path.
        /// </param>
        void WriteCriticalData<TData>(InterfaceExtensionDataScope scope, string path, TData data);

        /// <summary>
        /// Reads critical data which is specific to an interface extension.
        /// </summary>
        /// <typeparam name="TData">
        /// The type of the data being read.
        /// </typeparam>
        /// <param name="scope">
        /// The scope to read the data from.
        /// </param>
        /// <param name="path">
        /// The path the data is stored at.
        /// The caller must provide a unique path, e.g. prefixing the path with a unique string.
        /// </param>
        /// <returns>
        /// Value read from the specified scope and path.
        /// </returns>
        TData ReadCriticalData<TData>(InterfaceExtensionDataScope scope, string path);
    }
}