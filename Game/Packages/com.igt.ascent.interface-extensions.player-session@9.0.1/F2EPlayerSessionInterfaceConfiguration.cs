//-----------------------------------------------------------------------
// <copyright file = "F2EPlayerSessionInterfaceConfiguration.cs" company = "IGT">
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
    /// Interface Extension Configuration for the F2E PlayerSession extended interface.
    /// </summary>
    public sealed class F2EPlayerSessionInterfaceConfiguration : F2XPlayerSessionInterfaceConfigurationBase
    {
        #region Constructors

        /// <summary>
        /// Construct an instance of this interface configuration.
        /// </summary>
        /// <param name="required">
        /// The flag indicating if this extended interface is required.
        /// </param>
        public F2EPlayerSessionInterfaceConfiguration(bool required) : base(required)
        {
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
                        return new F2EPlayerSession(f2XDependencies.Category as IPlayerSessionCategory,
                            f2XDependencies.TransactionalEventDispatcher,
                            f2XDependencies.LayeredContextActivationEvents);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2E PlayerSession implementation from dependencies.", innerException);
                    }
                    
                case StandaloneInterfaceExtensionDependencies _:
                    return CreateStandalonePlayerSession(dependencies);

                default:
                    throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }
        }

        #endregion

        #region IF2XInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return GetCategoryRequests(FoundationTarget.AscentJSeriesMps.AndHigher());
        }

        #endregion
    }
}