//-----------------------------------------------------------------------
// <copyright file = "IForceGameCompletionCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using Schemas.Internal;

    /// <summary>
    /// ForceGameCompletion category of messages.  Category: 15  Version: 1  Based on GLI Standard #23, Video Lottery Terminal Disable Procedures.
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IForceGameCompletionCategory
    {
        /// <summary>
        /// Message from the bin querying the current force game-completion status.
        /// </summary>
        /// <returns>
        /// The status of the force game-completion.
        /// </returns>
        ForceGameCompletionStatusType QueryForceGameCompletionStatus();

    }

}

