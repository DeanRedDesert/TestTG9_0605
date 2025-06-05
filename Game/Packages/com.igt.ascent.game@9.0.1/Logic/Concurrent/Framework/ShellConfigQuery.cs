// -----------------------------------------------------------------------
// <copyright file = "ShellConfigQuery.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Communication.Logic.CommServices;
    using Game.Core.Communication.LogicPresentationBridge;
    using Interfaces;

    /// <summary>
    /// An implementation of <see cref="IShellConfigQuery"/>.
    /// </summary>
    internal sealed class ShellConfigQuery : IShellConfigQuery
    {
        #region Private Fields

        /// <summary>
        /// Object for the shell to query restricted information from Foundation.
        /// </summary>
        private readonly IShellLibRestricted shellLibRestricted;

        /// <summary>
        /// Object for the shell to query information from Foundation.
        /// </summary>
        private readonly IShellLib shellLib;

        /// <summary>
        /// The game configurators defined in the shell configurator.
        /// </summary>
        private readonly IReadOnlyDictionary<string, IGameConfigurator> gameConfigurators;

        /// <summary>
        /// The object managing the GL2P comm channels for shell and all coplayers.
        /// </summary>
        private readonly Gl2PCommManager gl2PCommManager;

        /// <summary>
        /// The object managing the service request data for shell and coplayers.
        /// </summary>
        private IServiceRequestDataManager serviceRequestDataManager;

        #endregion

        #region Constructors

        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal ShellConfigQuery(IShellLib shellLib,
                                  IShellLibRestricted shellLibRestricted,
                                  IReadOnlyDictionary<string, IGameConfigurator> gameConfigurators,
                                  Gl2PCommManager gl2PCommManager)
        {
            this.shellLib = shellLib;
            this.shellLibRestricted = shellLibRestricted;
            this.gameConfigurators = gameConfigurators;
            this.gl2PCommManager = gl2PCommManager;
        }

        #endregion

        #region Public Methods

        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        public void Initialize(IServiceRequestDataManager requestDataManager)
        {
            serviceRequestDataManager = requestDataManager;
        }

        #endregion

        #region IShellConfigQuery Implementation

        // This interface is to be used by coplayer runners.
        // Therefore, its implementation must be thread safe.

        /// <inheritdoc/>
        public IGameConfigurator GetGameConfigurator(string g2SThemeId)
        {
            lock(gameConfigurators)
            {
                if(!gameConfigurators.TryGetValue(g2SThemeId, out var result))
                {
                    throw new ConcurrentLogicException("No game configurator is defined for G2S Theme: " + g2SThemeId);
                }

                return result;
            }
        }

        /// <inheritdoc/>
        public ILogicCommServices GetLogicCommServices(string g2SThemeId, int coplayerId)
        {
            return gl2PCommManager.CreateCommServices(new CothemePresentationKey(coplayerId, g2SThemeId));
        }

        /// <inheritdoc/>
        public IDictionary<string, ServiceRequestData> GetServiceRequestDataMap(string g2SThemeId)
        {
            return serviceRequestDataManager.LoadServiceRequestData(g2SThemeId);
        }

        /// <inheritdoc/>
        public CoplayerInitData GetCoplayerInitData()
        {
            // This call is to be called when a GameStateMachineFramework is initialized.
            // By that time, all caching in Shell Lib is supposed to have completed.
            // Locking is not used to simplify code as all of the operations here are read only.

            CoplayerInitData result;
            switch(shellLib.Context.GameMode)
            {
                case GameMode.Play:
                {
                    var bankPlayProperties = shellLib.BankPlay.BankPlayProperties;
                    var playerMeters = shellLib.BankPlay.GamingMeters;

                    result = new CoplayerInitData
                                 {
                                     // Config data
                                     MachineWideBetConstraints = shellLib.BankPlay.MachineWideBetConstraints,
                                     CreditFormatter = shellLib.Localization.CreditFormatter,
                                     MinimumBaseGameTime = shellLib.GamePresentationBehavior.MinimumBaseGameTime,
                                     MinimumFreeSpinTime = shellLib.GamePresentationBehavior.MinimumFreeSpinTime,
                                     CreditMeterBehavior = shellLib.GamePresentationBehavior.CreditMeterBehavior,
                                     MaxBetButtonBehavior = shellLib.GamePresentationBehavior.MaxBetButtonBehavior,
                                     DisplayVideoReelsForStepper = shellLib.GamePresentationBehavior.DisplayVideoReelsForStepper,

                                     // Initial value of display control state
                                     DisplayControlState = shellLibRestricted.DisplayControlState,

                                     // Initial values of context properties
                                     CanBet = bankPlayProperties.CanBet,
                                     CanCommitGameCycle = bankPlayProperties.CanCommitGameCycle,
                                     PlayerBettable = playerMeters.Bettable,
                                     Culture = shellLib.GameCulture.Culture,
                                 };
                        break;
                }
                case GameMode.History:
                {
                    result = new CoplayerInitData { DisplayControlState = shellLibRestricted.DisplayControlState, };
                    break;
                }
                default:
                {
                    var message = $"GetCoplayerInitData in game mode {shellLib.Context.GameMode} is not supported.";
                    throw new NotSupportedException(message);
                }
            }

            return result;
        }

        #endregion
    }
}