// -----------------------------------------------------------------------
// <copyright file = "MonitorSettingsParser.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Communication.Standalone.Schemas;

    /// <summary>
    /// This class retrieves monitor settings by parsing an xml element that contains the needed information.
    /// </summary>
    internal class MonitorSettingsParser
    {
        /// <summary>
        /// Monitor settings config as the parsing result.
        /// </summary>
        private readonly MonitorSettingsConfig monitorSettingsConfig;

        /// <summary>
        /// Get the list of monitor configurations.
        /// </summary>
        public IList<MonitorType> MonitorsList => monitorSettingsConfig?.Monitors;

        /// <summary>
        /// Initialize a new instance of Monitor Settings Parser using
        /// an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="settingsElement">
        /// An xml element that contains the Cabinet settings.
        /// </param>
        public MonitorSettingsParser(XElement settingsElement)
        {
            if(settingsElement != null)
            {
                var serializer = new XmlSerializer(typeof(MonitorSettingsConfig), 
                                                   new XmlRootAttribute("MonitorSettings"));
                monitorSettingsConfig = (MonitorSettingsConfig)serializer.Deserialize(settingsElement.CreateReader());
            }
        }
    }
}
