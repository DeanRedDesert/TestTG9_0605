// -----------------------------------------------------------------------
// <copyright file = "ActivateAppExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should activate a new App-Extension context.
    /// </summary>
    /// <devdoc>
    /// This will only be sent after a new app context message has been sent.
    /// </devdoc>
    public class ActivateAppExtensionContextEventArgs : TransactionalEventArgs
    {
    }
}
