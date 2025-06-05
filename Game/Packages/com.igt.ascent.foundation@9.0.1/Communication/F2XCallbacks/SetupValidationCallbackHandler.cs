//-----------------------------------------------------------------------
// <copyright file = "SetupValidationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.SetupValidation;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Setup Validation API category.
    /// </summary>
    internal class SetupValidationCallbackHandler : ISetupValidationCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="SetupValidationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        internal SetupValidationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            if(eventCallbacks == null)
            {
                throw new ArgumentNullException("eventCallbacks");
            }

            this.eventCallbacks = eventCallbacks;
        }

        #region ISetupValidationCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessValidateThemeSetup(ThemeIdentifier theme, out IEnumerable<ValidationFault> callbackResult)
        {
            var validateThemeSetupEventArgs = new ValidateThemeSetupEventArgs(theme.Value);

            // Post event for report object to handle.
            eventCallbacks.PostEvent(validateThemeSetupEventArgs);

            // Process the return value of the event handling.
            var validateThemeSetupResults = validateThemeSetupEventArgs.ValidationResults;
            var errorMessage = validateThemeSetupEventArgs.ErrorMessage;

            if(validateThemeSetupResults != null)
            {
                callbackResult = validateThemeSetupResults.Select(
                    setupValidationResult => new ValidationFault
                    {
                        Type = (FaultType)setupValidationResult.FaultType,
                        Area = (FaultArea)setupValidationResult.FaultArea,
                        Messages = setupValidationResult.FaultLocalizationItems.Select(
                            item => new ValidationFaultLocalization
                            {
                                Culture = item.Culture,
                                Message = item.Message,
                                Title = item.Title
                            }
                        ).ToList()
                    });
            }
            // Setup validation may return null in case that no validation fault is found.
            else
            {
                callbackResult = new List<ValidationFault>();
            }

            return errorMessage;
        }

        #endregion
    }
}
