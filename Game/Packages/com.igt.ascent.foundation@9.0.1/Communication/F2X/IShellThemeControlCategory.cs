//-----------------------------------------------------------------------
// <copyright file = "IShellThemeControlCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;
    using Schemas.Internal.ShellThemeControl;
    using Schemas.Internal.Types;

    /// <summary>
    /// F2X Shell Theme Control category of messages.
    /// Category: 1015; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IShellThemeControlCategory
    {
        /// <summary>
        /// Requests a list of selectable themes.
        /// </summary>
        /// <returns>
        /// The content of the GetSelectableThemesReply message.
        /// </returns>
        IEnumerable<ThemeInformation> GetSelectableThemes();

        /// <summary>
        /// Requests the theme information for a given theme.
        /// </summary>
        /// <param name="themeIdentifier">
        /// Identifies the theme identifier whose theme information is being requested.
        /// </param>
        /// <returns>
        /// The content of the GetThemeInformationReply message.
        /// </returns>
        ThemeInformation GetThemeInformation(ThemeIdentifier themeIdentifier);

        /// <summary>
        /// Switches a theme on the coplayer.
        /// </summary>
        /// <param name="coplayer">
        /// The coplayer on which the theme switch happens.
        /// </param>
        /// <param name="themeSelector">
        /// Indicates the theme and denomination to be selected for the coplayer. If omitted, the coplayer is set to an
        /// empty theme identifier and blank denomination.
        /// </param>
        /// <returns>
        /// The content of the SwitchThemeReply message.
        /// </returns>
        bool SwitchTheme(int coplayer, ThemeSelector themeSelector);

    }

}

