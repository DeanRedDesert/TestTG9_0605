// -----------------------------------------------------------------------
// <copyright file = "ModuleException.cs" company = "IGT">
//     Copyright © 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework
{
    using System;

    /// <summary>
    /// Thrown from ModuleCommand methods and will be sent as an error response.
    /// </summary>
    public class ModuleException : Exception
    {
        public readonly string ErrorMessage;
        public readonly AutomationModule.ErrorResponses ErrorResponse;

        public ModuleException(string errorMessage, AutomationModule.ErrorResponses errorResponse)
        {
            ErrorMessage = errorMessage;
            ErrorResponse = errorResponse;
        }
    }
}