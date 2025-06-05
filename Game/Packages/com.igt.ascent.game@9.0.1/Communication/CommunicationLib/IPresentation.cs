//-----------------------------------------------------------------------
// <copyright file = "IPresentation.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;

    /// <summary>WCF Interface for the Presentation Module</summary>
    public interface IPresentation
    {
        /// <summary>Tell the Presentation that there has been an update to requested asynchronous data.</summary>
        /// <param name="stateName">Name of state that the data is intended for.</param>
        /// <param name="data">Data Item list containing updated asynchronous data.</param>
        void UpdateAsynchData(string stateName, DataItems data);

        /// <summary>Tell the Presentation to initiate a state transition to the specified state.</summary>
        /// <param name="stateName">Name of the state to start</param>
        /// <param name="stateData">List of data the state requested during negotiation.</param>
        void StartState(string stateName, DataItems stateData);
    }
}
