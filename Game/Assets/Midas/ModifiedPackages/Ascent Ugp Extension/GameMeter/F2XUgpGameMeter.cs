//-----------------------------------------------------------------------
// <copyright file = "F2XUgpGameMeter.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter
{
    using System;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpGameMeter extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpGameMeter : IUgpGameMeter, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpGameMeter category handler.
        /// </summary>
        private readonly IUgpGameMeterCategory ugpGameMeterCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpGameMeter"/>.
        /// </summary>
        /// <param name="ugpGameMeterCategory">
        /// The UgpGameMeter category used to communicate with the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="ugpGameMeterCategory"/> is null.
        /// </exception>
        public F2XUgpGameMeter(IUgpGameMeterCategory ugpGameMeterCategory)
        {
            this.ugpGameMeterCategory = ugpGameMeterCategory ?? throw new ArgumentNullException(nameof(ugpGameMeterCategory));
        }

        #endregion

        #region IUgpGameMeter Implementation

        /// <inheritdoc/>
        public void SendCurrentBetPerLineAndSelectedLines(long betPerLine, int selectedLines)
        {
            ugpGameMeterCategory.SendCurrentBetPerLineAndSelectedLines(betPerLine, selectedLines);
        }

        /// <inheritdoc/>
        public void SendUpdateGameBetMeterOnBet(string horizontalKey, string verticalKey)
        {
            ugpGameMeterCategory.SendUpdateGameBetMeterOnBet(horizontalKey, verticalKey);
        }

        /// <inheritdoc/>
        public void SendUpdateGameCreditDenom(long creditDenom)
        {
            ugpGameMeterCategory.SendUpdateGameCreditDenom(creditDenom);
        }

        #endregion
    }
}
