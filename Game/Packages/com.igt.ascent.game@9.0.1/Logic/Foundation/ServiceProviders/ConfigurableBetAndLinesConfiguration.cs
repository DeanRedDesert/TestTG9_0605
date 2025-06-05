// -----------------------------------------------------------------------
// <copyright file = "ConfigurableBetAndLinesConfiguration.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Contains information about the configurable bet and lines layout.
    /// i.e. line buttons on top, bet buttons on bottom or vice versa.
    /// </summary>
    [Serializable]
    public class ConfigurableBetAndLinesConfiguration : ICompactSerializable,
        IEquatable<ConfigurableBetAndLinesConfiguration>,
        IDeepCloneable
    {
        #region Public Properties

        /// <summary>
        /// Gets the selected configurable bet and lines configuration.
        /// </summary>
        public BetButtonLayout ButtonPanelLayoutStyle { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Necessary to support <see cref="ICompactSerializable"/>.
        /// </remarks>
        public ConfigurableBetAndLinesConfiguration()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurableBetAndLinesProvider"/> object.
        /// </summary>
        /// <param name="buttonPanelLayoutStyle">The selected button panel layout style.</param>
        public ConfigurableBetAndLinesConfiguration(BetButtonLayout buttonPanelLayoutStyle)
        {
            ButtonPanelLayoutStyle = buttonPanelLayoutStyle;
        }

        #endregion

        #region ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ButtonPanelLayoutStyle);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            ButtonPanelLayoutStyle = CompactSerializer.ReadEnum<BetButtonLayout>(stream);
        }

        #endregion

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(ConfigurableBetAndLinesConfiguration other)
        {

            return this == other;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as ConfigurableBetAndLinesConfiguration);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc/>
        public object DeepClone()
        {
            return new ConfigurableBetAndLinesConfiguration(ButtonPanelLayoutStyle);
        }

        #endregion

        #region Object

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ButtonPanelLayoutStyle.GetHashCode();
        }

        #endregion

        #region Operator Overload

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(
            ConfigurableBetAndLinesConfiguration left, ConfigurableBetAndLinesConfiguration right)
        {

            if(ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }

            if(ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                // If only one of the instances is null, return false.
                return false;
            }

            if(ReferenceEquals(left, right))
            {
                // The objects are the same instance.
                return true;
            }

            return left.ButtonPanelLayoutStyle == right.ButtonPanelLayoutStyle;

        }

        /// <summary>
        /// Tests for inequality.
        /// </summary>
        /// <param name="left">The left side object to compare.</param>
        /// <param name="right">The right side object to compare.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(ConfigurableBetAndLinesConfiguration left, ConfigurableBetAndLinesConfiguration right)
        {
            return !(left == right);
        }

    #endregion
    }
}