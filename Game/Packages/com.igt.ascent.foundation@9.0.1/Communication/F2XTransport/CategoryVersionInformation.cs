//-----------------------------------------------------------------------
// <copyright file = "CategoryVersionInformation.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Class for storing a category and its version number.
    /// </summary>
    public class CategoryVersionInformation : IEquatable<CategoryVersionInformation>
    {
        /// <summary>
        /// Format string used for <see cref="ToString"/> operations.
        /// </summary>
        private const string ToStringFormat = "Category: {0}, Major: {1}, Minor: {2}";

        /// <summary>
        /// Category number.
        /// </summary>
        public int Category { get; }

        /// <summary>
        /// Major version of the category.
        /// </summary>
        public uint MajorVersion { get; }

        /// <summary>
        /// Minor version of the category.
        /// </summary>
        public uint MinorVersion { get; }

        /// <summary>
        /// Construct a category version information object.
        /// </summary>
        /// <param name="category">Category number.</param>
        /// <param name="majorVersion">Major version of the category.</param>
        /// <param name="minorVersion">Minor version of the category.</param>
        public CategoryVersionInformation(int category, uint majorVersion, uint minorVersion)
        {
            Category = category;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        /// <summary>
        /// Compare the version to an object and determine if they are equal.
        /// </summary>
        /// <param name="obj">The object to compare this version to.</param>
        /// <returns>True if the versions are the same.</returns>
        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != typeof(CategoryVersionInformation))
            {
                return false;
            }

            var other = (CategoryVersionInformation)obj;
            return Equals(other);
        }

        /// <summary>
        /// Override of the hash code operator to follow equality rules.
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            var categoryShift = (uint)Category << 20;
            var majorShift = MajorVersion << 10;
            var minorShift = MinorVersion << 10;
            return (int)(categoryShift + majorShift + minorShift);
        }

        #region IEquatable<CategoryVersionInformation> Members

        /// <inheritdoc/>
        public bool Equals(CategoryVersionInformation other)
        {
            return other?.Category == Category &&
                   other.MajorVersion == MajorVersion &&
                   other.MinorVersion == MinorVersion;
        }

        #endregion

        #region Object Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, ToStringFormat, Category, MajorVersion, MinorVersion);
        }

        #endregion
    }
}
