//-----------------------------------------------------------------------
// <copyright file = "ActivateThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Event indicating to activate the context identified by the event payload.
    /// </summary>
    [Serializable]
    public class ActivateThemeContextEventArgs : EventArgs
    {
        /// <summary>
        /// Get the theme context.
        /// </summary>
        public ThemeContext ThemeContext { get; private set; }

        /// <summary>
        /// Construct a ActivateThemeContextEvent with given theme context.
        /// </summary>
        /// <param name="themeContext">The theme context.</param>
        public ActivateThemeContextEventArgs(ThemeContext themeContext)
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

            builder.AppendLine("ActivateThemeContextEvent -");
            builder.AppendLine("\t " + ThemeContext);

            return builder.ToString();
        }
    }
}
