//-----------------------------------------------------------------------
// <copyright file = "ProgressiveNearHit.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using IGT.Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type ProgressiveNearHit.
    /// </summary>
    public partial class ProgressiveNearHit : IProgressiveNearHit
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public ProgressiveNearHit()
        {
        }

        /// <summary>
        /// Constructor.  Creates a ProgressiveNearHit from an IProgressiveNearHit.
        /// </summary>
        /// <param name="nearHit">
        /// An implementation of <see cref="IProgressiveNearHit"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nearHit"/> is null.
        /// </exception>
        public ProgressiveNearHit(IProgressiveNearHit nearHit) : base(nearHit)
        {
            if(nearHit == null)
            {
                throw new ArgumentNullException(nameof(nearHit));
            }

            if(nearHit.GameLevel != null)
            {
                GameLevel = (uint)nearHit.GameLevel;
                GameLevelSpecified = true;
            }
            else
            {
                GameLevel = 0;
                GameLevelSpecified = false;
            }
        }

        #endregion Constructors

        #region IProgressiveNearHit Members

        /// <inheritdoc />
        uint? IProgressiveNearHit.GameLevel => GameLevelSpecified ? (uint?)GameLevel : null;

        #endregion IProgressiveNearHit Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, GameLevel);
            CompactSerializer.Write(stream, GameLevelSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            GameLevel = CompactSerializer.ReadUint(stream);
            GameLevelSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region ToString Override

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ProgressiveNearHit -");
            builder.AppendLine("\t  Game Level Specified = " + GameLevelSpecified);
            builder.AppendLine("\t  Game Level = " + GameLevel);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
