// -----------------------------------------------------------------------
// <copyright file = "ChooserProperties.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Contains the chooser services related properties.
    /// </summary>
    [Serializable]
    public sealed class ChooserProperties
    {
        #region Properties

        /// <summary>
        /// Gets the flag that indicates if the chooser may be offered to the player.
        /// This does not take into consideration whether a game is in progress.
        /// This only indicates whether there are more games available for selection.
        /// </summary>
        public bool Offerable { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ChooserProperties"/>.
        /// </summary>
        /// <param name="offerable">
        /// The flag indicates if the chooser services is available.
        /// </param>
        public ChooserProperties(bool offerable)
        {
            Offerable = offerable;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ChooserProperties -");
            builder.AppendLine("\t Offerable: " + Offerable);
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion
    }
}
