//-----------------------------------------------------------------------
// <copyright file = "ButtonPanelConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class containing the button panel information. 
    /// </summary>
    internal class ButtonPanelConfiguration : IButtonPanelConfiguration, IEquatable<ButtonPanelConfiguration>
    {
        #region Fields

        /// <summary>
        /// The place where the button panel is physically located. 
        /// </summary>
        private readonly ButtonPanelLocation panelLocation;

        /// <summary>
        /// The device type of the button panel.
        /// </summary>
        private readonly ButtonPanelType panelType;

        /// <summary>
        /// The button configurations on the panel.
        /// </summary>
        private readonly IList<IButtonConfiguration> buttons;

        /// <summary>
        /// The unique integer identifier for the button panel.
        /// </summary>
        private readonly uint panelIdentifier;

        /// <summary>
        /// The device Id of the button panel.
        /// </summary>
        private readonly string deviceId;

        #endregion

        #region IButtonPanelConfiguration Members

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ButtonPanelLocation PanelLocation => panelLocation;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ButtonPanelType PanelType => panelType;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public IList<IButtonConfiguration> Buttons => buttons;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public uint PanelIdentifier => panelIdentifier;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public string DeviceId => deviceId;

        /// <inheritdoc />
        public IButtonConfiguration GetButton(ButtonIdentifier buttonIdentifier)
        {
            if(buttonIdentifier == null)
            {
                throw new ArgumentNullException(nameof(buttonIdentifier));
            }

            return Buttons.FirstOrDefault(buttonConfiguration => 
                                          buttonConfiguration.ButtonIdentifier == buttonIdentifier);
        }

        /// <inheritdoc />
        public IButtonConfiguration GetButton(int buttonId)
        {
            return GetButton(new ButtonIdentifier(ButtonPanelLocation.Main, buttonId));
        }

        /// <inheritdoc />
        public bool HasButton(ButtonIdentifier buttonIdentifier)
        {
            if(buttonIdentifier == null)
            {
                throw new ArgumentNullException(nameof(buttonIdentifier));
            }

            return Buttons.Any(button => button.ButtonIdentifier == buttonIdentifier);
        }

        /// <inheritdoc />
        public bool HasButton(int buttonId)
        {
            return HasButton(new ButtonIdentifier(ButtonPanelLocation.Main, buttonId));
        }

        /// <inheritdoc />
        public bool HasButton(Predicate<IButtonConfiguration> predicate)
        {

            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return Buttons.Any(buttonConfiguration => predicate(buttonConfiguration));
        }

        #endregion

        /// <summary>
        /// Constructs an instance based on the button configuration information from the foundation.
        /// </summary>
        /// <param name="panelLocation">The button panel is physically located.</param>
        /// <param name="panelType">The type of the button panel.</param>
        /// <param name="buttons">The information on the button panel.</param>
        /// <param name="panelIdentifier">The identifier for the button panel.</param>
        /// <param name="deviceId">The device Id of the button panel.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttons"/> is null.
        /// </exception>
        public ButtonPanelConfiguration(ButtonPanelLocation panelLocation,
                                        ButtonPanelType panelType,
                                        IList<IButtonConfiguration> buttons,
                                        uint panelIdentifier,
                                        string deviceId)
        {
            if(buttons == null)
            {
                throw new ArgumentNullException(nameof(buttons));
            }

            this.panelLocation = panelLocation;
            this.panelType = panelType;
            //Strip out the duplicated elements for a better equality implementation of this class.
            this.buttons = new HashSet<IButtonConfiguration>(buttons).ToList();
            this.panelIdentifier = panelIdentifier;
            this.deviceId = deviceId;
        }

        #region Compare

        /// <inheritdoc />
        public bool Equals(ButtonPanelConfiguration other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            if(PanelLocation != other.PanelLocation ||
               PanelType != other.PanelType ||
               PanelIdentifier != other.PanelIdentifier ||
               DeviceId != other.DeviceId)
            {
                return false;
            }

            if(Buttons.Count != other.Buttons.Count)
            {
                return false;
            }

            // This algorithm is based on the assumption that no duplicated elements is existed in the list.
            return !Buttons.Except(other.Buttons).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 ^ PanelLocation.GetHashCode();
            hash = hash * 37 ^ PanelType.GetHashCode();
            hash = hash * 37 ^ PanelIdentifier.GetHashCode();
            if(DeviceId != null)
            {
                hash = hash * 37 ^ DeviceId.GetHashCode();
            }
            // orderless hash to the button list elements.
            return Buttons.Aggregate(hash * 37 + 17, (current, button) => current ^ button.GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ButtonPanelConfiguration);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder("ButtonPanelConfiguration: \n");
            builder.AppendFormat("\tPanelLocation={0}, PanelType={1}, PanelID={2}, DeviceId={3}\n",
                                 PanelLocation,
                                 PanelType,
                                 PanelIdentifier,
                                 DeviceId ?? "null");
            builder.Append("\tButtons");
            if(Buttons == null)
            {
                builder.Append("=Null\n");
            }
            else
            {
                builder.AppendFormat(": Count={0}\n", Buttons.Count);
                foreach(var button in Buttons)
                {
                    builder.AppendFormat("\t\t{0};\n", button);
                }
            }

            return builder.ToString();
        }
    }
}