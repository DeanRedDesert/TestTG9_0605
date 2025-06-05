//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionParametersInterfaceConfiguration.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Collections.Generic;
    using F2X;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Interface Extension Configuration for the PlayerSessionParameters extended interface.
    /// </summary>
    public sealed class PlayerSessionParametersInterfaceConfiguration : F2XInterfaceExtensionConfigurationBase
    {
        #region Private Fields

        /// <summary>
        /// The flag which indicates if or not this interface extension is required.
        /// </summary>
        private readonly bool required;

        /// <summary>
        /// The flag which indicates whether this interface extension is to be used by a Shell Application.
        /// </summary>
        private readonly bool isShellApplication;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="PlayerSessionParametersInterfaceConfiguration"/>.
        /// </summary>
        /// <param name="required">
        /// The flag indicating if this extended interface is required.
        /// </param>
        /// <param name="isShellApplication">
        /// The flag which indicates whether this interface extension is to be used by a Shell Application.
        /// </param>
        public PlayerSessionParametersInterfaceConfiguration(bool required, bool isShellApplication = false)
        {
            this.required = required;
            this.isShellApplication = isShellApplication;
        }

        #endregion

        #region IInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
        {
            switch(dependencies)
            {
                case null:
                    throw new ArgumentNullException(nameof(dependencies));

                case F2XInterfaceExtensionDependencies f2XDependencies:
                    try
                    {
                        return new F2XPlayerSessionParameters(f2XDependencies.Category as IPlayerSessionParametersCategory,
                                                              f2XDependencies.TransactionWeightVerification,
                                                              f2XDependencies.TransactionalEventDispatcher,
                                                              f2XDependencies.LayeredContextActivationEvents,
                                                              f2XDependencies.GameModeQuery);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2X PlayerSessionParameters implementation from dependencies.",
                            innerException);
                    }

                case StandaloneInterfaceExtensionDependencies standaloneDependencies:
                    try
                    {
                        return new StandalonePlayerSessionParameters(standaloneDependencies.LayeredContextActivationEvents,
                                                                     standaloneDependencies.GameModeQuery,
                                                                     standaloneDependencies.TransactionalEventDispatcher,
                                                                     standaloneDependencies.EventPoster,
                                                                     standaloneDependencies.PlayStatus);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing standalone PlayerSessionParameters implementation from dependencies.",
                            innerException);
                    }

                default:
                    throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }
        }

        /// <inheritdoc/>
        public override Type InterfaceType => typeof(IPlayerSessionParameters);

        #endregion

        #region IF2XInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return new List<CategoryRequest>
                       {
                           new CategoryRequest(
                               new CategoryVersionInformation((int)MessageCategory.PlayerSessionParameters, 1, 0),
                               required,
                               FoundationTarget.AscentHSeriesCds,
                               dependencies => new PlayerSessionParametersCategory(
                                   dependencies.Transport, new PlayerSessionParametersCategoryCallbacks(dependencies.EventCallbacks))),

                           new CategoryRequest(
                               new CategoryVersionInformation((int)MessageCategory.PlayerSessionParameters, 1, 1),
                               required,
                               FoundationTarget.AscentISeriesCds.AndHigher(),
                               dependencies => new PlayerSessionParametersCategory(
                                   dependencies.Transport, new PlayerSessionParametersCategoryCallbacks(dependencies.EventCallbacks)))
                       };
        }

        /// <inheritdoc/>
        /// <devdoc>
        /// If it is a shell application, Foundation will NOT grant the category on Link level.
        /// In that case, we should NOT ask for this category on Link level, otherwise if the
        /// required flag is true, the negotiation would fail.
        /// </devdoc>
        public override CategoryNegotiationLevel NegotiationLevel => isShellApplication
                                                                         ? CategoryNegotiationLevel.Shell
                                                                         : CategoryNegotiationLevel.Link;

        #endregion
    }
}
