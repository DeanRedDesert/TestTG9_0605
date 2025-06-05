//-----------------------------------------------------------------------
// <copyright file = "TransactionCommittedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// Event arguments which indicate that a transaction has been committed in the disk store manager.
    /// </summary>
    [Serializable]
    class TransactionCommittedEventArgs : EventArgs
    {
    }
}
