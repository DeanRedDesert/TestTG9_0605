// -----------------------------------------------------------------------
// <copyright file = "PropertyRelay.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    /// <summary>
    /// This enumeration defines properties whose data is sent to the shell by Foundation,
    /// and will be relayed to coplayers to consume.
    /// </summary>
    internal enum PropertyRelay
    {
        PlayerBettableMeter,
        CanBetFlag,
        CanCommitGameCycleFlag,
        CultureString,
    }
}