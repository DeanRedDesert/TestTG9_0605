//-----------------------------------------------------------------------
// <copyright file = "GameLogicPlayerSessionParametersResetMsg.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using CompactSerialization;
    using CommunicationLib;

    /// <summary>
    /// Message object that is compromised of the parameters of the
    /// Game Logic Interface Function ResetPlayerSessionParameters.
    /// </summary>
    [Serializable]
    public class GameLogicPlayerSessionParametersResetMsg : GameLogicGenericMsg, ICompactSerializable
    {
        /// <summary>
        /// Constructor for creating <see cref="GameLogicPlayerSessionParametersResetMsg"/>.
        /// </summary>
        /// <param name="actionType">
        /// An action which the Presentation is attempting to initiate.
        /// </param>
        /// <param name="actionData">
        /// The action data with the action to initiate.
        /// </param>
        public GameLogicPlayerSessionParametersResetMsg(PlayerSessionParametersResetActionType actionType,
                                                        object actionData)
        {
            ActionType = actionType;
            ActionData = actionData;
        }

        /// <summary>
        /// An action which the Presentation is attempting to initiate.
        /// </summary>
        public PlayerSessionParametersResetActionType ActionType { get; private set; }

        /// <summary>
        /// The payload data corresponding to the action to initiate.
        /// </summary>
        public object ActionData { get; private set; }

        #region GameLogicGenericMsg Overrides

        /// <inheritdoc />
        public override GameLogicMessageType MessageType
        {
            get
            {
                return GameLogicMessageType.ResetPlayerSessionParameters;
            }
        }

        #endregion

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public GameLogicPlayerSessionParametersResetMsg()
        {
        }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            CompactSerializer.Write(stream, ActionType);
            binaryFormatter.Serialize(stream, ActionData);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            ActionType = CompactSerializer.ReadEnum<PlayerSessionParametersResetActionType>(stream);
            ActionData = binaryFormatter.Deserialize(stream);
        }

        #endregion
    }
}
