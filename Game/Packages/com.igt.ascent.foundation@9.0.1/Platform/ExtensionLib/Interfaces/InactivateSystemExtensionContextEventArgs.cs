// -----------------------------------------------------------------------
// <copyright file = "InactivateSystemExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should inactivate the current System-Extension context. 
    /// </summary>
    /// <devdoc>
    /// After this message, the extension must be ready to receive a NewSystemContext message
    /// or receive a GetSystemApiVersions message for re-negotiation.
    /// </devdoc>
    public class InactivateSystemExtensionContextEventArgs : TransactionalEventArgs
    {
    }
}