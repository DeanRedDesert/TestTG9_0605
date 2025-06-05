//-----------------------------------------------------------------------
// <copyright file = "PayvarType.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;

    /// <summary>
    /// This enumeration defines the payvar type.
    /// </summary>
    [Serializable]
    public enum PayvarType
    {
        /// <summary>
        /// The standard payvar.
        /// </summary>
        Standard,

        /// <summary>
        /// The tournament payvar.
        /// </summary>
        Tournament,

        /// <summary>
        /// A type that indicates that this registry file is for a group template.
        /// </summary>
        PayvarGroupTemplate,

        /// <summary>
        /// A type that indicates that this registry file supports single-instance metering and group metering.
        /// </summary>
        SingleMultiTemplate
    }
}
