// -----------------------------------------------------------------------
// <copyright file = "CriticalDataName.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A validated critical data name.
    /// </summary>
    /// <remarks>
    /// Critical data names must conform to the following constraints.
    /// <list type="number">
    /// <item>
    /// The character set for the name is limited to a subset of
    /// ASCII characters that include numeric, alphabetic and the
    /// characters '/', '.', '_', and '-'.
    /// </item>
    /// <item>
    /// The name cannot start with '/'.
    /// </item>
    /// </list>
    /// </remarks>
    public readonly struct CriticalDataName : IEquatable<CriticalDataName>
    {
        /// <summary>
        /// Regular expression used to validate the critical data name.
        /// </summary>
        /// <devdoc>
        /// The regular expression in schema F2XTypes.CriticalDataName is: "([a-zA-Z0-9_-]+[/])*([a-zA-Z0-9_-]+)",
        /// which is more relaxed than what we have here.
        /// </devdoc>
        private static readonly Regex CriticalDataNameRegex = new Regex(@"^[a-zA-Z0-9\._-]([a-zA-Z0-9/\._-]{0,1}[a-zA-Z0-9\._-])+$");

        /// <summary>
        /// The underlying value.
        /// </summary>
        private readonly string value;

        /// <summary>
        /// Initializes a new <see cref="CriticalDataName"/>
        /// </summary>
        /// <param name="name">A path-like string to use as the name.</param>
        /// <exception cref="ArgumentNullException">
        /// Throws when <paramref name="name"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throws when <paramref name="name"/> is not a valid critical data name.
        /// </exception>
        public CriticalDataName(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty.");
            }
            if(!CriticalDataNameRegex.IsMatch(name))
            {
                throw new ArgumentException($"{name} is not a valid critical data name.");
            }
            value = name;
        }

        /// <summary>
        /// Implicitly converts a <see cref="CriticalDataName"/> instance to a <see cref="string"/>.
        /// </summary>
        /// <param name="criticalDataName">The critical data name to convert.</param>
        public static implicit operator string(CriticalDataName criticalDataName) => criticalDataName.value;

        /// <summary>
        /// Implicitly converts a <see cref="string"/> value to a <see cref="CriticalDataName"/>.
        /// </summary>
        /// <param name="name">The <see cref="string"/> value to convert.</param>
        public static implicit operator CriticalDataName(string name)
        {
            return new CriticalDataName(name);
        }

        /// <summary>
        /// Determines if another instance of <see cref="CriticalDataName"/> is equal to this one using value semantics.
        /// </summary>
        /// <param name="other">The other <see cref="CriticalDataName"/>.</param>
        /// <returns>A <see cref="bool"/> which is true if the other instance equals this one under value semantics.</returns>
        public bool Equals(CriticalDataName other)
        {
            return value == other.value;
        }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
        {
            return obj is CriticalDataName other && Equals(other) || obj is string name && Equals(new CriticalDataName(name));
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// Determines if two instances of <see cref="CriticalDataName"/> are equal under value semantics.
        /// </summary>
        /// <param name="left">The instance on the lhs.</param>
        /// <param name="right">The instance on the rhs.</param>
        /// <returns>A <see cref="bool"/> which is true if the left hand instance equals the right hand instance under value semantics.</returns>
        public static bool operator ==(CriticalDataName left, CriticalDataName right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two instances of <see cref="CriticalDataName"/> are not equal under value semantics.
        /// </summary>
        /// <param name="left">The instance on the lhs.</param>
        /// <param name="right">The instance on the rhs.</param>
        /// <returns>A <see cref="bool"/> which is true if the the left hand instance does not equal the right hand instance under value semantics.</returns>
        public static bool operator !=(CriticalDataName left, CriticalDataName right)
        {
            return !left.Equals(right);
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return this;
        }

        #endregion

    }
}