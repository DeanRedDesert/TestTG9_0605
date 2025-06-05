// -----------------------------------------------------------------------
// <copyright file = "IGameCulture.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for a game application to query culture related config data.
    /// </summary>
    /// <devdoc>
    /// Add CultureWrite methods to this interface as well in the future.
    /// </devdoc>
    public interface IGameCulture
    {
        /// <summary>
        /// Gets the culture of the game application in the format of: LanguageCode-RegionCode_Dialect.
        /// </summary>
        string Culture { get; }

        /// <summary>
        /// Gets the cultures supported by the game application.
        /// </summary>
        IReadOnlyList<string> AvailableCultures { get; }

        /// <summary>
        /// Event that is raised when the current culture of the game application has changed.
        /// </summary>
        /// <remarks>
        /// The <see cref="CultureContext"/> in the event arguments will always be <see cref="CultureContext.Game"/>.
        /// </remarks>
        event EventHandler<CultureChangedEventArgs> CultureChangedEvent;
    }
}