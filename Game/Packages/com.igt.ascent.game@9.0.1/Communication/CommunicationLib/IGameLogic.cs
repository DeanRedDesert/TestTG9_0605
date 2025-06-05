//-----------------------------------------------------------------------
// <copyright file = "IGameLogic.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;
    using System.Collections.Generic;
    using Tilts;

    /// <summary>
    /// This interface contains the APIs for the game presentation
    /// to communicate with the game logic.
    /// </summary>
    public interface IGameLogic
    {
        /// <summary>
        /// Inform the Game Logic that the Presentation state has
        /// completed and the logic can transition to another state.
        /// </summary>
        /// <param name="stateName">
        /// Name of the state which has completed.
        /// </param>
        /// <param name="actionRequest">
        /// An action which the Presentation is attempting to initiate.
        /// For instance an action may be “StartGame” and the serviceData
        /// and genericData would support that action.
        /// </param>
        /// <param name="data">:
        /// List of generic data to be used by the game logic. In most
        /// cases the logic should safe store this data.
        /// </param>
        void PresentationStateComplete(string stateName,
                                       string actionRequest,
                                       Dictionary<string, object> data = null);

        /// <summary>
        /// Inform the Game Logic that the Presentation would like
        /// to post a game side tilt.
        /// </summary>
        /// <param name="tiltKey">
        /// The key used to track the presentation tilt.
        /// </param>
        /// <param name="presentationTilt">
        /// The presentation tilt to post.
        /// </param>
        /// <param name="titleFormatArgs">
        /// The array of objects to format for the tilt title.
        /// </param>
        /// <param name="messageFormatArgs">
        /// The array of objects to format for the tilt message.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltKey"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="presentationTilt"/> is null.
        /// </exception>
        void PostPresentationTilt(string tiltKey, ITilt presentationTilt,
                                  object[] titleFormatArgs, object[] messageFormatArgs);

        /// <summary>
        /// Inform the Game Logic that the Presentation would like
        /// to clear a game side tilt.
        /// </summary>
        /// <param name="tiltKey">
        /// The key of the presentation tilt to be cleared.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tiltKey"/> is null.
        /// </exception>
        void ClearPresentationTilt(string tiltKey);
    }
}
