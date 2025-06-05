//-----------------------------------------------------------------------
// <copyright file = "GameLogicClearPresentationTiltMsg.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;

    /// <summary>
    /// This message is used to clear a game side tilt by the presentation.
    /// It is used by the Game Logic Interface Function ClearPresentationTilt.
    /// </summary>
    public class GameLogicClearPresentationTiltMsg : GameLogicGenericMsg
    {
        #region GameLogicGenericMsg Overrides

        /// <inheritdoc />
        public override GameLogicMessageType MessageType
        {
            get
            {
                return GameLogicMessageType.ClearPresentationTilt;
            }
        }

        #endregion

        /// <summary>
        /// The key of the presentation tilt to be cleared.
        /// </summary>
        public string TiltKey { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="GameLogicClearPresentationTiltMsg"/>
        /// with the key of the presentation tilt to be cleared.
        /// </summary>
        /// <param name="tiltKey">The key of the presentation tilt to be cleared.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty.
        /// </exception>
        public GameLogicClearPresentationTiltMsg(string tiltKey)
        {
            if(string.IsNullOrEmpty(tiltKey))
            {
                throw new ArgumentException("Tilt key is undefined.", "tiltKey");
            }

            TiltKey = tiltKey;
        }
    }
}
