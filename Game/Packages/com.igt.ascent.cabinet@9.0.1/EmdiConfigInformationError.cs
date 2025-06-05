//-----------------------------------------------------------------------
// <copyright file = "EmdiConfigInformationError.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration describing types of errors that may arise when getting
    /// EMDI config information.
    /// </summary>
    public enum EMDIConfigInformationError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None,

        /// <summary>
        /// Some undefined error occured.
        /// </summary>
        OtherError
    }
}