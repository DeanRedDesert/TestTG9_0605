// -----------------------------------------------------------------------
// <copyright file = "ButtonTypeConverters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas.Internal;
    using ButtonFunction = ButtonFunction;

    /// <summary>
    /// Class for converting between interface types and CSI Schema types.
    /// </summary>
    internal static class ButtonTypeConverters
    {
        /// <summary>
        /// Mappings from CSI <see cref="PanelType"/> to <see cref="ButtonPanelType"/>.
        /// </summary>
        private static readonly Dictionary<PanelType, ButtonPanelType> MapToButtonPanelType =
            new Dictionary<PanelType, ButtonPanelType>
                {
                    { PanelType.Unknown, ButtonPanelType.Unknown },
                    { PanelType.Static, ButtonPanelType.Static },
                    { PanelType.Dynamic, ButtonPanelType.Dynamic },
                    { PanelType.StaticDynamic, ButtonPanelType.StaticDynamic }
                };

        /// <summary>
        /// Mappings from CSI <see cref="PanelLocation"/> to <see cref="ButtonPanelLocation"/>.
        /// </summary>
        private static readonly Dictionary<PanelLocation, ButtonPanelLocation> MapToButtonPanelLocation =
            new Dictionary<PanelLocation, ButtonPanelLocation>
                {
                    { PanelLocation.Main, ButtonPanelLocation.Main },
                    { PanelLocation.Secondary, ButtonPanelLocation.Secondary },
                    { PanelLocation.Unknown, ButtonPanelLocation.Unknown }
                };

        /// <summary>
        /// Extension method to convert a CSI <see cref="PanelLocation"/>
        /// to SDK <see cref="ButtonPanelLocation"/>.
        /// </summary>
        /// <param name="panelLocation">The CSI location type to convert.</param>
        /// <returns>The SDK location type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="panelLocation"/> is not a supported location type.
        /// </exception>
        public static ButtonPanelLocation ToPublic(this PanelLocation panelLocation)
        {
            if(!MapToButtonPanelLocation.ContainsKey(panelLocation))
            {
                throw new ArgumentOutOfRangeException(nameof(panelLocation),
                                                      "The panelLocation should be Main/Secondary/Unknown.");
            }

            return MapToButtonPanelLocation[panelLocation];
        }

        /// <summary>
        /// Extension method to convert a CSI <see cref="PanelType"/> to SDK <see cref="ButtonPanelType"/>.
        /// </summary>
        /// <param name="panelType">The CSI panel type to convert.</param>
        /// <returns>The SDK panel type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="panelType"/> is not a supported panel type.
        /// </exception>
        public static ButtonPanelType ToButtonPanelType(this PanelType panelType)
        {
            if(!MapToButtonPanelType.ContainsKey(panelType))
            {
                throw new ArgumentOutOfRangeException(nameof(panelType), "The panel type specified is not supported.");
            }

            return MapToButtonPanelType[panelType];
        }

        /// <summary>
        /// Extension method to convert a CSI <see cref="Button"/> to SDK <see cref="ButtonConfiguration"/>.
        /// </summary>
        /// <param name="button">The CSI button to convert.</param>
        /// <param name="panelLocation">The location that the button on the panel.</param>
        /// <returns>The SDK button configuration type.</returns>
        public static ButtonConfiguration ToButtonConfiguration(this Button button, ButtonPanelLocation panelLocation)
        {
            if(button == null)
            {
                return null;
            }

            var identifier = new ButtonIdentifier(panelLocation, button.ButtonId.Value);
            var functions = button.ButtonFunctions != null
                ? button.ButtonFunctions.Select(function => (ButtonFunction)function).ToList()
                : new List<ButtonFunction>();
            ButtonType type;
            if(button.HardwareButtonIdSpecified)
            {
                switch(button.HardwareButtonId)
                {
                    case 1:
                    case 2:
                        type = ButtonType.Dynamic;
                        break;

                    default:
                        type = ButtonType.Static;
                        break;
                }
            }
            else
            {
                type = button.Dynamic ? ButtonType.Dynamic : ButtonType.Static;
            }

            return new ButtonConfiguration(identifier, type, button.Dynamic, functions);
        }

        /// <summary>
        /// Extension method to convert a CSI <see cref="ConfigurationResponseData"/> to button panel configuration.
        /// </summary>
        /// <param name="configurationResponseData">
        /// The data that contains button panel configuration.
        /// </param>
        /// <returns>The button panel configuration.</returns>
        public static ButtonPanelConfiguration ToButtonPanelConfiguration(
            this ConfigurationResponseData configurationResponseData)
        {
            if(configurationResponseData == null)
            {
                return null;
            }

            var panelType = configurationResponseData.PanelType.ToButtonPanelType();

            var panelLocation = configurationResponseData.PanelLocation.ToPublic();

            var buttonConfigurations = new List<IButtonConfiguration>();
            if(configurationResponseData.Buttons?.Button != null)
            {
                buttonConfigurations.AddRange(
                    configurationResponseData.Buttons.Button.Select(button =>
                        button.ToButtonConfiguration(panelLocation)));
            }

            return new ButtonPanelConfiguration(panelLocation,
                                                panelType,
                                                buttonConfigurations,
                                                configurationResponseData.PanelID,
                                                configurationResponseData.DeviceId);
        }
    }
}