//-----------------------------------------------------------------------
// <copyright file = "DataAccessing.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    /// <summary>
    /// The DataAccessing enumeration is used to represent
    /// the different types of access to a pieced of data.
    /// 
    /// This is only used in Standalone GameLib.
    /// </summary>
    internal enum DataAccessing
    {
        /// <summary>
        /// The data is being read.
        /// </summary>
        Read,

        /// <summary>
        /// The data being written.
        /// </summary>
        Write,

        /// <summary>
        /// The data is being removed.
        /// </summary>
        Remove,
    }
}
