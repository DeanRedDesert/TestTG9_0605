// -----------------------------------------------------------------------
// <copyright file = "InitializeGameLevelDataEventArgs.cs" company = "IGT">
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
    /// The argument of the Foundation event that notifies the game to initialize the game level progressive data.
    /// </summary>
    [Serializable]
    public class InitializeGameLevelDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the theme identifier used for initializing for game-level queries.
        /// </summary>
        public string ThemeIdentifier { get; private set; }

        /// <summary>
        /// Gets the paytable-denomination information list associated with <see cref="ThemeIdentifier"/> used
        /// for initializing for game-level queries.
        /// </summary>
        public IEnumerable<PaytableDenominationInfo> PaytableDenominationInfos { get; private set; }

        /// <summary>
        /// Gets or sets the error message when initialization failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="InitializeGameLevelDataEventArgs"/>.
        /// </summary>
        /// <param name="themeIdentifier">
        /// The theme identifier used for initializing for game-level queries.
        /// </param>
        /// <param name="paytableDenominationInfos">
        /// The paytable-denomination pairs information used for initializing for game-level queries.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> is null or empty,
        /// or <paramref name="paytableDenominationInfos"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableDenominationInfos"/> is empty.
        /// </exception>
        public InitializeGameLevelDataEventArgs(string themeIdentifier,
                                                IList<PaytableDenominationInfo> paytableDenominationInfos)
        {
            if(string.IsNullOrEmpty(themeIdentifier))
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(paytableDenominationInfos == null)
            {
                throw new ArgumentNullException(nameof(paytableDenominationInfos));
            }

            if(!paytableDenominationInfos.Any())
            {
                throw new ArgumentException("There should be at least one paytable-denomination pair information",
                                            nameof(paytableDenominationInfos));
            }

            ThemeIdentifier = themeIdentifier;
            PaytableDenominationInfos = paytableDenominationInfos;
        }
    }
}
