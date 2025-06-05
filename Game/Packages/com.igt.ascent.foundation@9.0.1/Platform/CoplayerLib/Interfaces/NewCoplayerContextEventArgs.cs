// -----------------------------------------------------------------------
// <copyright file = "NewCoplayerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating a new coplayer context is being switched to.
    /// </summary>
    [Serializable]
    public sealed class NewCoplayerContextEventArgs : TransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="GameMode"/> of the new context.
        /// </summary>
        public GameMode GameMode { get; private set; }

        /// <summary>
        /// Gets the denomination for the coplayer context.
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        /// Gets the TagData defined in the payvar registry.
        /// </summary>
        public string PayvarTag { get; private set; }

        /// <summary>
        /// Gets the TagDataFile defined in the payvar registry.
        /// </summary>
        public string PayvarTagDataFile { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="NewCoplayerContextEventArgs"/>.
        /// </summary>
        /// <param name="gameMode">
        /// The <see cref="GameMode"/> of the new context.
        /// </param>
        /// <param name="denomination">
        /// The denomination for the coplayer context.
        /// </param>
        /// <param name="payvarTag">
        /// The TagData defined in the payvar registry.
        /// </param>
        /// <param name="payvarTagDataFile">
        /// The TagDataFile defined in the payvar registry.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denomination"/> is less than 1.
        /// </exception>
        public NewCoplayerContextEventArgs(GameMode gameMode, long denomination, string payvarTag, string payvarTagDataFile)
        {
            if(denomination < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination cannot be less than 1.");

            }

            GameMode = gameMode;
            Denomination = denomination;
            PayvarTag = payvarTag;
            PayvarTagDataFile = payvarTagDataFile;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("NewCoplayerContextEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t GameMode: " + GameMode);
            builder.AppendLine("\t Denomination: " + Denomination);
            builder.AppendLine("\t PayvarTag: " + PayvarTag);
            builder.AppendLine("\t PayvarTagDataFile: " + PayvarTagDataFile);

            return builder.ToString();
        }

        #endregion
    }
}