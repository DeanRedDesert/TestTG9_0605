//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendLogicDataServiceNamesMsg.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This message class is responsible for sending the names of services being found by particular 
    /// providers and identifiers.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationSendLogicDataServiceNamesMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="serviceNames">
        /// The dictionary contains the names of providers for Keys,
        /// and a dictionary with identifiers and names of services for Values.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceNames"/>
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceNames"/> is empty.
        /// </exception>
        public GameLogicAutomationSendLogicDataServiceNamesMsg(IDictionary<string, IDictionary<int, string>> 
            serviceNames)
        {
            if(serviceNames == null)
            {
                throw new ArgumentNullException("serviceNames", "Service names cannot be null.");
            }

            if(!serviceNames.Any())
            {
                throw new ArgumentException("Service names cannot be empty.", "serviceNames");
            }
           
            ServiceNames = serviceNames;
        }
                      
        /// <summary>
        /// The dictionary contains the names of providers for Keys,
        /// and a dictionary with identifiers and names of services as Values.
        /// </summary>
        public IDictionary<string, IDictionary<int, string>> ServiceNames { get; private set; }
    }
}
