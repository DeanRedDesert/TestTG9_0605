// -----------------------------------------------------------------------
// <copyright file = "IShellConfigurator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System.Collections.Generic;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

    /// <summary>
    /// This interface defines information and functionalities that
    /// must be provided by the shell and game specific implementations.
    /// </summary>
    public interface IShellConfigurator
    {
        /// <summary>
        /// Gets the interface extensions requested by the shell.
        /// If granted, the interface extension can be accessed by
        /// <see cref="IShellLib.GetInterface{T}()"/>.
        /// </summary>
        /// <returns>
        /// List of interface extension configurations the shell wishes to have.
        /// Null of none is needed.
        /// </returns>
        IReadOnlyList<IInterfaceExtensionConfiguration> GetInterfaceExtensionRequests();

        /// <summary>
        /// Creates the shell's state machine.
        /// </summary>
        /// <remarks>
        /// Note that history is taken care of by the state machine framework,
        /// hence transparent to individual shell implementation.
        /// Therefore, this API will only be called for
        /// <see cref="Communication.Platform.Interfaces.GameMode.Play"/> mode and
        /// <see cref="Communication.Platform.Interfaces.GameMode.Utility"/> mode,
        /// but never for <see cref="Communication.Platform.Interfaces.GameMode.History"/> mode.
        /// </remarks>
        /// <param name="shellContext">
        /// The context for which the shell state machine is created.
        /// </param>
        /// <returns>
        /// The shell state machine created.
        /// </returns>
        IShellStateMachine CreateShellStateMachine(IShellContext shellContext);

        /// <summary>
        /// Gets the configurators for all cothemes packaged in the shell application,
        /// keyed by the G2SThemeID of the cothemes.
        /// </summary>
        IReadOnlyDictionary<string, IGameConfigurator> GetGameConfigurators();

        /// <summary>
        /// Gets the object that is responsible for routing incoming parcel calls
        /// from shell to the coplayers.
        /// </summary>
        /// <returns>
        /// The parcel call router object.
        /// <see langword="null"/> if no routing is needed, that is,
        /// all parcel calls will be handled only by the shell.
        /// </returns>
        IParcelCallRouter GetParcelCallRouter();

        /// <summary>
        /// Gets the mock implementation of <see cref="IGameParcelComm"/> used by the shell.
        /// This will be passed into the standalone implementation of <see cref="IShellLib"/>,
        /// and used by the <see cref="IShellLib.GameParcelComm"/> implementation.
        /// </summary>
        /// <remarks>
        /// This mock implementation allows the shell to run custom parcel comm during game development,
        /// such as playing the shell game in Unity Editor.
        /// 
        /// It is NOT used in standard running environment.
        /// </remarks>
        /// <returns>
        /// A mock implementation of <see cref="IGameParcelComm"/> provided and used by
        /// a shell when running in the standalone environment.
        /// 
        /// Null if none is needed.  In that case, SDK will use a default mock, which simply replies
        /// "parcel comm unavailable".
        /// </returns>
        IGameParcelComm GetStandaloneParcelCommMock();
    }
}