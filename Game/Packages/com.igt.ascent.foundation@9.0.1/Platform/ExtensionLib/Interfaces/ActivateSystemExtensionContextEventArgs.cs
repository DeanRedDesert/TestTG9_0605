// -----------------------------------------------------------------------
// <copyright file = "ActivateSystemExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should activate a new System-Extension context.
    /// </summary>
    /// <devdoc>
    /// This will only be sent after a new system context message has been sent.
    /// </devdoc>
    public class ActivateSystemExtensionContextEventArgs : TransactionalEventArgs
    {
    }
}
