// -----------------------------------------------------------------------
// <copyright file = "ISystemExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines functionality available for a System Extension to communicate with the Foundation.
    /// </summary>
    /// <remarks>
    /// A System Extension is an executable extension that extends the functionality of Foundation.
    /// It often can access functionality that is not available to games and app extensions.
    /// </remarks>
    public interface ISystemExtensionLib : IInnerLib
    {
        #region Events

        /// <summary>
        /// Occurs when a System Extension Context is activated.
        /// </summary>
        event EventHandler<ActivateInnerContextEventArgs<ISystemExtensionContext>> ActivateContextEvent;

        /// <summary>
        /// Occurs when a System Extension Context is inactivated.
        /// </summary>
        event EventHandler<InactivateInnerContextEventArgs<ISystemExtensionContext>> InactivateContextEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current System Extension Context.
        /// </summary>
        ISystemExtensionContext Context { get; }

        #endregion
    }
}