// -----------------------------------------------------------------------
// <copyright file = "IPlatformFactory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System.Collections.Generic;
    using CoplayerLib.Interfaces;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Interfaces;
    using ShellLib.Interfaces;

    /// <summary>
    /// This interface defines APIs for a factory making objects of types under Platform namespace.
    /// </summary>
    public interface IPlatformFactory
    {
        /// <summary>
        /// Creates a new shell lib instance.
        /// </summary>
        /// <param name="interfaceExtensionConfigurations">
        /// Interface extensions that are expected to be provided by the shell lib.
        /// </param>
        /// <param name="parcelCommMock">
        /// Parcel comm mock object to be used by the shell lib.
        /// </param>
        /// <returns>
        /// An instance of shell lib.
        /// </returns>
        IShellLibRestricted CreateShellLib(IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations,
                                           IGameParcelComm parcelCommMock = null);

        /// <summary>
        /// Creates a new coplayer lib instance.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer ID to be used by the lib.
        /// </param>
        /// <param name="sessionId">
        /// The session ID to be used by the lib.
        /// </param>
        /// <param name="interfaceExtensionConfigurations">
        /// Interface extensions that are expected to be provided by the coplayer lib.
        /// </param>
        /// <returns>
        /// An instance of coplayer lib.
        /// </returns>
        ICoplayerLibRestricted CreateCoplayerLib(int coplayerId,
                                                 int sessionId,
                                                 IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations);
    }
}
