// -----------------------------------------------------------------------
// <copyright file = "IShellLib.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines functionality of a shell communication lib
    /// to be used by shell clients.
    /// </summary>
    public interface IShellLib : IAppLib
    {
        /// <summary>
        /// Gets the current context of the shell, including the information
        /// on the mount point, and game mode etc.
        /// </summary>
        IShellContext Context { get; }

        /// <summary>
        /// Gets the maximum number of coplayers allowed to run at a time as defined by the foundation.
        /// </summary>
        int MaxNumCoplayers { get; }

        /// <summary>
        /// Gets the interface for access to the critical data store for the shell.
        /// </summary>
        ICriticalDataStore ShellStore { get; }

        /// <summary>
        /// Gets the interface for the shell to parcel communicate with the extension.
        /// </summary>
        IGameParcelComm GameParcelComm { get; }

        /// <summary>
        /// Gets the interface for a game to talk to the Foundation in terms of Bank Play, such as getting
        /// the player meters, Bank Play status, money events and machine wide bet constraints etc.
        /// </summary>
        IBankPlay BankPlay { get; }
        
        /// <summary>
        /// Gets the interface for querying the game play status, 
        /// and getting notifications when any of the game play status has changed.
        /// </summary>
        IGamePlayStatus GamePlayStatus { get;  }

        /// <summary>
        /// Gets the interface for querying config items related to game presentation behavior.
        /// </summary>
        IGamePresentationBehavior GamePresentationBehavior { get; }

        /// <summary>
        /// Gets the interface for querying config items related to localization.
        /// </summary>
        ILocalization Localization { get; }

        /// <summary>
        /// Gets the interface for querying and changing the culture for the game application.
        /// </summary>
        IGameCulture GameCulture { get; }

        /// <summary>
        /// Gets the interface for the shell to be able to check if the chooser is currently available,
        /// and request the chooser if needed.
        /// </summary>
        IChooserServices ChooserServices { get; }

        /// <summary>
        /// Gets the interface for the shell to be able to check the status of the EGM's current environment,
        /// and request to add money if needed in show mode.
        /// </summary>
        IShowDemo ShowDemo { get; }

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        IExtensionImportCollection ExtensionImportCollection { get; }

        /// <summary>
        /// Allows for the game to send tilts to the foundation.
        /// </summary>
        IShellTiltController TiltController { get; }

        /// <summary>
        /// Gets an extended interface if it was requested and installed.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of. 
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed, then <see langword="null"/> will
        /// be returned.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>()
            where TExtendedInterface : class;
    }
}