//-----------------------------------------------------------------------
// <copyright file = "IGameHistorySupplementalCategory.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.GameHistorySupplemental;

    /// <summary>
    /// The GameHistorySupplemental category of messages allows the game to report supplemental history information to
    /// the foundation
    /// Category: 1040; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IGameHistorySupplementalCategory
    {
        /// <summary>
        /// Client's request to the foundation to provide supplemental data for the current game cycle that will be
        /// stored in history and exported from the History menu page. This message may be sent at most once, and if
        /// sent it must be sent in the Finalized game cycle state, otherwise will cause an F2X exception. If this
        /// message is sent more than once in the Finalized state, subsequent calls will overwrite the data stored by
        /// the foundation in Release, and will cause a SystemError in Development. This message is not permitted in
        /// History mode. This message is ignored in Utility Mode.
        /// </summary>
        /// <param name="exportDataFormatIdentifier">
        /// 
        /// </param>
        /// <param name="exportData">
        /// 
        /// </param>
        void SetExportData(DataFormatIdentifier exportDataFormatIdentifier, byte[] exportData);

    }

}

