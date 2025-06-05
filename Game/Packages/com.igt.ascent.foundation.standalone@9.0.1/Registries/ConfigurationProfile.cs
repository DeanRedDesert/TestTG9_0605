//-----------------------------------------------------------------------
// <copyright file = "ConfigurationProfile.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Struct that represents the profile of a configuration item.
    /// </summary>
    [Serializable]
    public struct ConfigurationProfile
    {
        /// <summary>
        /// Data type used for the configuration item associated with this profile.
        /// </summary>
        public ConfigurationItemType DataType { get; }

        /// <summary>
        /// The name of the configuration referenced by the profiled configuration.
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Constructor takes arguments for both fields.
        /// </summary>
        /// <param name="dataType">Data type used for the configuration item
        ///                        associated with this profile.</param>
        /// <param name="reference">The name of the configuration referenced
        ///                         by the profiled configuration.</param>
        public ConfigurationProfile(ConfigurationItemType dataType, string reference = null)
            : this()
        {
            DataType = dataType;
            Reference = reference;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(ConfigurationProfile rightHand)
        {
            return DataType == rightHand.DataType &&
                   Reference == rightHand.Reference;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if (obj is ConfigurationProfile profile)
            {
                result = Equals(profile);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 37 + DataType.GetHashCode();
            hash = Reference != null ? hash * 37 + Reference.GetHashCode() : hash;

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(ConfigurationProfile left, ConfigurationProfile right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(ConfigurationProfile left, ConfigurationProfile right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"DataType({DataType})/ReferencedEnumDeclaration({Reference})";
        }

    }
}
