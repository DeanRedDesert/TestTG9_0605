//-----------------------------------------------------------------------
// <copyright file = "F2XRegistryLookup.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Registries.Internal.F2X
{
    using System;
    using System.Collections.Generic;
    using F2XRegistryVer1;

    /// <summary>
    /// A static class that maps the generated registry class types to their equivalent type enumerations.
    /// </summary>
    public static class F2XRegistryLookup
    {
        /// <summary>
        /// Dictionary mapping registry types to their enumerations.
        /// </summary>
        private static readonly Dictionary<Type, RegistryType> LookupDictionary =
                            new Dictionary<Type, RegistryType>

            {{typeof(F2XAppRegistryVer1.AppRegistry),
                     RegistryType.App},
             {typeof(F2XBaseExtensionInterfaceDefinitionRegistryVer1.ExtensionInterfaceDefinitionRegistry),
                    RegistryType.ExecutableExtension},
             {typeof(F2XBaseExtensionRegistryVer1.BaseExtensionRegistry),
                     RegistryType.ExecutableExtension},
             {typeof(F2XBetStepsRegistryVer1.BetStepsRegistry),
                    RegistryType.BetSteps},
             {typeof(F2XCdsBingoRegistryVer1.CdsBingoRegistry),
                     RegistryType.CdsBingo},
             {typeof(F2XCdsIcdRegistryVer1.CdsIcdRegistry),
                     RegistryType.CdsICD},             
             {typeof(F2XCdsPullTabRegistryVer1.CdsPullTabRegistry),
                     RegistryType.CdsPullTab},
             {typeof(F2XChooserRegistryVer1.ChooserRegistry),
                     RegistryType.Chooser},
             {typeof(F2XConfigurationExtensionInterfaceDefinitionRegistryVer1.ConfigurationExtensionInterfaceDefinitionRegistry),
                     RegistryType.ConfigurationExtensionInterfaceDefinition},
             {typeof(F2XConfigurationExtensionRegistryVer1.ConfigurationExtensionRegistry),
                    RegistryType.ConfigurationExtension},
             {typeof(F2XExecutableExtensionBinRegistryVer1.ExecutableExtensionBinRegistry),
                     RegistryType.ExecutableExtensionBin},
             {typeof(F2XExecutableExtensionRegistryVer1.ExecutableExtensionRegistry),
                     RegistryType.ExecutableExtension},
             {typeof(F2XMarketConfigurationDataVer1.MarketConfigurationData),
                    RegistryType.MarketConfigurationData},
             {typeof(F2XMarketRestrictionInterfaceDefinitionRegistryVer1.MarketRestrictionInterfaceDefinitionRegistry),
                     RegistryType.MarketRestrictionInterfaceDefinition},
             {typeof(F2XMarketRestrictionVer1.MarketRestriction),
                     RegistryType.MarketRestriction},
             {typeof(F2XMenuExtensionRegistryVer1.MenuExtensionRegistry),
                     RegistryType.MenuExtension},
             {typeof(F2XNetProgressiveControllerGroupRegistryVer1.NetProgressiveGroupRegistry),
                    RegistryType.NetProgressiveControllerGroup},
             {typeof(F2XPatchExtensionRegistryVer1.PatchExtensionRegistry),
                    RegistryType.PatchExtension},
             {typeof(F2XPayvarGroupRegistryVer1.PayvarGroupRegistry),
                     RegistryType.PayvarGroup},
             {typeof(F2XPlatformPackageDescriptorRegistryVer1.PlatformPackageDescriptor),
                     RegistryType.PlatformPackageDescriptor},
             {typeof(F2XProgressiveLinkRegistryVer1.ProgressiveLinkRegistry),
                     RegistryType.ProgressiveLink},
             {typeof(F2XRegistryMetadataVer1.RegistryMetadata),
                     RegistryType.RegistryMetadata},
             {typeof(F2XReportRegistryVer1.ReportRegistry),
                     RegistryType.Report},
             {typeof(F2XReportRegistryVer2.ReportRegistry),
                     RegistryType.Report},
             {typeof(F2XResourceExtensionRegistryVer1.ResourceExtensionRegistry),
                     RegistryType.ResourceExtension},
             {typeof(F2XShellRegistryVer1.ShellRegistry),
                     RegistryType.Shell},
             {typeof(F2XSpcGroupRegistryVer1.SpcGroupRegistry),
                     RegistryType.SpcGroup},
             {typeof(F2XVideoTopperRegistryVer1.VideoTopperRegistry),
                     RegistryType.VideoTopper},
             {typeof(F2XCdsEcpRegistryVer1.CdsEcpRegistry),
                 RegistryType.CdsECP}};

        /// <summary>
        /// Checks if the registry type enum and the registry class types match. Note that the type
        /// enum is unversioned and defined in a different namespace than the versioned actual class
        /// implementation. 
        /// </summary>
        /// <param name="registryType">The <see cref="Type"/> of the registry class.</param>
        /// <param name="registryTypeEnum">The <see cref="RegistryType"/> enumeration to check for a match.</param>
        /// <returns>Flag indicating that the type lookup succeeded.</returns>
        public static bool TypesMatch(Type registryType, RegistryType registryTypeEnum)
        {
            return LookupDictionary.ContainsKey(registryType) && LookupDictionary[registryType] == registryTypeEnum;
        }
    }
}