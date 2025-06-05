// -----------------------------------------------------------------------
// <copyright file = "IInterfaceExtensionInquiry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// Base interface used to provide inquiries to extensions of the platform interface.
    /// </summary>
    public interface IInterfaceExtensionInquiry
    {
        /// <summary>
        /// Gets the type of the interface. The value must derive from <see cref="IInterfaceExtensionInquiry"/>
        /// and must be an interface.
        /// </summary>
        Type InterfaceType { get; }
    }
}
