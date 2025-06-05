// -----------------------------------------------------------------------
// <copyright file = "GetGameLevelValuesEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Platform.Interfaces;

    /// <summary>
    /// The arguments and result for the request of getting the theme based game-level data.
    /// </summary>
    [Serializable]
    public class GetGameLevelValuesEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the theme identifier;
        /// </summary>
        public string ThemeIdentifier { get; private set; }

        /// <summary>
        /// Gets the Foundation maintained, raw progressive-level values associated with <see cref="ThemeIdentifier"/>.
        /// </summary>
        public IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> ProgressiveLevelValues { get; private set; }

        /// <summary>
        /// Gets or sets the game-level values after being adjusted by the Report object, which
        /// will be sent back to the Foundation.
        /// </summary>
        public IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> GameLevelValues { get; set; }

        /// <summary>
        /// Gets or sets the error message when getting game-level values failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="GetGameLevelValuesEventArgs"/>.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="progressiveLevelValues">
        /// The paytable-denomination pairs information and related progressive-level values
        /// used for updating game-level values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> is null or empty,
        /// or <paramref name="progressiveLevelValues"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="progressiveLevelValues"/> is empty.
        /// </exception>
        public GetGameLevelValuesEventArgs(string themeIdentifier,
            IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> progressiveLevelValues)
        {
            if(string.IsNullOrEmpty(themeIdentifier))
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(progressiveLevelValues == null)
            {
                throw new ArgumentNullException(nameof(progressiveLevelValues));
            }

            if(!progressiveLevelValues.Any())
            {
                throw new ArgumentException("Raw progressive-level values should not be empty.",
                                            nameof(progressiveLevelValues));
            }

            ThemeIdentifier = themeIdentifier;
            ProgressiveLevelValues = progressiveLevelValues;
        }
    }
}