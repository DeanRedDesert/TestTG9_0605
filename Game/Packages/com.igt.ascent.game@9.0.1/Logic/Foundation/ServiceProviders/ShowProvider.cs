//-----------------------------------------------------------------------
// <copyright file = "ShowProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Services;

    /// <summary>
    /// Provider which indicates if the GameLib is in show mode.
    /// </summary>
    public class ShowProvider
    {
        /// <summary>
        /// Reference to the current GameLib.
        /// </summary>
        private readonly IGameLibShow gameLib;

        /// <summary>
        /// Gets the Boolean which indicates if the GameLib is in ShowMode.
        /// </summary>
        [GameService]
        public bool ShowMode
        {
            get
            {
                return gameLib != null && gameLib.ShowMode;
            }
        }

        /// <summary>
        /// Gets the enumeration representing if the current show environment is Invalid, Development or Show.
        /// </summary>
        [GameService]
        public ShowEnvironment GameShowEnvironment
        {
            get
            {
                return gameLib != null ? gameLib.GetShowEnvironment() : ShowEnvironment.Invalid;
            }
        }


        /// <summary>
        /// Constructor for the ShowProvider.
        /// </summary>
        /// <param name="gameLib">GameLib used to determine if show mode is enabled.</param>
        public ShowProvider(IGameLib gameLib)
        {
            this.gameLib = gameLib as IGameLibShow;
        }
    }
}