// -----------------------------------------------------------------------
// <copyright file = "InactivateShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event indicating the current shell context is being inactivated.
    /// </summary>
    public sealed class InactivateShellContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the shell context that has been inactivated.
        /// </summary>
        public IShellContext LastActiveContext { get;}

        /// <summary>
        /// Initializes a new instance of <see cref="InactivateShellContextEventArgs"/>.
        /// </summary>
        /// <param name="lastActiveContext">
        /// The shell context that has been inactivated.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        public InactivateShellContextEventArgs(IShellContext lastActiveContext = null)
        {
            LastActiveContext = lastActiveContext;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("InactivateShellContextEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t LastActiveContext: " + LastActiveContext);

            return builder.ToString();
        }

        #endregion
    }
}
