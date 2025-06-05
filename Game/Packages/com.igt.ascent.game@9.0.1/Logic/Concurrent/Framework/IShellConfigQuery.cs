// -----------------------------------------------------------------------
// <copyright file = "IShellConfigQuery.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.Collections.Generic;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Communication.Logic.CommServices;
    using Interfaces;

    /// <summary>
    /// This interface defines functionalities for coplayer runner to query shell
    /// about the configuration info on a specific cotheme/coplayer.
    /// </summary>
    internal interface IShellConfigQuery
    {
        /// <summary>
        /// Gets the game configurator of a cotheme.
        /// </summary>
        /// <param name="g2SThemeId">The G2S theme id of the cotheme.</param>
        /// <returns>The game configurator of the cotheme.</returns>
        IGameConfigurator GetGameConfigurator(string g2SThemeId);

        /// <summary>
        /// Gets the logic comm services to use for a specific cotheme in a specific coplayer.
        /// </summary>
        /// <param name="g2SThemeId">The G2S theme id of the cotheme.</param>
        /// <param name="coplayerId">The coplayer id.</param>
        /// <returns>The logic comm services to use.</returns>
        ILogicCommServices GetLogicCommServices(string g2SThemeId, int coplayerId);

        /// <summary>
        /// Gets the service request data configured for a cotheme by LDC.
        /// </summary>
        /// <param name="g2SThemeId">The G2S theme id of the cotheme.</param>
        /// <returns>The collection of service request data.</returns>
        IDictionary<string, ServiceRequestData> GetServiceRequestDataMap(string g2SThemeId);

        /// <summary>
        /// Gets the initial data for a coplayer to start running.
        /// This includes both static configuration data kept by the shell and
        /// runtime data tracked by the shell.
        /// </summary>
        /// <remarks>
        /// For runtime data, these are the values at the moment of calling.
        /// Future update will be pushed to the coplayer by shell via various shell events.
        /// </remarks>
        /// <returns>The initial data for the coplayer.</returns>
        CoplayerInitData GetCoplayerInitData();
    }
}