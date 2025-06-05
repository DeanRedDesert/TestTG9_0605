//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationRequestLogicDataServiceNamesMsg.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This message class is responsible for requesting the logic data service names by
    /// particular providers and identifiers.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationRequestLogicDataServiceNamesMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="serviceNamesRequests">
        /// The dictionary contains provider names for Keys,
        /// and a list of service identifiers for Values.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceNamesRequests"/>
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceNamesRequests"/> is empty.
        /// </exception>
        public GameLogicAutomationRequestLogicDataServiceNamesMsg(IDictionary<string, IList<int>> serviceNamesRequests)
        {
            if(serviceNamesRequests == null)
            {
                throw new ArgumentNullException("serviceNamesRequests", "Service names requests cannot be null.");
            }

            if(!serviceNamesRequests.Any())
            {
                throw new ArgumentException("Service names requests cannot be empty.", "serviceNamesRequests");
            }

            ServiceNamesRequests = serviceNamesRequests;
        }

        /// <summary>
        /// The dictionary contains provider names for Keys and a list of service identifiers for Values.
        /// </summary>
        public IDictionary<string, IList<int>> ServiceNamesRequests { get; private set; }
    }
}
