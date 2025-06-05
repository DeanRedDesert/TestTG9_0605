//-----------------------------------------------------------------------
// <copyright file = "BankSynchronizationInformation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Represents the bank synchronization parameters.
    /// </summary>
    public class BankSynchronizationInformation
    {
        /// <summary>
        /// Construct a new instance given the synchronization parameters.
        /// </summary>
        /// <param name="enabled">If the feature is enabled or not.</param>
        /// <param name="currentTime">The current time in milliseconds.</param>
        /// <param name="precisionLevel">The synchronization precision level.</param>
        /// <param name="bankPosition">The machine position in the bank.</param>
        /// <param name="totalMachinesInBank">The total number of machines in a bank.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="currentTime"/> is negative.
        /// Thrown if <paramref name="bankPosition"/> is greater than <paramref name="totalMachinesInBank"/>.
        /// </exception>
        public BankSynchronizationInformation(bool enabled, long currentTime, TimeFramePrecisionLevel precisionLevel,
            uint bankPosition, uint totalMachinesInBank)
        {
            if(currentTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTime), "The time cannot be negative.");
            }

            if(bankPosition > totalMachinesInBank)
            {
                throw new ArgumentOutOfRangeException(nameof(bankPosition),
                    $"The bank position of {bankPosition} is greater than the total machine count of {totalMachinesInBank}.");
            }

            BankSynchronizationEnabled = enabled;
            CurrentTime = currentTime;
            PrecisionLevel = precisionLevel;
            BankPosition = bankPosition;
            TotalMachinesInBank = totalMachinesInBank;
        }

        #region Properties

        /// <summary>
        /// Gets if the bank synchronization feature is enabled.
        /// </summary>
        public bool BankSynchronizationEnabled
        {
            get;
        }

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        public long CurrentTime
        {
            get;
        }

        /// <summary>
        /// Gets the current bank precision level.
        /// </summary>
        public TimeFramePrecisionLevel PrecisionLevel
        {
            get;
        }

        /// <summary>
        /// Gets the position of the machine in the bank. This value is 1-based.
        /// </summary>
        public uint BankPosition
        {
            get;
        }

        /// <summary>
        /// Gets the total number of machines in the bank.
        /// </summary>
        public uint TotalMachinesInBank
        {
            get;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Enabled: {BankSynchronizationEnabled}, Precision: {PrecisionLevel}, Time: {CurrentTime}";
        }
    }
}
