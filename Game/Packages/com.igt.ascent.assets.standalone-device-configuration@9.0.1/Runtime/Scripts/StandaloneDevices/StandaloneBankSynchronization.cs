//-----------------------------------------------------------------------
// <copyright file = "StandaloneBankSynchronization.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    using Core.Communication.Cabinet.CSI.Schemas;
    using Core.Communication.Cabinet.Standalone;
    using UnityEngine;

    /// <summary>
    /// Used to control the bank synchronization feature in standalone mode.
    /// </summary>
    [AddComponentMenu("Ascent SDK/Game Configuration/Standalone Bank Synchronization")]
    public class StandaloneBankSynchronization : MonoBehaviour, IBankSynchronizationSettings
    {
        [SerializeField]
        private bool featureEnabled;
        [SerializeField]
        private TimeFramePrecisionLevel precision;
        [SerializeField]
        private uint bankPosition;
        [SerializeField]
        private uint totalMachinesInBank;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public StandaloneBankSynchronization()
        {
            BankPosition = 1;
            TotalMachinesInBank = 1;
        }

        /// <summary>
        /// Gets the bank synchronization feature.
        /// </summary>
        public BankSynchronization Synchronizer
        {
            get;
            private set;
        }

        #region IBankSynchronizationSettings Members

        /// <inheritdoc />
        public uint BankPosition
        {
            get
            {
                return bankPosition;
            }
            set
            {
                bankPosition = value;
            }
        }

        /// <inheritdoc />
        public bool Enabled
        {
            get
            {
                return featureEnabled;
            }
            set
            {
                featureEnabled = value;
            }
        }

        /// <inheritdoc />
        public TimeFramePrecisionLevel Precision
        {
            get
            {
                return precision;
            }
            set
            {
                precision = value;
            }
        }

        /// <inheritdoc />
        public uint TotalMachinesInBank
        {
            get
            {
                return totalMachinesInBank;
            }
            set
            {
                totalMachinesInBank = value;
            }
        }

        #endregion

        /// <summary>
        /// Sets the object to use the control the bank synchronization.
        /// </summary>
        /// <param name="syncObject">The bank synchronization object.</param>
        public void SetBankSynchronizerObject(BankSynchronization syncObject)
        {
            Synchronizer = syncObject;
            if (Synchronizer != null)
            {
                Synchronizer.Enabled = Enabled;
                Synchronizer.Precision = Precision;
                Synchronizer.TotalMachinesInBank = TotalMachinesInBank;
                Synchronizer.BankPosition = BankPosition;
            }
        }
    }
}
