//-----------------------------------------------------------------------
// <copyright file = "GameSubModeParser.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Xml.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class parses the system configuration file to get the game sub-mode information
    /// for Standalone mode.
    /// </summary>
    internal class GameSubModeParser
    {
        /// <summary>
        /// The game sub-mode of the game context.
        /// </summary>
        public GameSubMode GameSubMode { get; }

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        /// <param name="gameSubModeElement">
        /// An xml element containing an optional attribute on the game sub-mode. This is an optional element.
        /// The game sub-mode will default to Standard if it is not defined.
        /// </param>
        public GameSubModeParser(XElement gameSubModeElement)
        {
            GameSubMode = GameSubMode.Standard;

            var gameSubModeAttribute = gameSubModeElement?.Attribute("Type");
            if(gameSubModeAttribute != null)
            {
                GameSubMode = (GameSubMode)Enum.Parse(typeof(GameSubMode), gameSubModeAttribute.Value);
            }
        }
    }
}
