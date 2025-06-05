// -----------------------------------------------------------------------
// <copyright file = "InactivateCoplayerContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating the current coplayer context is being inactivated.
    /// </summary>
    public sealed class InactivateCoplayerContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the coplayer context that has been inactivated.
        /// </summary>
        public ICoplayerContext LastActiveContext { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="InactivateCoplayerContextEventArgs"/>.
        /// </summary>
        /// <param name="lastActiveContext">
        /// The shell context that has been inactivated.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        public InactivateCoplayerContextEventArgs(ICoplayerContext lastActiveContext = null)
        {
            LastActiveContext = lastActiveContext;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("InactivateCoplayerContextEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t LastActiveContext: " + LastActiveContext);

            return builder.ToString();
        }

        #endregion
    }
}
