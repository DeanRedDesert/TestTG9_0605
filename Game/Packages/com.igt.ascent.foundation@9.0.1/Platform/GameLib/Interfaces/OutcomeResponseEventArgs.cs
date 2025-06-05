//-----------------------------------------------------------------------
// <copyright file = "OutcomeResponseEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;
    using OutcomeList;
    using OutcomeList.Interfaces;

    /// <summary>
    /// Event which represents a finalize outcome response.
    /// </summary>
    [Serializable]
    public class OutcomeResponseEventArgs : EventArgs, ICompactSerializable
    {
        /// <summary>
        /// Property containing the finalized outcome list.
        /// </summary>
        public OutcomeList AdjustedOutcome
        {
            get;
            private set;
        }

        /// <summary>
        /// Property which indicates if this was the final outcome.
        /// </summary>
        public bool IsFinalOutcome
        {
            get;
            private set;
        }

        /// <summary>
        /// Construct an OutcomeResponseEventArgs with the given
        /// outcome list.
        /// </summary>
        /// <param name="outcomeList">The finalized outcome.</param>
        /// <param name="isFinalOutcome">The flag indicating if this was the final outcome.</param>
        public OutcomeResponseEventArgs(IOutcomeList outcomeList, bool isFinalOutcome)
        {
            AdjustedOutcome = outcomeList == null ? null : new OutcomeList(outcomeList);
            IsFinalOutcome = isFinalOutcome;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeResponseEvent -");
            builder.AppendLine("\t Is Final Outcome = " + IsFinalOutcome);
            builder.AppendFormat("\t {0}", AdjustedOutcome);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public OutcomeResponseEventArgs()
        {
        }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, IsFinalOutcome);
            CompactSerializer.Write(stream, AdjustedOutcome);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            IsFinalOutcome = CompactSerializer.ReadBool(stream);
            AdjustedOutcome = CompactSerializer.ReadSerializable<OutcomeList>(stream);
        }

        #endregion
    }
}