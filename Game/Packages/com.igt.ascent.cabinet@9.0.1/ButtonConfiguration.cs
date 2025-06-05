//-----------------------------------------------------------------------
// <copyright file = "ButtonConfiguration.cs" company = "IGT">
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
    /// Class containing the button panel's button information.
    /// </summary>
    internal class ButtonConfiguration : IButtonConfiguration, IEquatable<ButtonConfiguration>
    {
        #region Fields

        /// <summary>
        /// The unique button identifier which identifies a button on an EGM, which could be used to
        /// support multiple panels.
        /// </summary>
        private readonly ButtonIdentifier buttonIdentifier;

        /// <summary>
        /// The hardware type of the button.
        /// </summary>
        private readonly ButtonType hardwareType;

        /// <summary>
        /// Indicates if the button has dynamic display.
        /// </summary>
        private readonly bool hasDynamicDisplay;

        /// <summary>
        /// The functions associated with the button.
        /// </summary>
        private readonly IList<ButtonFunction> functions;

        #endregion

        #region IButtonConfiguration Members

        /// <inheritdoc />
        public byte ButtonId => (byte)ButtonIdentifier.Identifier;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ButtonIdentifier ButtonIdentifier => buttonIdentifier;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public ButtonType HardwareType => hardwareType;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public bool HasDynamicDisplay => hasDynamicDisplay;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public IList<ButtonFunction> Functions => functions;

        /// <inheritdoc />
        public bool HasValidButtonFunction()
        {
            return Functions.Any(funciton => funciton != ButtonFunction.Unknown);
        }

        #endregion

        /// <summary>
        /// Constructs an instance based on the button configuration information from the foundation.
        /// </summary>
        /// <param name="buttonIdentifier">The identifier for the button.</param>
        /// <param name="hardwareType">The hardware type of the button.</param>
        /// <param name="hasDynamicDisplay">Indicating if the button has dynamic display.</param>
        /// <param name="functions">The functions associated with the button.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttonIdentifier"/> is null.
        /// </exception>
        public ButtonConfiguration(ButtonIdentifier buttonIdentifier,
                                   ButtonType hardwareType,
                                   bool hasDynamicDisplay,
                                   IList<ButtonFunction> functions)
        {
            if(buttonIdentifier == null)
            {
                throw new ArgumentNullException(nameof(buttonIdentifier));
            }

            if(functions == null)
            {
                throw new ArgumentNullException(nameof(functions));
            }

            this.buttonIdentifier = buttonIdentifier;
            this.hardwareType = hardwareType;
            this.hasDynamicDisplay = hasDynamicDisplay;
            //Strip out the duplicated elements for a better equality implementation of this class.
            this.functions = new HashSet<ButtonFunction>(functions).ToList();
        }

        #region Compare

        /// <inheritdoc />
        public bool Equals(ButtonConfiguration other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            if(ButtonIdentifier != other.ButtonIdentifier ||
               HardwareType != other.HardwareType ||
               HasDynamicDisplay != other.HasDynamicDisplay)
            {
                return false;
            }

            if(Functions.Count != other.Functions.Count)
            {
                return false;
            }

            // This algorithm is based on the assumption that no duplicated elements is existed in the list.
            return !Functions.Except(other.Functions).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 ^ ButtonIdentifier.GetHashCode();
            hash = hash * 37 ^ HardwareType.GetHashCode();
            hash = hash * 37 ^ HasDynamicDisplay.GetHashCode();
            //orderless hash to the function list elements.
            return Functions.Aggregate(hash * 37 + 17, (current, func) => current ^ func.GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ButtonConfiguration);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder("ButtonConfiguration: ");
            builder.AppendFormat(
                "PanelLocation={0}, Identifier={1}, HardwareType={2}, HasDynamicDisplay={3} Functions=",
                ButtonIdentifier.PanelLocation,
                ButtonIdentifier.Identifier,
                HardwareType,
                HasDynamicDisplay);

            if(Functions == null)
            {
                builder.Append("Null");
            }
            else
            {
                builder.Append("{");
                foreach(var function in Functions)
                {
                    builder.AppendFormat("{0},", function);
                }
                builder.Append("}");
            }
            return builder.ToString();
        }
    }
}
