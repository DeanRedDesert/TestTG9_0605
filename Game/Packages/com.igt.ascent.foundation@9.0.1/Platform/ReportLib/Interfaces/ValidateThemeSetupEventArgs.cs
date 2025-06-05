//-----------------------------------------------------------------------
// <copyright file = "ValidateThemeSetupEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Game.Core.GameReport.Interfaces;

    /// <summary>
    /// The arguments and result for the request of validating a theme's setup.
    /// </summary>
    public class ValidateThemeSetupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the theme identifier to validate the setup.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets or sets the validation results.
        /// </summary>
        public IEnumerable<SetupValidationResult> ValidationResults { get; set; }

        /// <summary>
        /// Gets or sets the error message if a report could not be generated.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="ValidateThemeSetupEventArgs"/>.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme to validate the specific theme's setup.</param>
        public ValidateThemeSetupEventArgs(string themeIdentifier)
        {
            ThemeIdentifier = themeIdentifier;
        }
    }
}
