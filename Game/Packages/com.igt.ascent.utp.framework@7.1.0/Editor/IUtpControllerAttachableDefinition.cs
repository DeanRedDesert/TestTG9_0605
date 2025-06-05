// -----------------------------------------------------------------------
//  <copyright file = "IUtpControllerAttachableDefinition.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Editor
{
    using System.Collections.Generic;

    /// <summary>
    /// This type defines the interface to provide attachable component types to UtpController game object.
    /// </summary>
    public interface IUtpControllerAttachableDefinition
    {
        /// <summary>
        /// Get a list of type namespaces that can be attached to UtpController game object.
        /// </summary>
        /// <returns>A list of type namespaces that can be attached to UtpController game object.</returns>
        List<string> GetAttachableNamespaces();
    }
}