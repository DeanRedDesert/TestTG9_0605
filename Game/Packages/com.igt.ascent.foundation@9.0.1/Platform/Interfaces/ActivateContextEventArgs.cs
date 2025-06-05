// -----------------------------------------------------------------------
// <copyright file = "ActivateContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{

    /// <summary>
    /// Event indicating the extension should activate the new context.
    /// </summary>
    /// <devdoc>
    /// This will only be sent after a new context message has been sent.
    /// </devdoc>
    public class ActivateContextEventArgs : TransactionalEventArgs
    {
    }
}
