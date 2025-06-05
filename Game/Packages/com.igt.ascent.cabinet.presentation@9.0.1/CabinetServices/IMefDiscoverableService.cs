// -----------------------------------------------------------------------
// <copyright file = "IMefDiscoverableService.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// An interface that provides the run time MEF usable exported type.
    /// </summary>
    public interface IMefDiscoverableService
    {
        /// <summary>
        /// Returns the type of a unique interface to be added to the cabinet locator.
        /// </summary>
        Type ExportedType { get; }
    }
}