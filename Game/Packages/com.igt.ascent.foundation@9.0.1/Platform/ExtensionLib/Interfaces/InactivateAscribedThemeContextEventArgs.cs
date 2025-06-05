// -----------------------------------------------------------------------
// <copyright file = "InactivateAscribedThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should inactivate the current ascribed theme context. 
    /// </summary>
    /// <devdoc>
    /// After this message, the extension must be ready to receive a NewThemeContext message
    /// or receive a GetThemeApiVersions message for re-negotiation.
    /// </devdoc>
    public class InactivateAscribedThemeContextEventArgs : TransactionalEventArgs
    {
    }
}