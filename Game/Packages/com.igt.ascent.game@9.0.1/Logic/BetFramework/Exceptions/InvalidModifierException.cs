//-----------------------------------------------------------------------
// <copyright file = "InvalidModifierException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;

    /// <summary>
    /// Thrown if a requested modifier doesn't exist.
    /// </summary>
    public class InvalidModifierException : Exception
    {
        private const string MessageFormat = @"Invalid modifier: {0}";

        /// <summary>
        /// Name of the invalid modifier.
        /// </summary>
        public string ModifierName { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="modifierName">Name of the invalid modifier.</param>
        public InvalidModifierException(string modifierName)
            : base(string.Format(MessageFormat, modifierName))
        {
            ModifierName = modifierName;
        }
    }
}
