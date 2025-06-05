//-----------------------------------------------------------------------
// <copyright file = "PresentationStartStateMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CommunicationLib;

    /// <summary>
    ///    Message object that is compromised of the parameters of the
    ///    Presentation Interface Function StartState.
    /// </summary>
    [Serializable]
    public class PresentationStartStateMsg : PresentationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationStartStateMsg()
        {
        }

        /// <summary>
        ///    Constructor for creating PresentationStartStateMsg.
        /// </summary>
        /// <param name="stateName">
        ///    Name of the state to start.
        /// </param>
        /// <param name="stateData">
        ///    List of data the state requested during negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown is stateName or stateData is null.</exception>
        public PresentationStartStateMsg(string stateName, DataItems stateData)
        {
            if (stateName == null)
            {
                throw new ArgumentNullException("stateName", "Argument may not be null.");
            }
            if (stateData == null)
            {
                throw new ArgumentNullException("stateData", "Argument may not be null.");
            }

            StateName = stateName;
            StateData = stateData;
        }

        /// <summary>Override base implementation to provide better information.</summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tStateName = " + StateName);

            foreach (KeyValuePair<string, Dictionary<int, object>> kvp in StateData)
            {
                builder.AppendLine("\tStateData:" + kvp.Key);
                if (kvp.Value != null)
                {
                    foreach (KeyValuePair<int, object> kvp2 in kvp.Value)
                    {
                        builder.AppendLine("\t\t" + kvp2.Key + ": " + kvp2.Value);
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        ///    Name of the state to start.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        ///    List of data the state requested during negotiation.
        /// </summary>
        public DataItems StateData { get; set; }
    }
}
