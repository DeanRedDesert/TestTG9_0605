// -----------------------------------------------------------------------
// <copyright file = "PlayerBettableZeroBalanceEventArgs.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event occurs when money-on-machine status is changed.
    /// </summary>
    [Serializable]
    public sealed class MoneyOnMachineStatusEventArgs : NonTransactionalEventArgs
    {
        /// <summary>
        /// Gets the flag indicating whether there is money on EGM,
        /// i.e. whether the player still has money to play.
        /// </summary>
        public bool MoneyOnMachine { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="MoneyOnMachineStatusEventArgs"/>.
        /// </summary>
        /// <param name="moneyOnMachine">Whether there is money on machine.</param>
        public MoneyOnMachineStatusEventArgs(bool moneyOnMachine)
        {
            MoneyOnMachine = moneyOnMachine;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("MoneyOnMachineStatusEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t MoneyOnMachine: " + MoneyOnMachine);

            return builder.ToString();
        }

        #endregion
    }
}