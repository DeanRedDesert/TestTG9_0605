//-----------------------------------------------------------------------
// <copyright file = "ReadOnlyVariableException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Exception thrown when variable is read only.
    /// </summary>
    public class ReadOnlyVariableException: Exception
    {
        private const string MessageFormat = @"Variable is read only: {0}";

        /// <summary>
        /// Read only member info.
        /// </summary>
        public MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="memberInfo">Name of the invalid modifier.</param>
        public ReadOnlyVariableException(MemberInfo memberInfo)
            : base(string.Format(MessageFormat, memberInfo))
        {
            MemberInfo = memberInfo;
        }
    }
}
