//-----------------------------------------------------------------------
// <copyright file = "PaytableVariant.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// Struct that represents a specific paytable in a paytable file
    /// configured for a game theme.
    /// </summary>
    [Serializable]
    internal struct PaytableVariant : IEquatable<PaytableVariant>
    {
        /// <summary>
        /// The name of the game theme.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// The name of the paytable to be used for this game configuration.
        /// </summary>
        public string PaytableName { get; }

        /// <summary>
        /// The name of the file that contains the paytable for this game configuration.
        /// </summary>
        public string PaytableFileName { get; }

        /// <summary>
        /// Initialize a new instance of Paytable Variant with the empty string
        /// as the theme name.
        /// </summary>
        /// <param name="paytableName">Name of the paytable.</param>
        /// <param name="paytableFileName">Name of the file that contains the paytable.</param>
        public PaytableVariant(string paytableName, string paytableFileName)
            : this(string.Empty, paytableName, paytableFileName)
        {
        }

        /// <summary>
        /// Initialize a new instance of Paytable Variant with the information
        /// on the theme and the paytable.
        /// </summary>
        /// <param name="themeIdentifier">Name of the theme.</param>
        /// <param name="paytableName">Name of the paytable.</param>
        /// <param name="paytableFileName">Name of the file that contains the paytable.</param>
        public PaytableVariant(string themeIdentifier, string paytableName, string paytableFileName)
            : this()
        {
            ThemeIdentifier = themeIdentifier ?? string.Empty;
            PaytableName = paytableName;
            PaytableFileName = paytableFileName;
        }

        /// <summary>
        /// Overload Equals for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(PaytableVariant rightHand)
        {
            return ThemeIdentifier == rightHand.ThemeIdentifier &&
                   PaytableName == rightHand.PaytableName &&
                   PaytableFileName == rightHand.PaytableFileName;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if (obj is PaytableVariant variant)
            {
                result = Equals(variant);
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

            hash = ThemeIdentifier != null ? hash * 37 + ThemeIdentifier.GetHashCode() : hash;
            hash = PaytableName != null ? hash * 37 + PaytableName.GetHashCode() : hash;
            hash = PaytableFileName != null ? hash * 37 + PaytableFileName.GetHashCode() : hash;

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(PaytableVariant left, PaytableVariant right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(PaytableVariant left, PaytableVariant right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Theme({ThemeIdentifier})/Paytable({PaytableName})/Paytable File({PaytableFileName})";
        }
    }
}
