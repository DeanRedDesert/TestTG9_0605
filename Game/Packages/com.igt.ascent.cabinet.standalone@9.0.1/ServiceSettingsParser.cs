// -----------------------------------------------------------------------
// <copyright file = "ServiceSettingsParser.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using IGT.Game.Core.Communication.Standalone.Schemas;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.Serialization;


    /// <summary>
    /// This class retrieves Servuce settings
    /// by parsing an xml element that contains the needed information.
    /// </summary>
    internal class ServiceSettingsParser
    {
        /// <summary>
        /// Indicate if Settings are Available
        /// </summary>
        public bool SettingsAvailable;

        /// <summary>
        /// Indicate whether to prompt player on cash out
        /// </summary>
        public bool PromptPlayerOnCashout;

        /// <summary>
        /// Cache Emulatable Buttons from parser
        /// </summary>
        public IReadOnlyList<EmulatableButton> EmulatableButtons;

        /// <summary>
        /// Initialize a new instance of Service Settings Parser using 
        /// an xml element that contains the needed information for parsing.
        /// Also Convert ServiceSettingsConfigEmulatableButton to EmulatableButton
        /// </summary>
        /// <param name="settingsElement">
        /// An xml element that contains the needed information.
        /// </param>
        public ServiceSettingsParser(XElement settingsElement)
        {
            if(settingsElement != null)
            {
                SettingsAvailable = true;
                var serializer = new XmlSerializer(typeof(ServiceSettingsConfig),
                                                   new XmlRootAttribute("ServiceSettings"));
                var serviceSettingsConfig = (ServiceSettingsConfig)serializer.Deserialize(settingsElement.CreateReader());
                PromptPlayerOnCashout = serviceSettingsConfig.PromptPlayerOnCashout;
                
                if(serviceSettingsConfig.EmulatableButtons?.Count > 0)
                {
                    var temp = new List<EmulatableButton>();
                    foreach(var item in serviceSettingsConfig.EmulatableButtons)
                    {
                        switch(item)
                        {
                            case ServiceSettingsConfigEmulatableButton.Cashout:
                                temp.Add(EmulatableButton.Cashout);
                                break;
                            case ServiceSettingsConfigEmulatableButton.Service:
                                temp.Add(EmulatableButton.Service);
                                break;
                        }
                    }
                    EmulatableButtons = temp;
                }
                else
                {
                    EmulatableButtons = new List<EmulatableButton>();
                }
            }
        }
    }
}
