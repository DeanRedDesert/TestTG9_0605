// -----------------------------------------------------------------------
// <copyright file = "IExtensionLib.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Platform.Interfaces;
    using TiltControl;

    /// <summary>
    /// The interface to the Foundation used by extensions.
    /// </summary>
    public interface IExtensionLib
    {
        #region Events

        /// <summary>
        /// Occurs when the extension should change to a new context.
        /// </summary>
        event EventHandler<NewContextEventArgs> NewContextEvent;

        /// <summary>
        /// Event handler of ActivateContextEvent.
        /// </summary>
        event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <summary>
        /// Event handler of InactivateContextEvent.
        /// </summary>
        event EventHandler<InactivateContextEventArgs> InactivateContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new system-extension context.
        /// </summary>
        event EventHandler<NewSystemExtensionContextEventArgs> NewSystemExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should activate a new system-extension context.
        /// </summary>
        /// <devdoc>
        /// This will only be sent after a new system-extension context message has been sent.
        /// </devdoc>
        event EventHandler<ActivateSystemExtensionContextEventArgs> ActivateSystemExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should inactivate the current system-extension context.
        /// </summary>
        /// <devdoc>
        /// After this message, the extension must be ready to receive a NewSystemContext message
        /// or receive a GetSystemApiVersions message for re-negotiation.
        /// </devdoc>
        event EventHandler<InactivateSystemExtensionContextEventArgs> InactivateSystemExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new theme-extension context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="NewAscribedThemeContextEvent"/> or <see cref="NewAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        event EventHandler<NewAscribedThemeContextEventArgs> NewAscribedThemeContextEvent;

        /// <summary>
        /// Occurs when the extension should activate a new theme-extension context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="ActivateAscribedThemeContextEvent"/> or <see cref="ActivateAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        /// <devdoc>
        /// This will only be sent after a new theme-extension context message has been sent.
        /// </devdoc>
        event EventHandler<ActivateAscribedThemeContextEventArgs> ActivateAscribedThemeContextEvent;

        /// <summary>
        /// Occurs when the extension should inactivate the current theme-extension context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="InactivateAscribedThemeContextEvent"/> or <see cref="InactivateAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        /// <devdoc>
        /// After this message, the extension must be ready to receive a NewThemeContext message
        /// or receive a GetThemeApiVersions message for re-negotiation.
        /// </devdoc>
        event EventHandler<InactivateAscribedThemeContextEventArgs> InactivateAscribedThemeContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new ascribed shell context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="NewAscribedThemeContextEvent"/> or <see cref="NewAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        event EventHandler<NewAscribedShellContextEventArgs> NewAscribedShellContextEvent;

        /// <summary>
        /// Occurs when the extension should activate a new ascribed shell context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="ActivateAscribedThemeContextEvent"/> or <see cref="ActivateAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        /// <devdoc>
        /// This will only be sent after a new ascribed shell context message has been sent.
        /// </devdoc>
        event EventHandler<ActivateAscribedShellContextEventArgs> ActivateAscribedShellContextEvent;

        /// <summary>
        /// Occurs when the extension should inactivate the current ascribed shell context.
        /// </summary>
        /// <remarks>
        /// There will be either <see cref="InactivateAscribedThemeContextEvent"/> or <see cref="InactivateAscribedShellContextEvent"/>
        /// being posted, depending on whether the extension is linked to a theme or shell. Therefore, the extension should handle event
        /// according to the <see cref="AscribedGameType"/>.
        /// </remarks>
        /// <devdoc>
        /// After this message, the extension must be ready to receive a NewAscribedShellContext message
        /// or receive a GetAscribedShellApiVersions message for re-negotiation.
        /// </devdoc>
        event EventHandler<InactivateAscribedShellContextEventArgs> InactivateAscribedShellContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new TSM-extension context.
        /// </summary>
        event EventHandler<NewTsmExtensionContextEventArgs> NewTsmExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should activate a new TSM-extension context.
        /// </summary>
        /// <devdoc>
        /// This will only be sent after a new TSM-extension context message has been sent.
        /// </devdoc>
        event EventHandler<ActivateTsmExtensionContextEventArgs> ActivateTsmExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should inactivate the current TSM-extension context.
        /// </summary>
        /// <devdoc>
        /// After this message, the extension must be ready to receive a NewTsmContext message 
        /// or receive a GetTsmApiVersions message for re-negotiation.
        /// </devdoc>
        event EventHandler<InactivateTsmExtensionContextEventArgs> InactivateTsmExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new theme extension context without inactivating the
        /// current one first.
        /// </summary>
        event EventHandler<SwitchThemeExtensionContextEventArgs> SwitchThemeExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should change to a new app-extension context.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<NewAppExtensionContextEventArgs> NewAppExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should activate a new app-extension context.
        /// </summary>
        /// <remarks>
        /// Use <see cref="AppExtensionContext"/> to access the information on the
        /// app-extension being activated.
        /// </remarks>
        /// <devdoc>
        /// This will only be sent after a new app-extension context message has been sent.
        /// </devdoc>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<ActivateAppExtensionContextEventArgs> ActivateAppExtensionContextEvent;

        /// <summary>
        /// Occurs when the extension should inactivate the current app-extension context.
        /// </summary>
        /// <devdoc>
        /// After this message, the extension must be ready to receive a NewAppContext message
        /// or receive a GetAppApiVersions message for re-negotiation.
        /// </devdoc>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<InactivateAppExtensionContextEventArgs> InactivateAppExtensionContextEvent;

        /// <summary>
        /// Occurs when the display control state is changed.
        /// </summary>
        /// <remarks>
        /// This event is available for app-extension only, and it works along with app-extension context only.
        /// </remarks>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <summary>
        /// Occurs when the Foundation shuts down the extension executable.
        /// </summary>
        /// <remarks>
        /// Handler of this event must be thread safe.
        /// </remarks>
        event EventHandler ShutDownEvent;

        #endregion

        #region Properties

        /// <summary>
        /// The entity of the ascribed game linked to the extension.
        /// </summary>
        AscribedGameEntity AscribedGameEntity { get; }

        /// <summary>
        /// The mode of the context in which the current linked theme/shell runs.
        /// </summary>
        GameMode AscribedGameMode { get; }

#pragma warning disable 1584, 1581, 1580
        /// <summary>
        /// Gets the display control state of the app extension.
        /// </summary>
        /// <remarks>
        /// This is valid for app-extension only.
        /// For non-app-extension, it will always be <see cref="DisplayControlState.Invalid"/>.
        /// </remarks>
#pragma warning restore 1584, 1581, 1580
        DisplayControlState DisplayControlState { get; }

        /// <summary>
        /// Gets the interface for requesting information about custom configuration items.
        /// </summary>
        IConfigurationRead ConfigurationRead { get; }

        /// <summary>
        /// Gets the interface for localization information.
        /// </summary>
        ILocalizationInformation LocalizationInformation { get; }

        /// <summary>
        /// Gets the interface for requesting culture information.
        /// </summary>
        ICultureRead CultureRead { get; }

        /// <summary>
        /// Gets the unified interface for extension parcel communication.
        /// </summary>
        IExtensionParcelComm ExtensionParcelComm { get; }

        /// <summary>
        /// Gets the interface for critical data accessor.
        /// </summary>
        ICriticalDataAccessor CriticalDataAccessor { get; }

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        IExtensionImportCollection ExtensionImportCollection { get; }

        /// <summary>
        /// Gets the interface for requesting information on game themes and paytables.
        /// </summary>
        IGameInformation GameInformation { get; }

        /// <summary>
        /// The implementation of <see cref="IExtensionTiltController"/>.
        /// </summary>
        IExtensionTiltController ExtensionTiltController { get; }

        /// <summary>
        /// The implementation of <see cref="IBankStatus"/>.
        /// </summary>
        IBankStatus BankStatus { get; }

        /// <summary>
        /// Gets the context of current active app-extension.
        /// </summary>
        /// <remarks>
        /// This is available for app-extension only.
        /// For non-app-extensions, it will always be null.
        /// An app extension context is activated and inactivated by
        /// <see cref="ActivateAppExtensionContextEvent"/> and <see cref="InactivateAppExtensionContextEvent"/>.
        /// </remarks>
        IAppExtensionContext AppExtensionContext { get; }

        #endregion

        #region Methods

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
