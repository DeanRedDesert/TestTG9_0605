// -----------------------------------------------------------------------
// <copyright file = "ButtonIdentifier.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Unique button identifier.
    /// </summary>
    public class ButtonIdentifier : IEquatable<ButtonIdentifier>
    {
        /// <summary>
        /// Identifier for an invalid button.
        /// </summary>
        public static readonly ButtonIdentifier Invalid = new ButtonIdentifier(ButtonPanelLocation.Unknown,
            (int)SwitchId.InvalidButtonId);

        /// <summary>
        /// Gets the panel location that the button identifier belongs to.
        /// </summary>
        /// <remarks>If not specified default location is Main.</remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        public ButtonPanelLocation PanelLocation { get; }

        /// <summary>
        /// The button identifier value.
        /// </summary>
        /// <remarks>
        /// The value of 255 indicates all buttons on all button panels.
        /// </remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        public int Identifier { get; }

        /// <summary>
        /// Construct an instance of the <see cref="ButtonIdentifier"/>
        /// with <paramref name="location"/> and <paramref name="buttonId"/>.
        /// </summary>
        /// <param name="location">The location of the panel.</param>
        /// <param name="buttonId">The unique button id on this panel.</param>
        public ButtonIdentifier(ButtonPanelLocation location, int buttonId)
        {
            PanelLocation = location;
            Identifier = buttonId;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">The instance to copy.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="copy"/> is null.
        /// </exception>
        public ButtonIdentifier(ButtonIdentifier copy)
        {
            if(copy == null)
            {
                throw new ArgumentNullException(nameof(copy));
            }

            PanelLocation = copy.PanelLocation;
            Identifier = copy.Identifier;
        }

        /// <inheritdoc />
        public bool Equals(ButtonIdentifier other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return PanelLocation == other.PanelLocation && Identifier == other.Identifier;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ButtonIdentifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PanelLocation.GetHashCode() ^ Identifier.GetHashCode();
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(ButtonIdentifier left, ButtonIdentifier right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }
            if(ReferenceEquals(left, null) || ReferenceEquals(right,null))
            {
                return false;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not equal. False otherwise.</returns>
        public static bool operator !=(ButtonIdentifier left, ButtonIdentifier right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ButtonIdentifier: PanelLocation={PanelLocation}, Identifier={Identifier} ";
        }
    }
}