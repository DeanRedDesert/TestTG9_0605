// -----------------------------------------------------------------------
// <copyright file = "MoneyChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// The generic money event notifying that money has changed.
    /// </summary>
    [Serializable]
    public class MoneyChangedEventArgs : TransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the specific money changed event type.
        /// </summary>
        public MoneyChangedEventType MoneyChangedEventType { get; private set; }

        /// <summary>
        /// Gets the player meters. All the player meters are always set up with the post-change values.
        /// </summary>
        public GamingMeters GamingMeters { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of base event <see cref="MoneyChangedEventArgs"/>.
        /// </summary>
        /// <param name="moneyChangedEventType">The money event type.</param>
        /// <param name="gamingMeters">The player meters.</param>
        protected MoneyChangedEventArgs(MoneyChangedEventType moneyChangedEventType, GamingMeters gamingMeters)
        {
            MoneyChangedEventType = moneyChangedEventType;
            GamingMeters = gamingMeters;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("MoneyChangedEventArgs -");
            builder.AppendLine("\t MoneyChangedEventType: " + MoneyChangedEventType);
            builder.AppendLine("\t " + GamingMeters);

            return builder.ToString();
        }

        #endregion
    }
}
