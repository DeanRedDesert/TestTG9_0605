// -----------------------------------------------------------------------
// <copyright file = "InactivateAppExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should inactivate the current App-Extension context.
    /// </summary>
    /// <devdoc>
    /// After this message, the extension must be ready to receive a NewAppContext message
    /// or receive a GetAppApiVersions message for re-negotiation.
    /// </devdoc>
    public class InactivateAppExtensionContextEventArgs : TransactionalEventArgs
    {
    }
}
