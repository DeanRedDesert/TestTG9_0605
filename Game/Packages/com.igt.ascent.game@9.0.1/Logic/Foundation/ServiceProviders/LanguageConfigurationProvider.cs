//-----------------------------------------------------------------------
// <copyright file = "LanguageConfigurationProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Services;

    /// <summary>
    /// Language configuration provider is used to retrieve the language configuration information.
    /// </summary>
    public class LanguageConfigurationProvider : IGameLibEventListener, INotifyAsynchronousProviderChanged
    {
        #region Private Members

        /// <summary>
        /// The IGameLib instance which is used to communicate with the foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Language Configuration Provider.
        /// </summary>
        /// <param name="gameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        public LanguageConfigurationProvider(IGameLib gameLib)
        {
            gameLib.LanguageChangedEvent += OnLanguageChanged;
            this.gameLib = gameLib;
        }

        #endregion

        /// <summary>
        /// Get the current language name of the game.
        /// </summary>
        [AsynchronousGameService]
        public string GameLanguage
        {
            get { return gameLib.GameLanguage; }
        }

        /// <summary>
        /// Get the list of enabled languages.
        /// </summary>
        [GameService]
        public List<string> EnabledLanguages
        {
            get { return gameLib.AvailableLanguages.ToList(); }
        }

        /// <summary>
        /// Get the next language from the available languages by sequence.
        /// If current language is the last one in sequnce, the first one will return.
        /// </summary>
        [AsynchronousGameService]
        public string NextLanguage
        {
            get
            {
                var enabledLanguages = gameLib.AvailableLanguages.ToList();
                var languageIndex = enabledLanguages.IndexOf(GameLanguage);
                return languageIndex < enabledLanguages.Count - 1
                           ? enabledLanguages[languageIndex + 1]
                           : enabledLanguages[0];
            }
        }

        /// <summary>
        /// Get the flag indicating whether the language switch is enabled.
        /// </summary>
        [GameService]
        public bool LanguageSwitchEnabled
        {
            get { return gameLib.AvailableLanguages.Count > 1; }
        }

        #region INotifyAsynchronousProviderChanged Members

        /// <inherit />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region IGameLibEventListener Members

        /// <summary>
        /// Unregister all the Game Lib event handlers that have been registered.
        /// </summary>
        /// <param name="iGameLib">Reference to the Game Lib.</param>
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            iGameLib.LanguageChangedEvent -= OnLanguageChanged;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The handler of LanguageChangedEvent.
        /// </summary>
        /// <param name="sender">The object triggering the LanguageChangedEvent.</param>
        /// <param name="languageChangedEventArgs">The payload of the LanguageChangedEvent.</param>
        private void OnLanguageChanged(object sender, LanguageChangedEventArgs languageChangedEventArgs)
        {
            var handle = AsynchronousProviderChanged;
            if(handle != null)
            {
                handle(this, new AsynchronousProviderChangedEventArgs(
                      new List<ServiceSignature> 
                                      { 
                                          new ServiceSignature("GameLanguage"),
                                          new ServiceSignature("NextLanguage")
                                      }, true
                      ));
            }
        }

        #endregion
    }
}