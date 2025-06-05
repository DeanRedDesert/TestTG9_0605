// -----------------------------------------------------------------------
// <copyright file = "ButtonPanelSettingsParser.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Standalone = Communication.Standalone.Schemas;

    /// <summary>
    /// This class retrieves button panel settings by parsing an xml element that contains the needed information.
    /// </summary>
    internal class ButtonPanelSettingsParser
    {
        /// <summary>
        /// Button panels of the button panel settings parser.
        /// </summary>
        private readonly IList<Standalone.ButtonPanel> buttonPanels;

        /// <summary>
        /// Cache the button panel configurations of button panel settings parser.
        /// </summary>
        private List<IButtonPanelConfiguration> buttonPanelConfigurations;

        /// <summary>
        /// Dictionary to mapping between ButtonPanelLocation and DeviceId.
        /// </summary>
        private readonly Dictionary<ButtonPanelLocation, string> locationMapping =
            new Dictionary<ButtonPanelLocation, string>
            {
            { ButtonPanelLocation.Main, "MAIN BUTTON PANEL" },
            { ButtonPanelLocation.Secondary, "SECONDARY BUTTON PANEL" },
            { ButtonPanelLocation.Unknown, "NO DEVICE ID" }
        };

        /// <summary>
        /// Initializes a new instance of Button Panel Settings Parser using
        /// an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="settingsElement">
        /// An xml element that contains the Cabinet settings.
        /// </param>
        public ButtonPanelSettingsParser(XElement settingsElement)
        {
            if(settingsElement != null)
            {
                var serializer = new XmlSerializer(typeof(Standalone.ButtonPanelSettingsConfig),
                                                   new XmlRootAttribute("ButtonPanelSettings"));
                var buttonPanelSettingsConfig = (Standalone.ButtonPanelSettingsConfig)serializer.Deserialize(settingsElement.CreateReader());
                buttonPanels = buttonPanelSettingsConfig?.ButtonPanels;
            }
        }

        /// <summary>
        /// Get the button panel configurations.
        /// </summary>
        /// <returns>The button panel configurations.</returns>
        public IList<IButtonPanelConfiguration> GetButtonPanelConfigurations()
        {
            if(buttonPanels != null)
            {
                if(buttonPanelConfigurations == null)
                {
                    buttonPanelConfigurations = new List<IButtonPanelConfiguration>();
                    foreach(var buttonPanel in buttonPanels)
                    {
                        if(buttonPanel == null)
                        {
                            buttonPanelConfigurations.Add(null);
                            continue;
                        }
                        buttonPanelConfigurations.Add(ConvertToButtonPanelConfiguration(buttonPanel));
                    }
                }
                return buttonPanelConfigurations;
            }
            return new List<IButtonPanelConfiguration>();
        }

        /// <summary>
        /// Convert to button panel configuration from the button panels of the button panel settings parser.
        /// </summary>
        /// <param name="buttonPanel">The button panel of the button panel settings parser.</param>
        /// <returns>The button panel configuration.</returns>
        private ButtonPanelConfiguration ConvertToButtonPanelConfiguration(Standalone.ButtonPanel buttonPanel)
        {
            if(buttonPanel == null)
            {
                throw new ArgumentNullException(nameof(buttonPanel));
            }
            var panelType = ButtonPanelType.Unknown;
            switch(buttonPanel.PanelType)
            {
                case Standalone.PanelType.Static:
                    panelType = ButtonPanelType.Static;
                    break;
                case Standalone.PanelType.Dynamic:
                    panelType = ButtonPanelType.Dynamic;
                    break;
                case Standalone.PanelType.StaticDynamic:
                    panelType = ButtonPanelType.StaticDynamic;
                    break;
            }
            var panelLocation = ButtonPanelLocation.Unknown;
            switch(buttonPanel.PanelLocation)
            {
                case Standalone.PanelLocation.Main:
                    panelLocation = ButtonPanelLocation.Main;
                    break;
                case Standalone.PanelLocation.Secondary:
                    panelLocation = ButtonPanelLocation.Secondary;
                    break;
            }
            var deviceId = locationMapping[panelLocation];
            var buttonConfigurations = new List<IButtonConfiguration>();
            if(buttonPanel.Buttons != null)
            {
                foreach(var button in buttonPanel.Buttons)
                {
                    var identifier = new ButtonIdentifier(panelLocation, button.ButtonId.Value);
                    var functions = new List<ButtonFunction>();
                    if(button.ButtonFunctions != null)
                    {
                        functions =
                            button.ButtonFunctions.Select(
                                buttonFunction => (ButtonFunction)buttonFunction).ToList();
                    }
                    var type = (ButtonType)button.ButtonType;
                    buttonConfigurations.Add(
                        new ButtonConfiguration(identifier, type, button.HasDynamicDisplay, functions));
                }
            }
            return new ButtonPanelConfiguration(panelLocation, 
                                                panelType,
                                                buttonConfigurations,
                                                buttonPanel.PanelID,
                                                deviceId);
        }
    }
}
