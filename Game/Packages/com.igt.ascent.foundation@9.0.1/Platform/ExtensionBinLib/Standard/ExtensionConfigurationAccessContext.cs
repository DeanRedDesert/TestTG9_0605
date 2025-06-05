// -----------------------------------------------------------------------
// <copyright file = "ExtensionConfigurationAccessContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of <see cref="IConfigurationAccessContext"/> to be used by Extension clients.
    /// </summary>
    internal sealed class ExtensionConfigurationAccessContext : IConfigurationAccessContext
    {
        #region Implementation of IConfigurationAccessContext

        /// <inheritdoc />
        public bool IsConfigurationScopeIdentifierRequired => true;

        /// <inheritdoc />
        public void ValidateConfigurationAccess(ConfigurationScope configurationScope)
        {
            if(configurationScope != ConfigurationScope.Extension)
            {
                throw new ConfigurationAccessDeniedException(
                    $"Configuration data is not accessible for {configurationScope} scope.");
            }
        }

        #endregion
    }
}