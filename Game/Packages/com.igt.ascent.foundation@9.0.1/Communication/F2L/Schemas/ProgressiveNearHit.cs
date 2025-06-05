//-----------------------------------------------------------------------
// <copyright file = "ProgressiveNearHit.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

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
                throw new ArgumentNullException("nearHit");
            }

            game_levelFieldSpecified = nearHit.GameLevel.HasValue;
            game_levelField = nearHit.GameLevel.GetValueOrDefault();
        }

        #endregion Constructors

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ProgressiveNearHit -");
            builder.AppendLine("\t  Game Level Specified = " + game_levelSpecified);
            builder.AppendLine("\t  Game Level = " + game_level);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, game_level);
            CompactSerializer.Write(stream, game_levelSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            game_level = CompactSerializer.ReadUint(stream);
            game_levelSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion


        #region IProgressiveNearHit Members

        /// <inheritdoc />
        public uint? GameLevel
        {
            get { return game_levelField; }
        }

        #endregion IProgressiveNearHit Members
    }
}
