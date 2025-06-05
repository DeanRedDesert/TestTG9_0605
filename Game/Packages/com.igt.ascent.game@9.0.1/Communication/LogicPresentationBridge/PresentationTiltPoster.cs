//-----------------------------------------------------------------------
// <copyright file = "PresentationTiltPoster.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using CommunicationLib;
    using Tilts;

    /// <summary>
    /// Static class for the convenience of the game presentation to post and clear a tilt.
    /// </summary>
    public static class PresentationTiltPoster
    {
        private static IGameLogic gameLogic;

        /// <summary>
        /// Initialize with a reference to the game logic interface.
        /// </summary>
        /// <param name="iGameLogic">The interface to post game logic messages.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLogic"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this method is called more than once.
        /// </exception>
        public static void Initialize(IGameLogic iGameLogic)
        {
            if(iGameLogic == null)
            {
                throw new ArgumentNullException("iGameLogic");
            }

            if(gameLogic != null)
            {
                throw new InvalidOperationException("PresentationTiltPoster has already been initialized.");
            }

            gameLogic = iGameLogic;
        }

        /// <summary>
        /// Post a presentation tilt whose title and message do not require
        /// argument objects for formatting.
        /// </summary>
        /// <param name="tiltKey">The key used to track the presentation tilt.</param>
        /// <param name="presentationTilt">The presentation tilt to post.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="presentationTilt"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty, or
        /// <paramref name="presentationTilt"/> is a persistent tilt.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this method is called before <see cref="Initialize(IGameLogic)"/>.
        /// </exception>
        public static void PostTilt(string tiltKey, ITilt presentationTilt)
        {
            PostTilt(tiltKey, presentationTilt, null, null);
        }

        /// <summary>
        /// Post a presentation tilt whose title and message will be formatted
        /// with the specified argument objects.
        /// </summary>
        /// <param name="tiltKey">The key used to track the presentation tilt.</param>
        /// <param name="presentationTilt">The presentation tilt to post.</param>
        /// <param name="titleFormatArgs">The array of objects to format for the tilt title.</param>
        /// <param name="messageFormatArgs">The array of objects to format for the tilt message.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="presentationTilt"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this method is called before <see cref="Initialize(IGameLogic)"/>.
        /// </exception>
        public static void PostTilt(string tiltKey, ITilt presentationTilt,
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

            if(gameLogic == null)
            {
                throw new InvalidOperationException("PresentationTiltPoster is not initialized yet.");
            }

            gameLogic.PostPresentationTilt(tiltKey, presentationTilt, titleFormatArgs, messageFormatArgs);
        }

        /// <summary>
        /// Clear a presentation tilt.
        /// </summary>
        /// <param name="tiltKey">The key of the presentation tilt to be cleared.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this method is called before <see cref="Initialize(IGameLogic)"/>.
        /// </exception>
        public static void ClearTilt(string tiltKey)
        {
            if(string.IsNullOrEmpty(tiltKey))
            {
                throw new ArgumentException("Tilt key is undefined.", "tiltKey");
            }

            if(gameLogic == null)
            {
                throw new InvalidOperationException("PresentationTiltPoster is not initialized yet.");
            }

            gameLogic.ClearPresentationTilt(tiltKey);
        }

        /// <summary>
        /// Clear the game logic reference.  Used for tests.
        /// </summary>
        internal static void Uninitialize()
        {
            gameLogic = null;
        }

        /// <summary>
        /// Gets the initialization state of this class.
        /// </summary>
        /// <returns>A flag indicating the initialization state of this class.</returns>
        public static bool Initialized => gameLogic != null;
    }
}
