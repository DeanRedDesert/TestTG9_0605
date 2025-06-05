//-----------------------------------------------------------------------
// <copyright file = "CabinetDoesNotSupportLightsInterfaceException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;

    /// <summary>
    /// Thrown if the cabinet library does not support the peripheral lights interface.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class CabinetDoesNotSupportLightsInterfaceException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="interfaceName">The name of the missing interface.</param>
        public CabinetDoesNotSupportLightsInterfaceException(string interfaceName):
            base($"The cabinet library does not support the {interfaceName} interface.")
        {

        }
    }
}
