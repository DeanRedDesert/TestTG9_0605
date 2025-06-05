// -----------------------------------------------------------------------
// <copyright file = "ICultureRead.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// Provides methods to request culture-related information
    /// from the Foundation.
    /// </summary>
    public interface ICultureRead
    {
        /// <summary>
        /// Requests the current culture for the specified <paramref name="cultureContext"/>.
        /// </summary>
        /// <param name="cultureContext">
        /// The context in which the requested culture is intended for.
        /// </param>
        /// <returns>The culture in the format of: LanguageCode-RegionCode_Dialect.</returns>
        string GetCulture(CultureContext cultureContext);

        /// <summary>
        /// Event that is raised when the current culture has changed.
        /// </summary>
        event EventHandler<CultureChangedEventArgs> CultureChangedEvent;
    }
}
