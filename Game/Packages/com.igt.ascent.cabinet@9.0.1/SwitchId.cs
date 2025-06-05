//-----------------------------------------------------------------------
// <copyright file = "SwitchId.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The button switch Ids supported by SDK.
    /// </summary>
    /// <remarks>These Ids are defined by the foundation.</remarks>
    public enum SwitchId
    {
        /// <summary>Represents an invalid button.</summary>
        InvalidButtonId = 0,

        /// <summary>Switch ID of the 1st "Play N Credits" button.</summary>
        PlayNCredits1Id = 1,

        /// <summary>Switch ID of the 2nd "Play N Credits" button.</summary>
        PlayNCredits2Id = 2,
        
        /// <summary>Switch ID of the 3rd "Play N Credits" button.</summary>
        PlayNCredits3Id = 3,
        
        /// <summary>Switch ID of the 4th "Play N Credits" button.</summary>
        PlayNCredits4Id = 4,
        
        /// <summary>Switch ID of the 5th "Play N Credits" button.</summary>
        PlayNCredits5Id = 5,
        
        /// <summary>Switch ID of the "Repeat Bet" button.</summary>
        RepeatBetId = 7,
        
        /// <summary>Switch ID of the "Cashout" button.</summary>
        CashOutId = 8,
        
        /// <summary>Switch ID of the "Max Bet" button.</summary>
        MaxBetId = 9,
        
        /// <summary>Switch ID of the "Bet One" button.</summary>
        BetOneId = 10,
        
        /// <summary>Switch ID of the 1st "Select N Lines" button.</summary>
        SelectNLines1Id = 11,
        
        /// <summary>Switch ID of the 2nd "Select N Lines" button.</summary>
        SelectNLines2Id = 12,
        
        /// <summary>Switch ID of the 3rd "Select N Lines" button.</summary>
        SelectNLines3Id = 13,
        
        /// <summary>Switch ID of the 4th "Select N Lines" button.</summary>
        SelectNLines4Id = 14,
        
        /// <summary>Switch ID of the 5th "Select N Lines" button.</summary>
        SelectNLines5Id = 15,
        
        /// <summary>Switch ID of the "Double Up" button.</summary>
        DoubleUpId = 16,
        
        /// <summary>Switch ID of the "Take Win" button.</summary>
        TakeWinId = 17, // "Take Win" button of the Australian button panel
        
        /// <summary>Switch ID of the "Information" button.</summary>
        InformationId = 18, // "Information" button of the Australian button panel

        /// <summary>Switch ID of the "Accept" button.</summary>
        AcceptId = 19, // This button belongs to Secondary button panel.

        /// <summary>Switch ID of the "Reject" button.</summary>
        RejectId = 20, // This button belongs to Secondary button panel.

        /// <summary>SwitchId of the first Reset Key.</summary>
        ResetKey = 100,

        /// <summary>SwitchID of the Test Switch button.</summary>
        TestSwitch = 101,

        /// <summary>SwitchId of the second Reset Key.</summary>
        ResetKey2 = 102,

        /// <summary>Switch ID of the Stepper EGM arm/handle.</summary>
        HandleId = 103,
        
        /// <summary>Switch ID of the "Ghost" button.</summary>
        GhostId = 150,
        
        /// <summary>
        /// Switch ID of a special button key combo in tournament that provides additional behaviors that is easy for
        /// the Attendant to access without having to enter the Attendant menu.
        /// </summary>
        /// <remarks>
        /// Additional behaviors such as launching a special menu to configure a seat number for Foundation initiated
        /// Tournaments, and to reset the current Tournament session.
        /// </remarks>
        TournamentMenuButtonId = 151
    }
}