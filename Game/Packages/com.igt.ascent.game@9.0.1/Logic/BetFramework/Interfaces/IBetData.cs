//-----------------------------------------------------------------------
// <copyright file = "IBetData.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// Interface for classes containing bet data.
    /// </summary>
    public interface IBetData
    {
        /// <summary>
        /// Flag indicating whether the bet object is enabled (i.e. betting is allowed).
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Flag indicating the current bet should be committed.
        /// </summary>
        bool Commit { get; }

        /// <summary>
        /// Flag indicating the current bet should start a game.
        /// </summary>
        bool StartGame { get; }

        /// <summary>
        /// Flag indicating the bet has changed.
        /// </summary>
        bool BetChanged { get; }

        /// <summary>
        /// Get the value of a bet variable.
        /// </summary>
        /// <typeparam name="TVariable">Type of the variable.</typeparam>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>The variable's value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variableName"/> is null.</exception>
        /// <exception cref="InvalidVariableException">
        /// Thrown if the requested variable doesn't exist, isn't a field or property, or can't be read.
        /// </exception>
        /// <exception cref="VariableTypeException">
        /// Thrown if <typeparamref name="TVariable"/> does not match the variable's type.
        /// </exception>
        TVariable GetVariable<TVariable>(string variableName);

        /// <summary>
        /// Set the value for a bet variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">The value to set the variable to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variableName"/> is null.</exception>
        /// <exception cref="InvalidVariableException">
        /// Thrown if the requested variable doesn't exist, isn't a field or property.
        /// </exception>
        /// <exception cref="ReadOnlyVariableException">
        /// Thrown if the requested variable can't be written to.
        /// </exception>
        /// <exception cref="VariableTypeException">
        /// Thrown if <paramref name="value"/> does not match the variable's type.
        /// </exception>
        void SetVariable(string variableName, object value);
        
        /// <summary>
        /// Get a list of all variables.
        /// </summary>
        /// <returns>A list of all variables.</returns>
        IEnumerable<MemberInfo> GetVariables();

        /// <summary>
        /// Check if the current bet object is valid by its rules.
        /// </summary>
        /// <returns>True if the bet is legal.</returns>
        bool IsValid();

        /// <summary>
        /// Get the total value of the bet in credits.
        /// </summary>
        /// <returns>The total value in credits.</returns>
        long Total();
        
        /// <summary>
        /// Get a list of all individual bets (e.g. all payline bets).
        /// </summary>
        /// <returns>All current bet items with their values in credits.</returns>
        IEnumerable<KeyValuePair<string, long>> BetDefinitions();

        /// <summary>
        /// Clone the bet data object and return a new instance.
        /// </summary>
        /// <returns>New IBetData instance.</returns>
        IBetData Clone();

        /// <summary>
        /// Clone the bet data object and return a new instance with a specified type.
        /// </summary>
        /// <typeparam name="TBetData">Type to cast the instance as.</typeparam>
        /// <returns>Non-null instance.</returns>
        /// <exception cref="InvalidBetTypeException">Thrown if <typeparamref name="TBetData"/> isn't supported.</exception>
        TBetData Clone<TBetData>() where TBetData: class, IBetData;
    }
}
