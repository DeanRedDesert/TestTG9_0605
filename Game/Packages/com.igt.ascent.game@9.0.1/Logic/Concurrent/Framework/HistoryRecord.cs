// -----------------------------------------------------------------------
// <copyright file = "HistoryRecord.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.IO;
    using Game.Core.CompactSerialization;
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// A data structure to store a history record in a compact format.
    /// </summary>
    internal struct HistoryRecord : ICompactSerializable
    {
        /// <summary>
        /// Gets the step number of this history record.
        /// </summary>
        internal int StepNumber { get; private set; }

        /// <summary>
        /// Gets the name of the state that this record was saved for.
        /// </summary>
        internal string StateName { get; private set; }

        /// <summary>
        /// Gets the data stored in this record.
        /// </summary>
        internal DataItems HistoryData { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="HistoryRecord"/> struct.
        /// </summary>
        /// <param name="stepNumber">The step number for this history record.</param>
        /// <param name="stateName">The name of the state this data is being saved for.</param>
        /// <param name="data">The data to save.</param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid arguments.
        /// </devdoc>
        internal HistoryRecord(int stepNumber, string stateName, DataItems data)
        {
            StepNumber = stepNumber;
            StateName = stateName;
            HistoryData = data;
        }

        #region ICompactSerializable implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, StepNumber);
            CompactSerializer.Write(stream, StateName);
            CompactSerializer.Write(stream, HistoryData);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            StepNumber = CompactSerializer.ReadInt(stream);
            StateName = CompactSerializer.ReadString(stream);
            HistoryData = CompactSerializer.ReadSerializable<DataItems>(stream);
        }

        #endregion

        #region Overrides of ValueType

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{StateName}/Step{StepNumber}/DataItems";
        }

        #endregion
    }
}
