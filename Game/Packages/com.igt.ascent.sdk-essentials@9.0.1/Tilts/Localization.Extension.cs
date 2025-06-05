//-----------------------------------------------------------------------
// <copyright file = "GameTiltDefinition.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;

    public partial class Localization : IEquatable<Localization>
    {
        #region IEquatable<Localization> Member

        /// <summary>
        /// Determine if two Localization objects are equivalent.
        /// </summary>
        /// <param name="other">The TiltInfo to check against.</param>
        /// <returns>Returns true if they are equivalent.</returns>
        public bool Equals(Localization other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(cultureField, other.cultureField) && string.Equals(contentField, other.contentField);
        }

        #endregion

        #region Overrides
        /// <inheritdoc />
        /// <remarks>
        /// This class is an xml generated object, and must be mutable.
        /// It should not be modified after GetHashCode is called.
        /// It should not be used as a key in a dictionary.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return ((cultureField != null ? cultureField.GetHashCode() : 0) * 397) ^
                        (contentField != null ? contentField.GetHashCode() : 0);
                // ReSharper restore NonReadonlyFieldInGetHashCode
            }
        }

        /// <inheritdoc />
        public override bool Equals(object right)
        {
            if(ReferenceEquals(right, null))
                return false;

            if(ReferenceEquals(this, right))
                return true;

            if(GetType() != right.GetType())
                return false;

            return Equals(right as Localization);
        }

        #endregion
    }
}