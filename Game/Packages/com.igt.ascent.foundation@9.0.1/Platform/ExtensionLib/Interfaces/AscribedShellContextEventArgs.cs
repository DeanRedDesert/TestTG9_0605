// -----------------------------------------------------------------------
// <copyright file = "AscribedShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for ascribed shell events.
    /// </summary>
    public abstract class AscribedShellContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the identifier of the ascribed shell.
        /// </summary>
        public string AscribedShellIdentifier { get; }

        /// <summary>
        /// When overridden, initialize a new instance of the <see cref="AscribedShellContextEventArgs"/> class.
        /// </summary>
        /// <param name="shellEntity">
        /// The <see cref="AscribedGameEntity"/> for the ascribed shell. 
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="shellEntity"/> is not an ascribed shell entity.
        /// </exception>
        protected AscribedShellContextEventArgs(AscribedGameEntity shellEntity)
        {
            if(shellEntity != null && shellEntity.AscribedGameType != AscribedGameType.Shell)
            {
                throw new ArgumentException("The ascribed game type must be Shell to get a shell identifier.");
            }
            AscribedShellIdentifier = shellEntity?.AscribedGameIdentifier;
        }
    }
}
