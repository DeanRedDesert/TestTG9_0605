//-----------------------------------------------------------------------
// <copyright file = "SystemProgressiveData.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    /// <summary>
    /// Class for storing information about a system progressive.
    /// </summary>
    /// <remarks>This class is intended for conveying data between the game control category and the GameLib.</remarks>
    public class SystemProgressiveData
    {
        /// <summary>
        /// The amount of the progressive in cents.
        /// </summary>
        public long Amount { private set; get; }

        /// <summary>
        /// The game level of the progressive.
        /// </summary>
        public uint GameLevel { private set; get; }

        /// <summary>
        /// The prize string associated with the progressive.
        /// </summary>
        public string PrizeString { private set; get; }

        /// <summary>
        /// Create an instance of the progressive data.
        /// </summary>
        /// <param name="amount">The amount of the progressive in cents.</param>
        /// <param name="level">The game level of the progressive.</param>
        /// <param name="prizeString">Prize string associated with the progressive.</param>
        public SystemProgressiveData(long amount, uint level, string prizeString)
        {
            Amount = amount;
            GameLevel = level;
            PrizeString = prizeString;
        }
    }
}
