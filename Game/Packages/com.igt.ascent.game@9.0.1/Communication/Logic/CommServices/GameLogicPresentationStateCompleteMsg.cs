//-----------------------------------------------------------------------
// <copyright file = "GameLogicPresentationStateCompleteMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    ///    Message object that is compromised of the parameters of the
    ///    Game Logic Interface Function PresentationStateComplete.
    /// </summary>
    [Serializable]
    public class GameLogicPresentationStateCompleteMsg : GameLogicGenericMsg, ICompactSerializable
    {
        /// <summary>
        ///    Constructor for creating GameLogicPresentationStateCompleteMsg.
        /// </summary>
        /// <param name="stateName">
        ///    Name of the state which is completing.
        /// </param>
        /// <param name="actionRequest">
        ///     An action which the Presentation is attempting to initiate.
        ///     For instance an action may be “StartGame” and the serviceData
        ///     and genericData would support that action.
        /// </param>
        /// <param name="data">: 
        ///     List of generic data to be used by the game logic. In most
        ///     cases the logic should safe store this data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown is the state name or action request is null.
        /// </exception>
        public GameLogicPresentationStateCompleteMsg(string stateName,
                                                     string actionRequest,
                                                     Dictionary<string, object> data)
        {
            if (stateName == null)
            {
                throw new ArgumentNullException("stateName", "Parameter may not be null.");
            }
            if (actionRequest == null)
            {
                throw new ArgumentNullException("actionRequest", "Parameter may not be null.");
            }

            StateName = stateName;
            ActionRequest = actionRequest;
            GenericData = data;
        }

        /// <summary>Override base implementation to provide better information.</summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tStateName = " + StateName);
            builder.AppendLine("\tActionRequest = " + ActionRequest);

            if (GenericData != null)
            {
                builder.AppendLine(GenericData.Aggregate("\tGenericData:",
                                                          (current, keyValuePair) =>
                                                          current +
                                                          (Environment.NewLine + "\t" + keyValuePair.Key + ": " + keyValuePair.Value)));
            }
            else
            {
                builder.AppendLine("\tGenericData = null");
            }

            return builder.ToString();
        }

        /// <summary>
        ///    Name of the state which has completed.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        ///    An action which the Presentation is attempting to initiate.
        /// </summary>
        public string ActionRequest { get; set; }

        /// <summary>
        ///     List of generic data to be used by the game logic. In most
        ///     cases the logic should safe store this data.
        /// </summary>
        public Dictionary<string, object> GenericData { get; set; }

        #region GameLogicGenericMsg Overrides

        /// <inheritdoc />
        public override GameLogicMessageType MessageType
        {
            get
            {
                return GameLogicMessageType.PresentationStateComplete;
            }
        }

        #endregion

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public GameLogicPresentationStateCompleteMsg()
        {
        }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            CompactSerializer.Write(stream, StateName);
            CompactSerializer.Write(stream, ActionRequest);

            if(GenericData == null)
            {
                // The dictionary is null.
                CompactSerializer.Write(stream, true);
            }
            else
            {
                // The dictionary is not null.
                CompactSerializer.Write(stream, false);

                // Write the count of the dictionary.
                CompactSerializer.Write(stream, GenericData.Count);

                // Write the entries of the dictionary.
                foreach(var entry in GenericData)
                {
                    CompactSerializer.Write(stream, entry.Key);

                    if(entry.Value == null)
                    {
                        // The object is null.
                        CompactSerializer.Write(stream, true);
                    }
                    else
                    {
                        // The object is not null.
                        CompactSerializer.Write(stream, false);

                        // Use Binary Formatter to serialize object types.
                        binaryFormatter.Serialize(stream, entry.Value);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            StateName = CompactSerializer.ReadString(stream);
            ActionRequest = CompactSerializer.ReadString(stream);

            if(CompactSerializer.ReadBool(stream))
            {
                // The dictionary is null.
                GenericData = null;
            }
            else
            {
                // Read the count of the dictionary.
                var count = CompactSerializer.ReadInt(stream);

                // Create the dictionary.
                GenericData = new Dictionary<string, object>(count);

                // Read the entries of the dictionary.
                for(var i = 0; i < count; i++)
                {
                    var entryKey = CompactSerializer.ReadString(stream);

                    object entryValue = null;

                    // The object is not null.
                    if(!CompactSerializer.ReadBool(stream))
                    {
                        // Use Binary Formatter to deserialize object types.
                        entryValue = binaryFormatter.Deserialize(stream);
                    }

                    GenericData.Add(entryKey, entryValue);
                }
            }
        }

        #endregion
    }
}
