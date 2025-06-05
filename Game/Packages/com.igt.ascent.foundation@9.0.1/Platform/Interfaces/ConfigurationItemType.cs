//-----------------------------------------------------------------------
// <copyright file = "ConfigurationItemType.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the types of a custom configuration item
    /// declared by the game in the registry files.
    /// </summary>
    [Serializable]
    public enum ConfigurationItemType
    {
        /// <summary>
        /// Default is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Signed, 64-bit integer value, with embedded
        /// integrity (CRC) check.
        /// </summary>
        Amount,

        /// <summary>
        /// Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Declaration list containing the set of allowable values
        /// that may be set on an <see cref="Item"/> type or a
        /// <see cref="FlagList"/> type.
        /// </summary>
        EnumerationList,

        /// <summary>
        /// List of string names/Boolean pairs that reference an <see cref="EnumerationList"/> type,
        /// with each enumerated name appearing once and in the same order as the Enumeration List.
        /// The Boolean value indicates if the value is set or cleared.
        /// This type may serve as the basis for a conversion to a bit-field/bit-mask.
        /// </summary>
        FlagList,

        /// <summary>
        /// Single precision floating point number.
        /// </summary>
        Float,

        /// <summary>
        /// Signed 64-bit integer.
        /// </summary>
        Int64,

        /// <summary>
        /// A single value of an <see cref="EnumerationList"/> type
        /// manipulated as a character string.
        /// </summary>
        Item,

        /// <summary>
        /// ASCII character string not to exceed 99 characters in length.
        /// </summary>
        String
    }
}
