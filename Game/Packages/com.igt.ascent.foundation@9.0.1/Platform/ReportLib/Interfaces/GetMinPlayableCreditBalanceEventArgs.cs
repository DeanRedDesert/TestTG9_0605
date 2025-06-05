//-----------------------------------------------------------------------
// <copyright file = "GetMinPlayableCreditBalanceEventArgs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using System.Collections.Generic;

    /// <summary>
    /// The arguments and result for the minimum playable credit balance request.
    /// </summary>
    public class GetMinPlayableCreditBalanceEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// The list of requests for the minimum playable credit balance.
        /// </summary>
        public IList<MinPlayableCreditBalanceRequest> Requests { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="GetMinPlayableCreditBalanceEventArgs"/>.
        /// </summary>
        /// <param name="requests">TThe list of requests for the minimum playable credit balance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="requests"/> is null.
        /// </exception>
        public GetMinPlayableCreditBalanceEventArgs(IList<MinPlayableCreditBalanceRequest> requests)
        {
            Requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }
    }
}
