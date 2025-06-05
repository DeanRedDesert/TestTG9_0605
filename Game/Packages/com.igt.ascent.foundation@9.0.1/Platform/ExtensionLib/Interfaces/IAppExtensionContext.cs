//-----------------------------------------------------------------------
// <copyright file = "IAppExtensionContext.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines data pertaining to an app extension's context.
    /// </summary>
    public interface IAppExtensionContext
    {
        /// <summary>
        /// Gets the Guid of the app extension in the context.
        /// </summary>
        Guid AppExtensionGuid { get; }
    }
}
