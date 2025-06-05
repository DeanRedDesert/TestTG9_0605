//-----------------------------------------------------------------------
// <copyright file = "NewThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Event indicating a change in the context of the game.
    /// As a result of this message the game can begin re-configuring
    /// for play in the new context.
    /// </summary>
    [Serializable]
    public class NewThemeContextEventArgs : EventArgs
    {
        /// <summary>
        /// Get the theme context.
        /// </summary>
        public ThemeContext ThemeContext { get; private set; }

        /// <summary>
        /// Construct a NewThemeContextEvent with given theme context.
        /// </summary>
        /// <param name="themeContext">The theme context.</param>
        public NewThemeContextEventArgs(ThemeContext themeContext)
        {
            ThemeContext = themeContext;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("NewThemeContextEvent -");
            builder.AppendLine("\t " + ThemeContext);

            return builder.ToString();
        }
    }
}
