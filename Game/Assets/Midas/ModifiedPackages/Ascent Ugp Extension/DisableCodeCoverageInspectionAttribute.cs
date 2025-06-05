//-----------------------------------------------------------------------
// <copyright file = "DisableCodeCoverageInspectionAttribute.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp
{
    using System;

    /// <summary>
    /// Used to disable code coverage inspection in UGP interface extension project. It should be removed at
    /// the same time at cleaning up UGP category message classes when C3G category codes are ready for UGP
    /// category message.
    /// </summary>
    public sealed class DisableCodeCoverageInspectionAttribute : Attribute
    {
    }
}
