//-----------------------------------------------------------------------
// <copyright file = "F2XPlayerSessionInterfaceConfigurationBase.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using System.Collections.Generic;
    using F2X;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Base implementation of the Interface Extension Configuration for PlayerSession extended interface.
    /// </summary>
    public abstract class F2XPlayerSessionInterfaceConfigurationBase : F2XInterfaceExtensionConfigurationBase
    {
        #region Protected Fields

        /// <summary>
        /// The flag which indicates if or not the interface for this configuration is required.
        /// </summary>
        private readonly bool required;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates an instance of type <see cref="StandaloneInterfaceExtensionDependencies"/>
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        protected IInterfaceExtension CreateStandalonePlayerSession(IInterfaceExtensionDependencies dependencies)
        {
            if(dependencies is StandaloneInterfaceExtensionDependencies standaloneDependencies)
            {
                try
                {
                    return new StandalonePlayerSession(standaloneDependencies.LayeredContextActivationEvents,
                                                       standaloneDependencies.GameModeQuery,
                                                       standaloneDependencies.TransactionalEventDispatcher,
                                                       standaloneDependencies.EventPoster,
                                                       standaloneDependencies.PlayStatus);
                }
                catch(Exception innerException)
                {
                    throw new InterfaceExtensionDependencyException(
                        "Issue constructing standalone PlayerSession implementation from dependencies.", innerException);
                }
            }

            throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
        }

        /// <summary>
        /// Gets a collection of category-version requests needed by the interface extension.
        /// </summary>
        /// <param name="target">
        /// The foundation targeted by the category request.
        /// </param>
        /// <returns>
        /// All the supported versions for a specific category required by the interface extension.
        /// </returns>
        protected IEnumerable<CategoryRequest> GetCategoryRequests(FoundationTarget target)
        {
            return new List<CategoryRequest>
                       {
                           new CategoryRequest(
                               new CategoryVersionInformation((int)MessageCategory.PlayerSession, 1, 0),
                               required,
                               target,
                               dependencies => new PlayerSessionCategory(
                                   dependencies.Transport, new PlayerSessionCategoryCallbacks(dependencies.EventCallbacks)))
                       };
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an instance of this interface configuration.
        /// </summary>
        /// <param name="required">
        /// The flag indicating if this extended interface is required.
        /// </param>
        protected F2XPlayerSessionInterfaceConfigurationBase(bool required)
        {
            this.required = required;
        }

        #endregion

        #region IInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override Type InterfaceType => typeof(IPlayerSession);

        #endregion
    }
}