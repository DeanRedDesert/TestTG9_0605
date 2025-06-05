//-----------------------------------------------------------------------
// <copyright file = "ThemeTag.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Text;

    /// <summary>
    /// Contains information about a theme as read from the theme registry.
    /// </summary>
    public class ThemeTag
    {
        /// <summary>
        /// The identifier of the theme set by the foundation.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// The G2S identifier of the theme.
        /// </summary>
        /// <remarks>This will be null if the targeted foundation doesn't support the G2SThemeIdentifier.</remarks>
        public string G2SThemeIdentifier { get; }

        /// <summary>
        /// The theme's tag name as defined in the theme registry.
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// The data file for the theme.
        /// </summary>
        public string TagDataFile { get; }

        /// <summary>
        /// Instantiates an instance of <see cref="ThemeTag"/>.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <param name="g2SThemeIdentifier">The G2S identifier of the theme.</param>
        /// <param name="tag">The theme's tag name as defined in the theme registry.</param>
        /// <param name="tagDataFile">The data file for the theme.</param>
        public ThemeTag(string themeIdentifier, string g2SThemeIdentifier, string tag, string tagDataFile)
        {
            ThemeIdentifier = themeIdentifier;
            G2SThemeIdentifier = g2SThemeIdentifier;
            Tag = tag;
            TagDataFile = tagDataFile;
        }

        #region ToString

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Foundation.ThemeTag -");
            builder.AppendLine($"\t ThemeIdentifier({ThemeIdentifier})");
            builder.AppendLine($"\t G2SThemeIdentifier({G2SThemeIdentifier})");
            builder.AppendLine($"\t Tag({Tag})");
            builder.AppendLine($"\t TagDataFile({TagDataFile})");
            return builder.ToString();
        }

        #endregion
    }
}