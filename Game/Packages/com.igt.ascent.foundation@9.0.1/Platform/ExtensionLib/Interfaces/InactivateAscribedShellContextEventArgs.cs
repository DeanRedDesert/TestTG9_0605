// -----------------------------------------------------------------------
// <copyright file = "InactivateAscribedShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;

    /// <summary>
    /// Event indicating the extension should inactivate the current AscribedShell context.
    /// </summary>
    /// <devdoc>
    /// After this message, the extension must be ready to receive a NewAscribedShellContext message 
    /// or receive a GetAscribedShellApiVersions message for re-negotiation.
    /// </devdoc>
    public class InactivateAscribedShellContextEventArgs : AscribedShellContextEventArgs
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="InactivateAscribedShellContextEventArgs"/> class.
        /// </summary>
        /// <param name="shellEntity">
        /// The <see cref="AscribedGameEntity"/> for the ascribed shell. 
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="shellEntity"/> is not an ascribed shell entity.
        /// </exception>
        public InactivateAscribedShellContextEventArgs(AscribedGameEntity shellEntity = null) : base(shellEntity)
        {
        }
    }
}
