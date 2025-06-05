//-----------------------------------------------------------------------
// <copyright file = "BetModifierAttribute.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Attributes
{
    using System;

    /// <summary>
    /// Indicates a method is a bet modifier.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class BetModifierAttribute : Attribute
    { }
}
