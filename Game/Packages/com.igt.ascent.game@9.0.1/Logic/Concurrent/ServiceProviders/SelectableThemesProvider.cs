// -----------------------------------------------------------------------
// <copyright file = "SelectableThemesProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Logic.Services;

    /// <summary>
    /// This provider provides game services for querying information
    /// on the selectable themes for starting a new coplayer in a Shell.
    /// </summary>
    public class SelectableThemesProvider : NonObserverProviderBase
    {
        #region Constants

        /// <summary>
        /// The default name of the provider.
        /// </summary>
        private const string DefaultName = nameof(SelectableThemesProvider);

        #endregion

        #region Private Fields

        /// <summary>
        /// Back end field for the property SelectableThemes to help with argument validation in the setter.
        /// </summary>
        private IReadOnlyList<ShellThemeInfo> selectableThemes = new List<ShellThemeInfo>();

        #endregion

        #region Game Services

        /// <summary>
        /// Gets or sets the list of selectable themes and all
        /// related information in <see cref="ShellThemeInfo"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the value to set is null.
        /// </exception>
        [GameService]
        public IReadOnlyList<ShellThemeInfo> SelectableThemes
        {
            get => selectableThemes;
            set => selectableThemes = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of <see cref="SelectableThemesProvider" />.
        /// </summary>
        /// <param name="name">
        /// The name of the service provider.
        /// This parameter is optional.  If not specified, the provider name
        /// will be set to <see cref="DefaultName" />.
        /// </param>
        public SelectableThemesProvider(string name = DefaultName) : base(name)
        {
        }

        #endregion
    }
}