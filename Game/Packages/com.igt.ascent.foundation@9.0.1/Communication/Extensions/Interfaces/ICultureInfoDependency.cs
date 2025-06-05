// -----------------------------------------------------------------------
// <copyright file = "ICultureInfoDependency.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines APIs for interface extensions to access culture related information.
    /// </summary>
    public interface ICultureInfoDependency
    {
        /// <summary>
        /// Gets the culture supported by the application.
        /// The culture string is in the format of LanguageCode-RegionCode_Dialect.
        /// </summary>
        ICollection<string> AvailableCultures { get; }
    }
}