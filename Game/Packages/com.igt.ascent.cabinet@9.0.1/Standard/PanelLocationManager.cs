// -----------------------------------------------------------------------
// <copyright file = "PanelLocationManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas.Internal;

    /// <summary>
    /// This class is mainly responsible for conversion between <see cref="ButtonPanelLocation"/> and DeviceId.
    /// </summary>
    internal class PanelLocationManager
    {
        /// <summary>
        /// Dictionary to mapping between ButtonPanelLocation and DeviceId.
        /// </summary>
        private Dictionary<ButtonPanelLocation, string> locationMapping = new Dictionary<ButtonPanelLocation, string>();

        /// <summary>
        /// Set the mapping dictionary to <see cref="PanelLocationManager"/>.
        /// </summary>
        /// <param name="panelConfigurations">The configurations of the button panel.</param>
        public void UpdateLocationMapping(List<ConfigurationResponseData> panelConfigurations)
        {
            locationMapping = panelConfigurations.ToDictionary(
                                  data => data.PanelLocation.ToPublic(),
                                  data => data.DeviceId);
        }

        /// <summary>
        /// Retrieve the DeviceId from <see cref="locationMapping"/>.
        /// </summary>
        /// <param name="panelLocation">Location of the panel.</param>
        /// <param name="deviceId">
        /// The device Id that corresponding to the <paramref name="panelLocation"/>.
        /// When the panel location can not be found (return value is false),
        /// this value could be null.
        /// </param>
        /// <returns>
        /// True if the string returned in <paramref name="deviceId"/> is valid;
        /// otherwise, false.
        /// </returns>
        public bool GetDeviceId(ButtonPanelLocation panelLocation, out string deviceId)
        {
            deviceId = null;
            return locationMapping != null && locationMapping.TryGetValue(panelLocation, out deviceId);
        }

        /// <summary>
        /// Convert SDK <see cref="ButtonIdentifier"/> to CSI <see cref="ButtonId"/>.
        /// </summary>
        /// <param name="buttonIdentifier">The Unique button identifier that used in SDK.</param>
        /// <returns>
        /// The button Id that used to talk to Foundation.
        /// When trying to use a button panel that does not exist returns null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonIdentifier"/> is null.
        /// </exception>
        public ButtonId ConvertToButtonId(ButtonIdentifier buttonIdentifier)
        {
            if(buttonIdentifier == null)
            {
                throw new ArgumentNullException(nameof(buttonIdentifier));
            }

            return GetDeviceId(buttonIdentifier.PanelLocation, out string deviceId)
                   ? new ButtonId
                   {
                       DeviceId = deviceId,
                       Value = (byte)buttonIdentifier.Identifier
                   }
                   : null;
        }

        /// <summary>
        /// Convert CSI <see cref="ButtonId"/> to SDK <see cref="ButtonIdentifier"/>.
        /// </summary>
        /// <param name="buttonId">The button Id from Foundation.</param>
        /// <returns>The button identifier that used in SDK.</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        public ButtonIdentifier ConvertToButtonIdentifier(ButtonId buttonId)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            var panelLocation = GetPanelLocation(buttonId);
            return new ButtonIdentifier(panelLocation, buttonId.Value);
        }

        /// <summary>
        /// Retrieve the <see cref="ButtonPanelLocation"/> from <see cref="locationMapping"/>.
        /// </summary>
        /// <param name="buttonId">The button Id from Foundation.</param>
        /// <returns>The location of the button panel.</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        private ButtonPanelLocation GetPanelLocation(ButtonId buttonId)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            if(locationMapping != null)
            {
                foreach(var location in locationMapping)
                {
                    if(location.Value == buttonId.DeviceId)
                    {
                        return location.Key;
                    }
                }
            }

            return ButtonPanelLocation.Unknown;
        }
    }
}