//-----------------------------------------------------------------------
// <copyright file = "TiltManagementInterfaceConfiguration.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement
{
    using System;
    using System.Collections.Generic;
    using Communication;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Configuration for the identification extended interface.
    /// </summary>
    public class TiltManagementInterfaceConfiguration : F2XInterfaceExtensionConfigurationBase
    {
        #region Private Fields

        /// <summary>
        /// Flag which indicates if the interface for this configuration is required.
        /// </summary>
        private readonly bool required;

        #endregion

        #region Private Constants

        /// <summary>
        /// The requested major version of the underlying F2L category.
        /// </summary>
        private const int RequestedMajorVersion = 1;

        /// <summary>
        /// The requested minor version of the underlying F2L category.
        /// </summary>
        private const int RequestedMinorVersion = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct an instance of the configuration.
        /// </summary>
        /// <param name="required">Flag indicating if this extended interface is mandatory.</param>
        public TiltManagementInterfaceConfiguration(bool required)
        {
            this.required = required;
        }

        #endregion

        #region IInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
        {
            if(dependencies != null)
            {
                var f2XDependencies = dependencies as F2XInterfaceExtensionDependencies;
                if(f2XDependencies != null)
                {
                    try
                    {
                        return new TiltManager(f2XDependencies.Category as IGameTiltCategory,
                                               f2XDependencies.CriticalDataProvider,
                                               f2XDependencies.TransactionWeightVerification,
                                               f2XDependencies.LayeredContextActivationEvents,
                                               f2XDependencies.CultureInfoProvider,
                                               f2XDependencies.GameModeQuery);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2L tilt manager implementation from dependencies.", innerException);
                    }
                }

                var standaloneDependencies = dependencies as StandaloneInterfaceExtensionDependencies;
                if(standaloneDependencies != null)
                {
                    try
                    {
                        return new TiltManager(null,
                                               standaloneDependencies.CriticalDataProvider,
                                               standaloneDependencies.TransactionWeightVerification,
                                               standaloneDependencies.LayeredContextActivationEvents,
                                               standaloneDependencies.CultureInfoProvider,
                                               standaloneDependencies.GameModeQuery);
                    }
                    catch(Exception e)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing standalone tilt manager implementation from dependencies.", e);
                    }
                }

                throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }

            throw new ArgumentNullException("dependencies");
        }


        /// <inheritdoc/>
        public override Type InterfaceType
        {
            get { return typeof(ITiltManagement); }
        }

        #endregion

        #region IF2XInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return new List<CategoryRequest>
                       {
                           // The GameTilt message category is intended for foundation target Phase 2.5 and beyond.
                           new CategoryRequest(
                               new CategoryVersionInformation((int)MessageCategory.GameTilt,
                                                              RequestedMajorVersion,
                                                              RequestedMinorVersion),
                               required,
                               FoundationTarget.All,
                               dependencies => new F2LGameTiltCategory(dependencies.Transport))
                       };
        }

        #endregion
    }
}