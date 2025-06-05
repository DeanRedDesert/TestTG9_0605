// -----------------------------------------------------------------------
// <copyright file = "ActivateAscribedThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should activate a new Theme-Extension context.
    /// </summary>
    /// <devdoc>
    /// This will only be sent after a new theme context message has been sent.
    /// </devdoc>
    public class ActivateAscribedThemeContextEventArgs : TransactionalEventArgs
    {
    }
}
