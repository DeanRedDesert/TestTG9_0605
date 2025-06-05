//-----------------------------------------------------------------------
// <copyright file = "GameLogicPostPresentationTiltMsg.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;
    using CompactSerialization;
    using Tilts;

    /// <summary>
    /// This message is used to post a game side tilt by the presentation.
    /// It is used by the Game Logic Interface Function PostPresentationTilt.
    /// </summary>
    public class GameLogicPostPresentationTiltMsg : GameLogicGenericMsg
    {
        #region GameLogicGenericMsg Overrides

        /// <inheritdoc />
        public override GameLogicMessageType MessageType
        {
            get
            {
                return GameLogicMessageType.PostPresentationTilt;
            }
        }

        #endregion

        /// <summary>
        /// The key used to track the presentation tilt.
        /// </summary>
        public string TiltKey { get; private set; }

        /// <summary>
        /// The presentation tilt to post.
        /// The tilt must also implement <see cref="ICompactSerializable"/>.
        /// </summary>
        public ITilt PresentationTilt { get; private set; }

        /// <summary>
        /// The array of objects to format for the tilt title.
        /// </summary>
        public object[] TitleFormatArgs { get; private set; }

        /// <summary>
        /// The array of objects to format for the tilt message.
        /// </summary>
        public object[] MessageFormatArgs { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="GameLogicPostPresentationTiltMsg"/>
        /// with the presentation tilt to post.
        /// </summary>
        /// <param name="tiltKey">The key used to track the presentation tilt.</param>
        /// <param name="presentationTilt">The presentation tilt to post.</param>
        /// <param name="titleFormatArgs">The array of objects to format for the tilt title.</param>
        /// <param name="messageFormatArgs">The array of objects to format for the tilt message.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="presentationTilt"/> is null.
        /// </exception>
        public GameLogicPostPresentationTiltMsg(string tiltKey, ITilt presentationTilt,
                                                object[] titleFormatArgs, object[] messageFormatArgs)
        {
            if(string.IsNullOrEmpty(tiltKey))
            {
                throw new ArgumentException("Tilt key is undefined.", "tiltKey");
            }

            if(presentationTilt == null)
            {
                throw new ArgumentNullException("presentationTilt");
            }

            TiltKey = tiltKey;
            PresentationTilt = presentationTilt;
            TitleFormatArgs = titleFormatArgs;
            MessageFormatArgs = messageFormatArgs;
        }
    }
}
