//-----------------------------------------------------------------------
// <copyright file = "NewThemeSelectionEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating a change in game theme.
    /// </summary>
    [Serializable]
    public class NewThemeSelectionEventArgs : NonTransactionalEventArgs
    {
        /// <summary>
        /// The tag of the data in the file used by the theme.
        /// </summary>
        public ThemeResource ThemeResource { get; private set; }

        /// <summary>
        /// Construct a NewThemeSelectionEvent with given parameters.
        /// </summary>
        /// <param name="themeResource">The tag of the data in the file used by the theme.</param>
        public NewThemeSelectionEventArgs(ThemeResource themeResource)
        {
            ThemeResource = themeResource;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("NewThemeSelectionEvent -");
            builder.AppendLine("\t - " + ThemeResource);

            return builder.ToString();
        }
    }
}
