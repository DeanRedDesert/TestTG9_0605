//-----------------------------------------------------------------------
// <copyright file = "IStandaloneCriticalDataDependency.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// Interface for providing critical data access to interface extensions.
    /// </summary>
    public interface IStandaloneCriticalDataDependency
    {
        /// <summary>
        /// Write critical data which is specific to an interface extension and which may not be
        /// accessed from the game.
        /// </summary>
        /// <param name="scope">The scope to write the data to.</param>
        /// <param name="path">
        /// The path in the specified scope to write the data. Consumers of this interface should provide a unique
        /// prefix to their data.
        /// </param>
        /// <param name="data">The data to write in the specified scope at the specified path.</param>
        void WriteFoundationData(InterfaceExtensionDataScope scope, string path, object data);


        /// <summary>
        /// Read critical data which is specific to an interface extension and which may not be
        /// accessed from the game.
        /// </summary>
        /// <typeparam name="TData">The type of the data being read.</typeparam>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path the data is stored at.</param>
        /// <returns>Value read from the specified scope and path.</returns>
        TData ReadFoundationData<TData>(InterfaceExtensionDataScope scope, string path);
    }
}
