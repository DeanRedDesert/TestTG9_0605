//-----------------------------------------------------------------------
// <copyright file = "LocalizedResourceType.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    /// <summary>
    /// This enumeration is used to define all the types of localized resource.
    /// </summary>
    public enum LocalizedResourceType
    {
        /// <summary>
        /// The localized resource is a String.
        /// </summary>
        String,

        /// <summary>
        /// The localized resource is a File.
        /// </summary>
        File,

        /// <summary>
        /// The localized resource is an Image.
        /// </summary>
        Image,

        /// <summary>
        /// The localized resource is a Movie.
        /// </summary>
        Movie,

        /// <summary>
        /// The localized resource is a Sound.
        /// </summary>
        Sound,
    }
}