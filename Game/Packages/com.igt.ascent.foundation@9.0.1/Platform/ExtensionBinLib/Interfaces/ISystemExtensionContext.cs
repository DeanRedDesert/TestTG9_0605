// -----------------------------------------------------------------------
// <copyright file = "ISystemExtensionContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines data pertaining to System Extensions currently in service.
    /// </summary>
    public interface ISystemExtensionContext : IInnerContext
    {
        /// <summary>
        /// Gets the list of System Extensions that the Extension Bin is to provide, i.e.
        /// the services that the Extension Bin claimed to support, and “won” the right to support.
        /// </summary>
        IReadOnlyList<IExtensionIdentity> ExtensionsInService { get; }
    }
}