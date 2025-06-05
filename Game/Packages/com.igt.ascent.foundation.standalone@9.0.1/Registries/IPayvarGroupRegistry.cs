//-----------------------------------------------------------------------
// <copyright file = "IPayvarGroupRegistry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface represents a payvar group registry object and is used to retrieve the information from
    /// the payvar group registry file.
    /// </summary>
    public interface IPayvarGroupRegistry
    {
        /// <summary>
        /// Gets the name of the payvar group template(payvar registry) for this payvar registry group.
        /// </summary>
        string GroupTemplateName { get; }

        /// <summary>
        /// Gets a list of <see cref="Payvar"/>.
        /// </summary>
        IList<Payvar> Payvars { get; }

        /// <summary>
        /// Store the theme registry associated with this PayvarGroup, used to distinguish
        /// two different payvars that use the same paytable name but are in different themes.
        /// </summary>
        string ThemeRegistryFileName { get; set; }
    }
}
