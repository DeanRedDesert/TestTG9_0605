// -----------------------------------------------------------------------
// <copyright file = "ISetupValidationServiceHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// This interface defines the service handler for Setup Validation Reporting Service.
    /// </summary>
    public interface ISetupValidationServiceHandler : IReportResourceCleanUp
    {
        /// <summary>
        /// Generates the result for setup validation reporting service.
        /// </summary>
        /// <param name="setupValidationContext">
        /// The context used to validate the setup.
        /// </param>
        /// <returns>
        /// The collection of <see cref="SetupValidationResult"/>.
        /// </returns>
        IEnumerable<SetupValidationResult> ValidateThemeSetup(SetupValidationContext setupValidationContext);
    }
}