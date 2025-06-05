//-----------------------------------------------------------------------
// <copyright file = "PresentationUpDataAsynchDataMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using CommunicationLib;

    /// <summary>
    ///    Message object that is compromised of the parameters of the
    ///    Presentation Interface Function UpdateAsynchData.
    /// </summary>
    [Serializable]
    public class PresentationUpDateAsynchDataMsg : PresentationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationUpDateAsynchDataMsg()
        {
        }

        /// <summary>
        ///    Constructor for creating PresentationUpDateAsynchDataMsg.
        /// </summary>
        /// <param name="stateName">
        ///    Name of state that the data is intended for.
        /// </param>
        /// <param name="data">
        ///    Data Item list containing updated asynchronous data.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if stateName or data are null.</exception>
        public PresentationUpDateAsynchDataMsg(string stateName, DataItems data)
        {
            if (stateName == null)
            {
                throw new ArgumentNullException("stateName", "Argument cannot be null.");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data", "Argument cannot be null.");
            }

            StateName = stateName;
            Data = data;
        }

        /// <summary>Override base implementation to provide better information.</summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tStateName = " + StateName);

            foreach (KeyValuePair<string, Dictionary<int, object>> kvp in Data)
            {
                builder.AppendLine("\tData:" + kvp.Key);
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
        ///    Name of state that the data is intended for.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        ///    Data Item list containing updated asynchronous data.
        /// </summary>
        public DataItems Data { get; set; }
    }
}
