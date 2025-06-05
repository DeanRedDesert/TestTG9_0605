// -----------------------------------------------------------------------
// <copyright file = "CabinetServiceTypeException.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// Thrown when a cabinet service type is invalid (e.g. isn't an interface).
    /// </summary>
    [Serializable]
    public class CabinetServiceTypeException : Exception
    {
        /// <summary>
        /// Gets the invalid service's type.
        /// </summary>
        public Type Service { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal CabinetServiceTypeException(Type service)
            : base($"Service type {service} is not an interface.")
        {
            Service = service;
        }
    }
}