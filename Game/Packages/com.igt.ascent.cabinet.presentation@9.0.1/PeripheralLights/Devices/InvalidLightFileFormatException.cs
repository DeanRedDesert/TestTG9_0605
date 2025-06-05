//-----------------------------------------------------------------------
// <copyright file = "InvalidLightFileFormatException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates an invalid input is read from the <see cref="LightControllerImporter"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class InvalidLightFileFormatException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Invalid light file input. {0}";

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="errorDescription">The file format error description.</param>
        public InvalidLightFileFormatException(string errorDescription)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, errorDescription))
        {
        }

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="errorDescription">The file format error description.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidLightFileFormatException(string errorDescription, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, errorDescription), innerException)
        {
        }
    }
}

