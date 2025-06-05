//-----------------------------------------------------------------------
// <copyright file = "SetupValidationContext.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;

    /// <summary>
    /// Represents the context for validating setup.
    /// </summary>
    public class SetupValidationContext
    {
        /// <summary>
        /// Gets the theme identifier.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Instantiates a new <see cref="SetupValidationContext"/>.
        /// </summary>
        /// <param name="themeIdentifier">Identifier of the theme to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="themeIdentifier"/> is null or empty.
        /// </exception>
        public SetupValidationContext(string themeIdentifier)
        {
            if(string.IsNullOrEmpty(themeIdentifier))
            {
                throw new ArgumentException("Theme identifier must not be null or empty", nameof(themeIdentifier));
            }

            ThemeIdentifier = themeIdentifier;
        }
    }
}
