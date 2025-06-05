// -----------------------------------------------------------------------
// <copyright file = "CoupledLightException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;

    /// <summary>
    /// This exception indicates that there was an error when requesting a coupled light device. 
    /// </summary>
    public class CoupledLightException : Exception
    {
        /// <summary>
        /// Construct a new instance of the exception.
        /// </summary>
        public CoupledLightException(string message)
            : base(message)
        {
        }
    }
}