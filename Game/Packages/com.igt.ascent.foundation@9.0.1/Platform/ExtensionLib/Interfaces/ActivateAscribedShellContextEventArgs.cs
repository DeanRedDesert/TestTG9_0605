// -----------------------------------------------------------------------
// <copyright file = "ActivateAscribedShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;

    /// <summary>
    /// Event indicating the extension should activate a new AscribedShell context.
    /// </summary>
    /// <devdoc>
    /// This will only be sent after a new AscribedShell context message has been sent.
    /// </devdoc>
    public class ActivateAscribedShellContextEventArgs : AscribedShellContextEventArgs
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ActivateAscribedShellContextEventArgs"/> class.
        /// </summary>
        /// <param name="shellEntity">
        /// The <see cref="AscribedGameEntity"/> for the ascribed shell. 
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="shellEntity"/> is not an ascribed shell entity.
        /// </exception>
        public ActivateAscribedShellContextEventArgs(AscribedGameEntity shellEntity = null) : base(shellEntity)
        {
        }
    }
}
