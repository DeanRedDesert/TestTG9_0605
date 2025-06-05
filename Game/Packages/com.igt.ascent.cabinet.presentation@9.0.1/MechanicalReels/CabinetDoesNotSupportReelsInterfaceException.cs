//-----------------------------------------------------------------------
// <copyright file = "CabinetDoesNotSupportReelsInterfaceException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;

    /// <summary>
    /// Thrown if the cabinet library does not support the mechanical reels interface.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class CabinetDoesNotSupportReelsInterfaceException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        public CabinetDoesNotSupportReelsInterfaceException() :
            base("The cabinet library does not support the IMechanicalReels interface.")
        {
        }
    }
}
