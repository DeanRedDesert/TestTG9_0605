//-----------------------------------------------------------------------
// <copyright file = "ThemeResource.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// The resource paths and data tag for a game theme.
    /// </summary>
    /// <DevDoc>
    /// We might have to expand this class to include more information
    /// from LinkControlGetThemeApiVersionsSend as we know more about
    /// how individual games handle the theme switching.
    /// </DevDoc>
    [Serializable]
    public class ThemeResource
    {
        /// <summary>
        /// The mount point of the game package.
        /// </summary>
        public string GameMountPoint { get; set; }

        /// <summary>
        /// The file containing the tag data used by the theme.
        /// </summary>
        /// <remarks>
        /// This is the absolute path of the theme tag data file.
        /// </remarks>
        public string TagDataFile { get; set; }

        /// <summary>
        /// The tag of the data in the file used by the theme.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ThemeResource -");
            builder.AppendLine("\t Game Mount Point = " + GameMountPoint);
            builder.AppendLine("\t Tag Data File = " + TagDataFile);
            builder.AppendLine("\t Tag = " + Tag);

            return builder.ToString();
        }
    }
}
