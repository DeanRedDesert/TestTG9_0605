//-----------------------------------------------------------------------
// <copyright file = "IContextCache.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    /// <summary>
    /// This interface defines the methods for managing cached context data.
    /// </summary>
    internal interface IContextCache
    {
        /// <summary>
        /// Called when a new context has been activated.
        /// </summary>
        /// <remarks>This resets all cached data. Configuration data may change on context changes.</remarks>
        void NewContext();
    }
}
