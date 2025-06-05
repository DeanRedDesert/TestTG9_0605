//-----------------------------------------------------------------------
// <copyright file = "CurrentResetParametersChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event indicating that the current reset parameters have changed.
    /// </summary>
    [Serializable]
    public class CurrentResetParametersChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the list of pending player session parameters to reset.
        /// </summary>
        public IList<PlayerSessionParameterType> PendingParameters { get; private set; }

        /// <summary>
        /// Gets the list of player session parameters that had been reset.
        /// </summary>
        public IList<PlayerSessionParameterType> ResetParameters { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="CurrentResetParametersChangedEventArgs"/>.
        /// </summary>
        /// <param name="pendingParameters">
        /// A collection of pending player session parameters to reset.
        /// </param>
        /// <param name="resetParameters">
        /// A collection of player session parameters that had been reset.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="pendingParameters"/> or <paramref name="resetParameters"/> is null.
        /// </exception>
        public CurrentResetParametersChangedEventArgs(IEnumerable<PlayerSessionParameterType> pendingParameters,
                                                      IEnumerable<PlayerSessionParameterType> resetParameters)
        {
            if(pendingParameters == null)
            {
                throw new ArgumentNullException(nameof(pendingParameters));
            }
            if(resetParameters == null)
            {
                throw new ArgumentNullException(nameof(resetParameters));
            }

            PendingParameters = pendingParameters.ToList();
            ResetParameters = resetParameters.ToList();
        }

        /// <summary>
        /// Overrides base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(base.ToString());
            builder.Append("Pending player session parameters:");
            foreach(var parameter in PendingParameters)
            {
                builder.Append(" " + parameter);
            }
            builder.AppendLine("");
            builder.AppendLine("Reset player session parameters:");
            foreach(var parameter in ResetParameters)
            {
                builder.Append(" " + parameter);
            }
            builder.AppendLine("");

            return builder.ToString();
        }
    }
}
