//-----------------------------------------------------------------------
// <copyright file = "StandaloneIdentificationManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions
{
    using System;
    using Interfaces;

    /// <summary>
    /// Implement and interface for retrieving machine identification data when running in standalone mode.
    /// </summary>
    internal sealed class StandaloneIdentificationManager : IStandaloneIdentificationManager
    {
        #region Private Fields

        /// <summary>
        /// Critical data path for storing the serial number.
        /// </summary>
        /// <remarks>For standalone purposes a serial number is generated and then persisted.</remarks>
        private static readonly string SerialNumberPath = typeof(StandaloneIdentificationManager) + "/SerialNumber";

        /// <summary>
        /// Critical data path for storing the G2S EGM identifier.
        /// </summary>
        /// <remarks>For standalone purposes a G2S EGM identifier is generated and then persisted.</remarks>
        private static readonly string G2SEgmIdentifierPath = typeof(StandaloneIdentificationManager) + "/G2SEgmIdentifier";

        /// <summary>
        /// Critical data path for storing the machine asset number.
        /// </summary>
        /// <remarks>For standalone purposes a machine asset number is generated and then persisted.</remarks>
        private static readonly string MachineAssetNumberPath = typeof(StandaloneIdentificationManager) + "/MachineAssetNumber";

        /// <summary>
        /// Interface for validating transactions.
        /// </summary>
        private readonly ITransactionWeightVerificationDependency transactionVerifier;

        /// <summary>
        /// Interface to use for persisting data.
        /// </summary>
        private readonly IStandaloneCriticalDataDependency criticalDataProvider;

        /// <summary>
        /// The current value for the machine serial number; cached the first time it is requested.
        /// </summary>
        private string serialNumberValue;

        /// <summary>
        /// The current value for the G2X EGM Identifier; cached the first time it is requested.
        /// </summary>
        private string g2SEgmIdentifierValue;

        /// <summary>
        /// The current value for the machine asset number; cached the first time it is requested.
        /// </summary>
        private uint? machineAssetNumberValue;

        /// <summary>
        /// The value for the machine floor location.
        /// </summary>
        private readonly string machineFloorLocationValue = "Main Floor";

        #endregion

        #region Public Constants

        /// <summary>
        /// The maximum value for the machine asset number (defined in default.config).
        /// </summary>
        public const uint MaxMachineAssetNumber = 999999999;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct an instance of the <see cref="StandaloneIdentificationManager"/> class.
        /// </summary>
        /// <param name="transactionVerifier">Interface to use for validating transactions.</param>
        /// <param name="criticalDataProvider">Interface used for persisting data to the platform.</param>
        public StandaloneIdentificationManager(ITransactionWeightVerificationDependency transactionVerifier,
                                               IStandaloneCriticalDataDependency criticalDataProvider)
        {
            this.transactionVerifier = transactionVerifier ?? throw new InterfaceExtensionDependencyException("transactionVerifier");
            this.criticalDataProvider = criticalDataProvider ?? throw new InterfaceExtensionDependencyException("criticalDataProvider");
        }

        #endregion

        #region IStandaloneIdentificationManager

        /// <inheritdoc/>
        public string GetMachineSerialNumber()
        {
            transactionVerifier.MustHaveHeavyweightTransaction();

            return serialNumberValue ?? ReadMachineSerialNumber();
        }

        /// <inheritdoc/>
        public string GetG2SEgmIdentifier()
        {
            transactionVerifier.MustHaveHeavyweightTransaction();

            return g2SEgmIdentifierValue ?? ReadG2SEgmIdentifier();
        }

        /// <inheritdoc/>
        public uint GetMachineAssetNumber()
        {
            transactionVerifier.MustHaveHeavyweightTransaction();

            return machineAssetNumberValue ?? ReadMachineAssetNumber();
        }

        /// <inheritdoc/>
        public string GetMachineFloorLocation()
        {
            transactionVerifier.MustHaveHeavyweightTransaction();

            return machineFloorLocationValue;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieve the value of the machine serial number from critical data; if it has not been set, generate a GUID for
        /// the serial number, and store it in critical data.
        /// </summary>
        /// <returns>The value of the G2S EGM Identifier.</returns>
        private string ReadMachineSerialNumber()
        {
            lock(SerialNumberPath)
            {
                serialNumberValue = criticalDataProvider.ReadFoundationData<string>(InterfaceExtensionDataScope.Theme,
                                                                                    SerialNumberPath);

                if(string.IsNullOrEmpty(serialNumberValue))
                {
                    serialNumberValue = GenerateAndStoreSerialNumber();
                }
            }

            return serialNumberValue;
        }

        /// <summary>
        /// Retrieve the value of the G2S EGM Identifier from critical data; if it has not been set, generate a GUID for
        /// the identifier, and store it in critical data.
        /// </summary>
        /// <returns>The value of the G2S EGM Identifier.</returns>
        private string ReadG2SEgmIdentifier()
        {
            lock(G2SEgmIdentifierPath)
            {
                g2SEgmIdentifierValue =
                    criticalDataProvider.ReadFoundationData<string>(InterfaceExtensionDataScope.Theme,
                                                                    G2SEgmIdentifierPath);

                if(string.IsNullOrEmpty(g2SEgmIdentifierValue))
                {
                    g2SEgmIdentifierValue = GenerateAndStoreG2SEgmIdentifier();
                }
            }

            return g2SEgmIdentifierValue;
        }

        /// <summary>
        /// Retrieve the value of the machine asset number from critical data; if it has not been set, generate a GUID for
        /// the asset number, and store it in critical data.
        /// </summary>
        /// <returns>The value of the G2S EGM Identifier.</returns>
        private uint ReadMachineAssetNumber()
        {
            lock(MachineAssetNumberPath)
            {
                machineAssetNumberValue =
                    criticalDataProvider.ReadFoundationData<uint?>(InterfaceExtensionDataScope.Theme,
                                                                   MachineAssetNumberPath) ??
                                                                   GenerateAndStoreAssetNumber();
            }

            return machineAssetNumberValue.Value;
        }

        /// <summary>
        /// Generate a GUID to use as the machine serial number, and store it in critical data for future reference.
        /// </summary>
        /// <returns>Returns the generated serial number.</returns>
        private string GenerateAndStoreSerialNumber()
        {
            var serialNumber = Guid.NewGuid().ToString();
            criticalDataProvider.WriteFoundationData(InterfaceExtensionDataScope.Theme,
                                                     SerialNumberPath,
                                                     serialNumber);

            return serialNumber;
        }

        /// <summary>
        /// Generate a GUID to use as the G2S EGM identifier, and store it in critical data for future reference.
        /// </summary>
        /// <returns>Returns the generated G2S EGM identifier.</returns>
        private string GenerateAndStoreG2SEgmIdentifier()
        {
            var identifier = Guid.NewGuid().ToString();
            criticalDataProvider.WriteFoundationData(InterfaceExtensionDataScope.Theme,
                                                     G2SEgmIdentifierPath,
                                                     identifier);

            return identifier;
        }

        /// <summary>
        /// Generate a random value to use as the asset number, and store it in critical data for future reference.
        /// </summary>
        /// <returns>Returns the generated asset number.</returns>
        private uint GenerateAndStoreAssetNumber()
        {
            var assetNumber = (uint)new Random((int)DateTime.Now.Ticks & 0x0000FFFF).Next(0, (int)MaxMachineAssetNumber);
            criticalDataProvider.WriteFoundationData(InterfaceExtensionDataScope.Theme, MachineAssetNumberPath,
                                                     assetNumber);

            return assetNumber;
        }

        #endregion
    }
}
