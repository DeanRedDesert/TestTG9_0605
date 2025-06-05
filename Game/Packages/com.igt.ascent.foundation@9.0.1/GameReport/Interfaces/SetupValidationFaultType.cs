//-----------------------------------------------------------------------
// <copyright file = "SetupValidationFaultType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Enumeration used to set the fault types of setup validation.
    /// </summary>
    public enum SetupValidationFaultType
    {
        /// <summary>
        /// Warning indicates that the operator should be made aware of the fault,
        /// but the fault should not prevent game play.
        /// </summary>
        Warning,

        /// <summary>
        /// The fault that has been detected is severe enough to prevent game play.
        /// </summary>
        Error
    }
}
