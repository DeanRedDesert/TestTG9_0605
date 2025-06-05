//-----------------------------------------------------------------------
// <copyright file = "PidConfigurationChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for pid configuration being changed.
    /// </summary>
    [Serializable]
    public class PidConfigurationChangedEventArgs : TransactionalEventArgs
    {
    }
}
