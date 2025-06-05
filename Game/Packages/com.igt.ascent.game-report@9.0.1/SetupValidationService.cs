// -----------------------------------------------------------------------
// <copyright file = "SetupValidationService.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;
    using Logging;

    /// <summary>
    /// This class implements the <see cref="IReportingService"/> for setup validation
    /// reporting service.
    /// </summary>
    internal class SetupValidationService : IReportingService
    {
        #region Private Fields

        /// <summary>
        /// The service handler for setup validation reporting service.
        /// </summary>
        private readonly ISetupValidationServiceHandler setupValidationServiceHandler;

        #endregion

        #region Contructors

        /// <summary>
        /// Instantiates a new <see cref="SetupValidationService"/>.
        /// </summary>
        /// <param name="setupValidationServiceHandler">
        /// Service handler for setup validation reporting service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="setupValidationServiceHandler"/> is null.
        /// </exception>
        public SetupValidationService(ISetupValidationServiceHandler setupValidationServiceHandler)
        {
            this.setupValidationServiceHandler = setupValidationServiceHandler ?? throw new ArgumentNullException(nameof(setupValidationServiceHandler));
        }

        #endregion

        #region IReportingService Members

        /// <inheritdoc/>
        public ReportingServiceType ReportingServiceType => ReportingServiceType.SetupValidation;

        /// <inheritdoc/>
        public void RegisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.ValidateThemeSetupEvent += HandleValidateThemeSetup;
        }

        /// <inheritdoc/>
        public void UnregisterReportLibEvents(IReportLib reportLib)
        {
            reportLib.ValidateThemeSetupEvent -= HandleValidateThemeSetup;
        }

        /// <inheritdoc/>
        public void CleanUpResources(IReportLib reportLib)
        {
            setupValidationServiceHandler.CleanUpResources(reportLib);

            if(setupValidationServiceHandler is IDisposable disposableServiceHandler)
            {
                disposableServiceHandler.Dispose();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generates a <see cref="IEnumerable{T}"/> collection of the <see cref="SetupValidationResult"/> objects to use.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">Generate setup validation report event data.</param>
        private void HandleValidateThemeSetup(object sender, ValidateThemeSetupEventArgs eventArgs)
        {
            try
            {
                eventArgs.ValidationResults =
                    setupValidationServiceHandler.ValidateThemeSetup(new SetupValidationContext(eventArgs.ThemeIdentifier));
            }
            catch(Exception exception)
            {
                eventArgs.ValidationResults = null;
                eventArgs.ErrorMessage = exception.ToString();
                Log.WriteWarning(exception.ToString());
            }
        }

        #endregion
    }
}
