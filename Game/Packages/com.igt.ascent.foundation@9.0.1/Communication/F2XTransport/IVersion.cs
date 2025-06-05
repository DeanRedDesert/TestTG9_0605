//-----------------------------------------------------------------------
// <copyright file = "IVersion.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    /// <summary>
    /// The interface for storing version information.
    /// </summary>
    public interface IVersion
    {
        /// <summary>
        /// The major version of the category.
        /// </summary>
        uint MajorVersion { get; set; }

        /// <summary>
        /// The minor version of the category.
        /// </summary>
        uint MinorVersion { get; set; }
    }
}
