//-----------------------------------------------------------------------
// <copyright file = "SetupValidationResult.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the results that were detected while performing setup validation.
    /// </summary>
    public class SetupValidationResult
    {
        /// <summary>
        /// Gets the type of setup validation fault.
        /// </summary>
        public SetupValidationFaultType FaultType { get; }

        /// <summary>
        /// Gets the area of setup validation fault.
        /// </summary>
        public SetupValidationFaultArea FaultArea { get; }
    
        /// <summary>
        /// Gets the collection of <see cref="ValidationFaultLocalization"/> .
        /// </summary>
        public IEnumerable<ValidationFaultLocalization> FaultLocalizationItems { get; }

        /// <summary>
        /// Instantiates a new <see cref="SetupValidationResult"/>.
        /// </summary>
        /// <param name="type">The Type of fault that was detected.</param>
        /// <param name="area">The area affected by the detected fault.</param>
        /// <param name="items">The list of localized fault information.</param>
        public SetupValidationResult(SetupValidationFaultType type, SetupValidationFaultArea area,
            IEnumerable<ValidationFaultLocalization> items)
        {
            FaultType = type;
            FaultArea = area;
            FaultLocalizationItems = items;
        }
    }
}
