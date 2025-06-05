//-----------------------------------------------------------------------
// <copyright file = "StateHandlerRegistry.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// Contains all of the data needed by the state manager to register a state handler.
    /// </summary>
    [Serializable]
    public abstract class StateHandlerRegistry : ISerializable, ICompactSerializable
    {
        private const string BaseSceneKey = "BaseScene";
        private const string StateNameKey = "StateName";
        private const string ScenesWithRegisteredDataKey = "ScenesWithRegisteredData";
        private const string IsHistoryKey = "IsHistory";

        private List<string> scenesWithRegisteredData;

        /// <summary>
        /// Gets the name of the scene that contains the actual state handler.
        /// </summary>
        public string BaseScene { get; private set; }

        /// <summary>
        /// Gets a collection containing the names of all the scenes that have registered data for the state
        /// handler's state.
        /// </summary>
        public IEnumerable<string> ScenesWithRegisteredData => scenesWithRegisteredData.AsReadOnly();

        /// <summary>
        /// Gets the name of the state that the handler will be registered for.
        /// </summary>
        public string StateName { get; private set; }

        /// <summary>
        /// Gets a <see cref="bool"/> indicating if the handler supports the <see cref="IHistoryPresentationState"/>
        /// interface.
        /// </summary>
        public bool IsHistory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateHandlerRegistry"/> class for a presentation state that
        /// does not support <see cref="IHistoryPresentationState"/>.
        /// </summary>
        /// <param name="stateName">The name of the state to handle.</param>
        /// <param name="baseScene">The name of the scene that contains the state handler.</param>
        /// <param name="scenesWithRegisteredData">
        /// The names of all the scenes that have registered data for the state.
        /// </param>
        protected StateHandlerRegistry(string stateName, string baseScene, IEnumerable<string> scenesWithRegisteredData)
            :this(stateName, baseScene, scenesWithRegisteredData, false)
        {
        }

        /// <summary>
        /// Constructor for ICompactSerializable.
        /// </summary>
        protected StateHandlerRegistry()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="StateHandlerRegistry"/> class for a presentation state.  This
        /// overload allows you to specify whether or not the handler supports the 
        /// <see cref="IHistoryPresentationState"/> interface.
        /// </summary>
        /// <param name="stateName">The name of the state to handle.</param>
        /// <param name="baseScene">The name of the scene that contains the state handler.</param>
        /// <param name="scenesWithRegisteredData">
        /// The names of all the scenes that have registered data for the state.
        /// </param>
        /// <param name="isHistory">
        /// <b>true</b> to indicate that the handler supports <see cref="IHistoryPresentationState"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="stateName"/> or <paramref name="baseScene"/> are <b>null</b> or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="scenesWithRegisteredData"/> is <b>null</b>.
        /// </exception>
        protected StateHandlerRegistry(string stateName, string baseScene, IEnumerable<string> scenesWithRegisteredData, bool isHistory)
        {
            if(string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentException("stateName");
            }
            if(string.IsNullOrEmpty(baseScene))
            {
                throw new ArgumentException("baseScene");
            }
            if(scenesWithRegisteredData == null)
            {
                throw new ArgumentNullException(nameof(scenesWithRegisteredData));
            }
            StateName = stateName;
            BaseScene = baseScene;
            this.scenesWithRegisteredData = new List<string>(scenesWithRegisteredData);
            IsHistory = isHistory;
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object containing the serialized data.</param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> containing context information about the serialization operation.
        /// </param>
        protected StateHandlerRegistry(SerializationInfo info, StreamingContext context)
        {
            StateName = info.GetString(StateNameKey);
            BaseScene = info.GetString(BaseSceneKey);
            scenesWithRegisteredData = (List<string>)info.GetValue(ScenesWithRegisteredDataKey, typeof(List<string>));
            IsHistory = info.GetBoolean(IsHistoryKey);
        }

        /// <summary>
        /// Updates the collection of scenes with required data to match <paramref name="newScenes"/>.
        /// </summary>
        /// <param name="newScenes">The new collection of scenes with required data.</param>
        public void UpdateScenesWithRegisteredData(IEnumerable<string> newScenes)
        {
            scenesWithRegisteredData.Clear();
            scenesWithRegisteredData.AddRange(newScenes);
        }

        /// <summary>
        /// When overriden, gets a reference to the state handler that this registration pertains to.
        /// </summary>
        /// <returns>A reference to an <see cref="IStateHandler"/> object.</returns>
        public abstract IStateHandler GetStateHandler();

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine("StateName: " + StateName);
            sb.AppendLine("BaseScene: " + BaseScene);
            sb.AppendLine("ScenesWithData: " + string.Join(", ", ScenesWithRegisteredData.ToArray()));
            return sb.ToString();
        }

        #region Implementation of ISerializable

        /// <summary>
        /// Populates the given <see cref="SerializationInfo"/> object with the data required to serialize
        /// this object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate.</param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> containing context information about the serialization operation.
        /// </param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(StateNameKey, StateName);
            info.AddValue(BaseSceneKey, BaseScene);
            info.AddValue(ScenesWithRegisteredDataKey, scenesWithRegisteredData);
            info.AddValue(IsHistoryKey, IsHistory);
        }

        #endregion

        #region ICompactSerializable Implementation
        
        /// <inheritdoc/>
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Serialize(stream, StateName);
            CompactSerializer.Serialize(stream, BaseScene);
            CompactSerializer.Serialize(stream, scenesWithRegisteredData);
            CompactSerializer.Serialize(stream, IsHistory);
        }

        /// <inheritdoc/>
        public void Deserialize(System.IO.Stream stream)
        {
            StateName = CompactSerializer.Deserialize<string>(stream);
            BaseScene = CompactSerializer.Deserialize<string>(stream);
            scenesWithRegisteredData = CompactSerializer.Deserialize<List<string>>(stream);
            IsHistory = CompactSerializer.Deserialize<bool>(stream);
        }

        #endregion
    }
}
