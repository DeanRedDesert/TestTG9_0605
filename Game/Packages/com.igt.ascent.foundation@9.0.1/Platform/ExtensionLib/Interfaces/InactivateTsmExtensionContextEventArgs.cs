// -----------------------------------------------------------------------
// <copyright file = "InactivateTsmExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should inactivate the current TSM-Extension context. 
    /// </summary>
    /// <devdoc>
    /// After this message, the extension must be ready to receive a NewTsmContext message 
    /// or receive a GetTsmApiVersions message for re-negotiation.
    /// </devdoc>
    public class InactivateTsmExtensionContextEventArgs : TransactionalEventArgs
    {
    }
}
