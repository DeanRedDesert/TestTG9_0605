// -----------------------------------------------------------------------
// <copyright file = "ThemeActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionBinLib;
    using Ascent.Communication.Platform.ExtensionBinLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using F2XThemeActivation = F2X.Schemas.Internal.ThemeActivation;

    /// <summary>
    /// Handles ThemeActivation category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class ThemeActivationCallbackHandler : IThemeActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="ThemeActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public ThemeActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IThemeActivationCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessActivateThemeContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new ActivateAscribedThemeContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new ActivateInnerContextEventArgs<IAscribedGameContext>());

            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateThemeContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new InactivateAscribedThemeContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new InactivateInnerContextEventArgs<IAscribedGameContext>());

            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewThemeContext(PayvarIdentifier payvar, uint denomination, F2XThemeActivation.GameMode gameMode)
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new NewAscribedThemeContextEventArgs(payvar.Value, denomination, (GameMode)gameMode));

            // This event is used by ExtensionBinLib.
            var context = new AscribedGameContext((GameMode)gameMode,
                                                  new PaytableDenominationInfo(payvar.Value, denomination));

            eventCallbacks.PostEvent(new NewInnerContextEventArgs<IAscribedGameContext>(context));

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSwitchThemeContext(ThemeIdentifier theme, PayvarIdentifier payvar, uint denomination)
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new SwitchThemeExtensionContextEventArgs(theme.Value, payvar.Value, denomination));

            // This event is used by ExtensionBinLib.
            var context = new AscribedGameContext(
                paytableDenominationInfo: new PaytableDenominationInfo(payvar.Value, denomination),
                ascribedGameEntity: new AscribedGameEntity(AscribedGameType.Theme, theme.Value));

            eventCallbacks.PostEvent(new SwitchInnerContextEventArgs<IAscribedGameContext>(context));

            return null;
        }

        #endregion
    }
}
