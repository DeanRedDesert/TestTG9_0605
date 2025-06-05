// -----------------------------------------------------------------------
// <copyright file = "LightTypeNotCompatibleException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;

    /// <summary>
    /// Exception to be thrown when an incompatible light type is created. 
    /// </summary>
    public class LightTypeNotCompatibleException : Exception
    {
        /// <summary>
        /// Get the type of the light object that was created.
        /// </summary>
        public Type CreatedType { get; }

        /// <summary>
        /// Get the hardware that the light object was created for.
        /// </summary>
        public Hardware SpecifiedHardware { get; }

        private const string ExceptionFormatString =
            "Created light type '{0}' is not compatible with the specified hardware '{1}'.";

        /// <summary>
        /// Create an instance of <see cref="LightTypeNotCompatibleException"/>.
        /// </summary>
        /// <param name="createdType">Created type made for the hardware.</param>
        /// <param name="specifiedHardware">Specified hardware to make a light device for.</param>
        public LightTypeNotCompatibleException(Type createdType, Hardware specifiedHardware) 
            : base(string.Format(ExceptionFormatString, createdType, createdType))
        {
            CreatedType = createdType;
            SpecifiedHardware = specifiedHardware;
        }
    }
}