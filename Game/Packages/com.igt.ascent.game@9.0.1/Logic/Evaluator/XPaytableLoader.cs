// -----------------------------------------------------------------------
// <copyright file = "XPaytableLoader.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using PaytableLoader.Interfaces;
    using Schemas;

    /// <summary>
    /// A class that loads paytables in the Ascent XPaytable format.
    /// </summary>
    public class XPaytableLoader : IPaytableLoader
    {
        private Paytable paytable;
        private GenericXPaytableData genericXPaytableData;

        /// <inheritdoc/>
        public PaytableLoadResult LoadPaytable(string paytableFileName, string paytableId)
        {
            var loadMessage = "";

            try
            {
                paytable = Paytable.Load(paytableFileName);
            }
            // If there are any problems then the Xpaytable file contains corrupted Xml, or is of the wrong version.
            catch(Exception ex)
            {
                loadMessage = string.Format("The load attempt failed with exception: {0}", ex.Message);
                return new PaytableLoadResult(false, paytableFileName, PaytableType.Xpaytable, loadMessage, paytableId, null);
            }

            genericXPaytableData = new GenericXPaytableData
            {
                RawPaytable = paytable,
                BaseRtpPercent = paytable.Abstract.MinPaybackPercentageWithoutProgressiveContributions,
                GameDescription = paytable.Abstract.gameID,
                LegacyPaytableName = paytable.Abstract.gameID
            };

            return new PaytableLoadResult(false, paytableFileName, PaytableType.Xpaytable, loadMessage, paytableId, genericXPaytableData);
        }
    }
}
