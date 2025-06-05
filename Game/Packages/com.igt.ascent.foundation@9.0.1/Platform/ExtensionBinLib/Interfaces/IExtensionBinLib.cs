// -----------------------------------------------------------------------
// <copyright file = "IExtensionBinLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using ExtensionLib.Interfaces;
    using ExtensionLib.Interfaces.TiltControl;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines functionality available for an Extension Bin application
    /// to communicate with the Foundation.
    /// </summary>
    /// <remarks>
    /// An Extension Bin is the application executable loaded by the Foundation.
    /// The executable may contain one or more Executable Extensions that are active on
    /// one or more negotiation levels, such as Link level, System level, App level,
    /// and Ascribed Game level etc.
    ///
    /// Link level is the most basic level of an application.  Its functionality is  represented by this ExtensionBinLib.
    /// Functionality on other levels will be exposed by corresponding "inner lib" interfaces, such as IAppExtensionLib,
    /// which are available as Properties of this ExtensionBinLib interface.
    /// </remarks>
    public interface IExtensionBinLib : IAppLib
    {
        #region Inner Libs

        /// <summary>
        /// Gets the interface for accessing System Extension specific functionality.
        /// </summary>
        ISystemExtensionLib SystemExtensionLib { get; }

        /// <summary>
        /// Gets the interface for accessing App Extension specific functionality.
        /// </summary>
        IAppExtensionLib AppExtensionLib { get; }

        /// <summary>
        /// Gets the interface for accessing functionality of an executable extension linked to a Game (either a Theme or a Shell).
        /// </summary>
        IAscribedGameExtensionLib AscribedGameExtensionLib { get; }

        /// <summary>
        /// Gets the interface for accessing functionality of an executable extension linked to a Chooser.
        /// </summary>
        IAscribedChooserExtensionLib AscribedChooserExtensionLib { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        IExtensionImportCollection ExtensionImportCollection { get; }

        /// <summary>
        /// Gets the unified interface for extension parcel communication.
        /// </summary>
        IExtensionParcelComm ExtensionParcelComm { get; }

        /// <summary>
        /// Gets the interface for querying config items related to localization.
        /// </summary>
        ILocalization Localization { get; }

        /// <summary>
        /// Gets the interface for requesting culture information.
        /// </summary>
        ICultureRead CultureRead { get; }

        /// <summary>
        /// Gets the interface for reading custom configuration items.
        /// </summary>
        IConfigurationRead ConfigurationRead { get; }

        /// <summary>
        /// Gets the interface for accessing critical data.
        /// </summary>
        ICriticalDataAccessor CriticalDataAccessor { get; }

        /// <summary>
        /// Gets the interface for requesting information on game themes and paytables.
        /// </summary>
        IGameInformation GameInformation { get; }

        /// <summary>
        /// Gets the interface for querying  bank status and listening for bank events.
        /// </summary>
        IBankStatus BankStatus { get; }

        /// <summary>
        /// Gets the interface for posting and clearing tilts.
        /// </summary>
        IExtensionTiltController ExtensionTiltController { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an extended interface if it was requested and installed on the Link level.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none was found.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class;

        /// <summary>
        /// Clears the configuration caches so that the configuration values will be re-queried from Foundation.
        /// This includes:
        /// <list type="bullet">
        /// <item>Some configuration items owned by Foundation.</item>
        /// <item>Custom configuration items owned by the Extension, if the cache is enabled when constructing Extension Lib.</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// The configuration caches are usually cleared by Extension Lib when any of its inner contexts is
        /// activated/re-activated. However, that might not be sufficient for those system extensions that
        /// are not linked to a game or chooser, hence only have one inner context, i.e the system context,
        /// which is only activated once when the extension is launched.
        ///
        /// Depending on specific Extension's implementation, those extensions might want to call this method
        /// to actively clear the configuration cache at appropriate times.  One recommendation is for the extension
        /// to use the PlayerControlContextMonitoring interface extension, and clear the configuration cache when
        /// PlayerControlContextState is changed to Normal.
        /// </remarks>
        void ClearConfigurationCache();

        #endregion
    }
}