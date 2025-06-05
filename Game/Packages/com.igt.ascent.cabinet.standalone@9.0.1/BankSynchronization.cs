//-----------------------------------------------------------------------
// <copyright file = "BankSynchronization.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// The standalone implementation of the bank synchronization interface.
    /// </summary>
    [Serializable]
    public class BankSynchronization : IBankSynchronization, IBankSynchronizationSettings
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);
        private uint totalMachinesInBank;
        private uint bankPosition;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public BankSynchronization()
        {
            Precision = TimeFramePrecisionLevel.None;
            Enabled = false;
            TotalMachinesInBank = 1;
            BankPosition = 1;
        }

        #region IBankSynchronizationSettings Members

        /// <inheritdoc />
        public bool Enabled
        {
            get;
            set;
        }

        /// <inheritdoc />
        public TimeFramePrecisionLevel Precision
        {
            get;
            set;
        }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if the bank position is larger than the total number of machines in the bank.
        /// </exception>
        public uint BankPosition
        {
            get => bankPosition;
            set
            {
                if(value > TotalMachinesInBank)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The bank position of {value} cannot be larger than the total number of machines in the bank {TotalMachinesInBank}.");
                }

                bankPosition = value;
            }
        }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if the total number of machines is less than the bank position.
        /// </exception>
        public uint TotalMachinesInBank
        {
            get => totalMachinesInBank;
            set
            {
                if(value < BankPosition)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"The total number of machines in the bank {value} cannot be smaller than the current bank position of {BankPosition}");
                }

                totalMachinesInBank = value;
            }
        }

        #endregion

        #region IBankSynchronization Members

        /// <inheritdoc />
        public BankSynchronizationInformation GetSynchronizationStatus()
        {
            var currentTime = Convert.ToInt64((DateTime.UtcNow - Epoch).TotalMilliseconds);
            return new BankSynchronizationInformation(Enabled, currentTime, Precision, BankPosition, TotalMachinesInBank);
        }

        /// <inheritdoc />
        public bool GameEventsEnabled { get; private set; }

        /// <inheritdoc />
        public bool RegisterForGameEvents()
        {
            GameEventsEnabled = true;
            return GameEventsEnabled;
        }

        /// <inheritdoc />
        public void UnregisterForGameEvents()
        {
            GameEventsEnabled = false;
        }

        /// <inheritdoc />
        public void SendGameEvent(string message)
        {
            if(GameEventsEnabled)
            {
                StandaloneMessageReceive(message);
            }
        }

        /// <inheritdoc />
        public void CleanUpGameEvents()
        {
            GameEventsEnabled = false;
        }

        /// <summary>
        /// Force the game to receive a message in standalone. 
        /// </summary>
        /// <param name="message">The message to receive.</param>
        protected void StandaloneMessageReceive(string message)
        {
            GameMessageReceivedEvent?.Invoke(this, new GameMessageReceivedEventArgs(message));
        }
        
        /// <inheritdoc />
        public event EventHandler<GameMessageReceivedEventArgs> GameMessageReceivedEvent;

        #endregion
    }
}
