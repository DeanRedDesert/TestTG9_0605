//-----------------------------------------------------------------------
// <copyright file = "ConfigurationEditorBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;

    /// <summary>
    /// Defines the base class for the configuration editor.
    /// </summary>
    /// <typeparam name="TConfig">The corresponding configuration type.</typeparam>
    public abstract class ConfigurationEditorBase<TConfig> where TConfig : class, ICloneable, new()
    {
        /// <summary>
        /// Cached configuration for this editor.
        /// </summary>
        protected TConfig CachedConfig = new TConfig();

        /// <summary>
        /// Gets or sets the flag indicating if the configuration needs to be supported.
        /// </summary>
        protected bool Supported { get; set; }

        /// <summary>
        /// Imports the configuration settings to this editor.
        /// </summary>
        /// <param name="config">The configuration settings to import.</param>
        public virtual void ImportConfig(TConfig config)
        {
            if(config != null)
            {
                Supported = true;
                CachedConfig = config.Clone() as TConfig;
            }
            else
            {
                Supported = false;
            }
        }

        /// <summary>
        /// Exports the configuration settings from this editor.
        /// </summary>
        /// <returns>The exported configuration settings.</returns>
        public virtual TConfig ExportConfig()
        {
            return Supported ? CachedConfig.Clone() as TConfig : null;
        }

        /// <summary>
        /// When implemented in a derived class, update the configuration settings in Unity editor.
        /// </summary>
        /// <param name="configId">The configuration Id to show up in the editor.</param>
        public abstract void UpdateConfig(string configId);
    }
}
