// -----------------------------------------------------------------------
// <copyright file = "MoneySetEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that the Foundation forcibly sets one or more of the gaming meters to a new value.
    /// </summary>
    [Serializable]
    public sealed class MoneySetEventArgs : MoneyChangedEventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneySetEventArgs"/>.
        /// </summary>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneySetEventArgs(GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneySet, gamingMeters)
        {
        }

        #endregion
    }
}
