// -----------------------------------------------------------------------
// <copyright file = "CabinetDisconnectedException.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;

    /// <summary>
    /// Exception thrown when the cabinet interface is used while the cabinet is disconnected.
    /// </summary>
    [Serializable]
    public class CabinetDisconnectedException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal CabinetDisconnectedException() :
            base("Cabinet is disconnected when a Cabinet Service functionality is being accessed. " +
                 "Make sure you are not using a cached instance of the service.  Always obtain it from CabinetServiceLocator " +
                 "and check the return value to ensure that a functioning service is available; " +
                 "Or implement an ICabinetServiceListener to monitor the availability of the service.")
        {
        }
    }
}