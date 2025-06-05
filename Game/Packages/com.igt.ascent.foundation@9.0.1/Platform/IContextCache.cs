//-----------------------------------------------------------------------
// <copyright file = "IContextCache.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using Interfaces;

    /// <summary>
    /// This interface defines the methods for managing cached context data
    /// during <see cref="GameMode.Play"/> mode and <see cref="GameMode.Utility"/> mode.
    /// 
    /// Do not call the methods in this interface for <see cref="GameMode.History"/> mode.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    internal interface IContextCache<in TContext> where TContext : class
    {
        /// <summary>
        /// Called when a new context has been activated.
        /// </summary>
        /// <param name="newContext">
        /// The arguments of the new context.  This could be helpful for the interface implementation
        /// to get the cached data for the new context.
        /// </param>
        /// <remarks>
        /// This resets all cached data. Configuration data may change on context changes.
        /// </remarks>
        void NewContext(TContext newContext);
    }
}