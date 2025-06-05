// -----------------------------------------------------------------------
// <copyright file = "RuntimeGameEventsInterfaceConfiguration.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    using System;
    using System.Collections.Generic;
    using F2X;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Interface Extension Configuration for the runtime game events extended interface.
    /// </summary>
    public class RuntimeGameEventsInterfaceConfiguration: F2XInterfaceExtensionConfigurationBase
    {
        /// <summary>
        /// The flag which indicates if or not this interface extension is required.
        /// </summary>
        private readonly bool required;

        /// <summary>
        /// Initializes an instance of <see cref="RuntimeGameEventsInterfaceConfiguration"/>.
        /// </summary>
        /// <param name="required">
        /// The flag indicating if this extended interface is required.
        /// </param>
        public RuntimeGameEventsInterfaceConfiguration(bool required)
        {
            this.required = required;
        }

        #region Overrides of F2XInterfaceExtensionConfigurationBase

        /// <inheritdoc />
        public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
        {
            switch(dependencies)
            {
                case null:
                    throw new ArgumentNullException(nameof(dependencies));
                case F2XInterfaceExtensionDependencies f2XDependencies:
                    try
                    {
                        return new F2XRuntimeGameEvents(
                            f2XDependencies.Category as IRuntimeGameEventsCategory,
                            f2XDependencies.LayeredContextActivationEvents,
                            f2XDependencies.GameModeQuery);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2X RuntimeGameEvents implementation from dependencies.",
                            innerException);
                    }
                    
                case StandaloneInterfaceExtensionDependencies _:
                    try
                    {
                        return new StandaloneRuntimeGameEvents();
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing standalone RuntimeGameEvents implementation from dependencies.",
                            innerException);
                    }
                    
                default:
                    throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }
        }

        /// <inheritdoc />
        public override Type InterfaceType => typeof(IRuntimeGameEvents);

        /// <inheritdoc />
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return new List<CategoryRequest>
            {
                new CategoryRequest(
                    new CategoryVersionInformation((int)MessageCategory.RuntimeGameEvents, 1, 0),
                    required,
                    FoundationTarget.AscentR2Series,
                    dependencies => new RuntimeGameEventsCategory(
                        dependencies.Transport))
            };
        }

        #endregion
    }
}
