//-----------------------------------------------------------------------
// <copyright file = "ColorProfileSetting.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Class used to represent the settings for a color profile.
    /// </summary>
    public class ColorProfileSetting
    {
        /// <summary>
        /// The type of color profile for this setting.
        /// </summary>
        public ColorProfile ProfileType { get; }

        /// <summary>
        /// Path for use with custom profiles. For non-custom profiles this property should be null.
        /// </summary>
        public string ProfilePath { get; }

        /// <summary>
        /// Construct a profile setting with the given profile type. This constructor should be used for non-custom
        /// profiles.
        /// </summary>
        /// <param name="profileType">The type of the profile for the setting.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the profileType is custom. Custom profiles require the inclusion of a path to the profile.
        /// An alternate constructor is provided for custom profiles.
        /// </exception>
        public ColorProfileSetting(ColorProfile profileType)
        {
            if(profileType == ColorProfile.Custom)
            {
                throw new ArgumentException(
                    "A custom profile requires the inclusion of a path. Please use the profile constructor which takes" +
                    " a path as an argument.");
            }
            ProfileType = profileType;
        }

        /// <summary>
        /// Construct a profile setting for a custom profile.
        /// </summary>
        /// <param name="profilePath">The path to the custom profile.</param>
        /// <exception cref="ArgumentException">Thrown when the path is either null or empty.</exception>
        public ColorProfileSetting(string profilePath)
        {
            if(string.IsNullOrEmpty(profilePath))
            {
                throw new ArgumentException("A custom profile requires the inclusion of a path.");
            }

            ProfileType = ColorProfile.Custom;
            ProfilePath = profilePath;
        }
    }
}
