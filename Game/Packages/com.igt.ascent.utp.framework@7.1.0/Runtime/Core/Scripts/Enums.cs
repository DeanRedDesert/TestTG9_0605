// -----------------------------------------------------------------------
//  <copyright file = "Enums.cs" company = "IGT">
//      Copyright (c) 2019 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    /// <summary>
    /// A set of statuses that help the controller know how to deal with this module.
    /// </summary>
    public enum ModuleStatuses
    {
        /// <summary>
        /// A module is 'Found' after its type has been discovered via reflection.
        /// </summary>
        Found,

        /// <summary>
        /// Status when Initialize is successful.
        /// </summary>
        InitializedEnabled,

        /// <summary>
        /// Status when Initialize returns false.
        /// </summary>
        InitializedDisabled,

        /// <summary>
        /// Error is used when Initialize or Reinitialize throws an exception.
        /// </summary>
        Error
    }
}