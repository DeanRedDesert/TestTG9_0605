//-----------------------------------------------------------------------
// <copyright file = "BetVariableAttribute.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Attributes
{
    using System;

    /// <summary>
    /// Indicates a property or field is a bet variable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class BetVariableAttribute : Attribute
    {}
}
