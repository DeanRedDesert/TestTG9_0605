// -----------------------------------------------------------------------
// <copyright file = "ProgressiveLevel.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains the information about UGP progressive level.
    /// </summary>
    [Serializable]
    public class ProgressiveLevel : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the progressive level ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the progressive level name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the increment percentage of the progressive level.
        /// </summary>
        public double IncrementPercentage { get; set; }

        /// <summary>
        /// Gets or sets the hidden increment percentage of the progressive level.
        /// </summary>
        public double HiddenIncrementPercentage { get; set; }

        /// <summary>
        /// The startup amount for the progressive level.
        /// </summary>
        public long Startup { get; set; }

        /// <summary>
        /// Gets or sets the ceiling amount for the progressive level.
        /// </summary>
        public long Ceiling { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the progressive level is standalone.
        /// </summary>
        public bool IsStandalone { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the progressive level is triggered.
        /// </summary>
        public bool IsTriggered { get; set; }

        /// <summary>
        /// Gets or sets the return percentage of the progressive level.
        /// </summary>
        public double Rtp { get; set; }

        /// <summary>
        /// Gets or sets the trigger probability of the progressive level.
        /// </summary>
        public double TriggerProbability { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="ProgressiveLevel"/>.
        /// </summary>
        /// <param name="progressiveLevelInfo">
        /// The instance of internal class <see cref="ProgressiveLevelInfo"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="progressiveLevelInfo"/> is null.
        /// </exception>
        public ProgressiveLevel(ProgressiveLevelInfo progressiveLevelInfo)
        {
            if(progressiveLevelInfo == null)
            {
                throw new ArgumentNullException(nameof(progressiveLevelInfo));
            }

            Id = progressiveLevelInfo.Id;
            Name = progressiveLevelInfo.Name;
            IncrementPercentage = progressiveLevelInfo.IncrementPercentage;
            HiddenIncrementPercentage = progressiveLevelInfo.HiddenIncrementPercentage;
            IsTriggered = progressiveLevelInfo.ProgressiveType == ProgressiveType.Triggered;
            IsStandalone = progressiveLevelInfo.IsStandalone;
            Startup = progressiveLevelInfo.Startup;
            Ceiling = progressiveLevelInfo.Ceiling;
            Rtp = progressiveLevelInfo.Rtp;
            TriggerProbability = progressiveLevelInfo.TriggerProbability;
        }

        #endregion

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Progressive Level -");
            builder.AppendLine("\t ID = " + Id);
            builder.AppendLine("\t Name = " + Name);
            builder.AppendLine("\t IncrementPercentage = " + IncrementPercentage);
            builder.AppendLine("\t HiddenIncrementPercentage = " + HiddenIncrementPercentage);
            builder.AppendLine("\t Startup = " + Startup);
            builder.AppendLine("\t Ceiling = " + Ceiling);
            builder.AppendLine("\t IsStandalone = " + IsStandalone);
            builder.AppendLine("\t IsTriggered = " + IsTriggered);
            builder.AppendLine("\t Rtp = " + Rtp);
            builder.AppendLine("\t TriggerProbability = " + TriggerProbability);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// Constructs an instance of the <see cref="ProgressiveLevel"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing ICompactSerializable must have a public parameter-less constructor.
        /// </remarks>
        public ProgressiveLevel()
        {
        }

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Id);
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, IncrementPercentage);
            CompactSerializer.Write(stream, HiddenIncrementPercentage);
            CompactSerializer.Write(stream, Startup);
            CompactSerializer.Write(stream, Ceiling);
            CompactSerializer.Write(stream, IsStandalone);
            CompactSerializer.Write(stream, IsTriggered);
            CompactSerializer.Write(stream, Rtp);
            CompactSerializer.Write(stream, TriggerProbability);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Id = CompactSerializer.ReadString(stream);
            Name = CompactSerializer.ReadString(stream);
            IncrementPercentage = CompactSerializer.ReadDouble(stream);
            HiddenIncrementPercentage = CompactSerializer.ReadDouble(stream);
            Startup = CompactSerializer.ReadLong(stream);
            Ceiling = CompactSerializer.ReadLong(stream);
            IsStandalone = CompactSerializer.ReadBool(stream);
            IsTriggered = CompactSerializer.ReadBool(stream);
            Rtp = CompactSerializer.ReadDouble(stream);
            TriggerProbability = CompactSerializer.ReadDouble(stream);
        }

        #endregion
    }
}
