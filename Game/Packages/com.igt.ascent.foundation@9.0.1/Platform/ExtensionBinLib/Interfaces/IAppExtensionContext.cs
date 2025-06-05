//-----------------------------------------------------------------------
// <copyright file = "IAppExtensionContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    /// <summary>
    /// This interface defines data pertaining to the current active App Extension.
    /// </summary>
    public interface IAppExtensionContext : IInnerContext
    {
        /// <summary>
        /// Gets the identity of the App Extension in the context.
        /// </summary>
        IExtensionIdentity AppExtensionIdentity { get; }
    }
}
