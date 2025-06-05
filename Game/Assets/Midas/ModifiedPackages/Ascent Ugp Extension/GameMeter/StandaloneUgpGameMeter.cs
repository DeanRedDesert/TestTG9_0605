//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpGameMeter.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter
{
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpGameMeter extended interface.
    /// </summary>
    internal sealed class StandaloneUgpGameMeter : IUgpGameMeter, IInterfaceExtension
    {
        #region IUgpGameMeter Implementation

        /// <inheritdoc/>
        public void SendCurrentBetPerLineAndSelectedLines(long betPerLine, int selectedLines)
        {
        }

        /// <inheritdoc/>
        public void SendUpdateGameBetMeterOnBet(string horizontalKey, string verticalKey)
        {
        }

        /// <inheritdoc/>
        public void SendUpdateGameCreditDenom(long creditDenom)
        {
        }

        #endregion
    }
}
