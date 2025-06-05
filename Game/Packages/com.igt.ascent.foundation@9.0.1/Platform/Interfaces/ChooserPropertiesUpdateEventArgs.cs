// -----------------------------------------------------------------------
// <copyright file = "ChooserPropertiesUpdateEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that one or more chooser service's properties have changed.
    /// </summary>
    [Serializable]
    public sealed class ChooserPropertiesUpdateEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets a flag indicating whether or not the player can request the chooser.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? Offerable { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ChooserPropertiesUpdateEventArgs"/>.
        /// </summary>
        /// <param name="offerable">
        /// The nullable flag indicating whether or not the player can request the chooser.
        /// </param>
        /// <remarks>
        /// An argument being null means that the value has not been changed since last time it was queried/updated.
        /// </remarks>
        public ChooserPropertiesUpdateEventArgs(bool? offerable)
        {
            Offerable = offerable;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ChooserPropertiesUpdateEventArgs -");
            builder.Append(base.ToString());
            if(Offerable != null)
            {
                builder.AppendLine("\t Offerable: " + Offerable);
            }
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion
    }
}
