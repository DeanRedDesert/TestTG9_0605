//-----------------------------------------------------------------------
// <copyright file = "TiltConverters.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;

    /// <summary>
    /// A collection of extension methods useful for working with GameTiltDefinitions
    /// </summary>
    public static class TiltConverters
    {
        #region Priority Converters

        /// <summary>
        /// Convert a GameTiltDefinitionPriority to a TiltPriority.
        /// </summary>
        /// <param name="tiltDefPriority">The GameTiltDefinitonPriority to convert.</param>
        /// <returns>returns an equivalent TiltPriority</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltDefPriority"/> is an invalid value.</exception>
        public static TiltPriority ToTiltPriority(this GameTiltDefinitionPriority tiltDefPriority)
        {
            switch (tiltDefPriority)
            {
                case GameTiltDefinitionPriority.Low:
                    {
                        return TiltPriority.Low;
                    }
                case GameTiltDefinitionPriority.Medium:
                    {
                        return TiltPriority.Medium;
                    }
                case GameTiltDefinitionPriority.High:
                    {
                        return TiltPriority.High;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltDefPriority));
            }
        }
        
        /// <summary>
        /// Convert a TiltPriority to a GameTiltDefinitionPriority.
        /// </summary>
        /// <param name="tiltPriority">The TiltPriority to convert.</param>
        /// <returns>returns an equivalent GameTiltDefinitionPriority</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltPriority"/> is an invalid value.</exception>
        public static GameTiltDefinitionPriority ToTiltDefinitionPriority(this TiltPriority tiltPriority)
        {
            switch (tiltPriority)
            {
                case TiltPriority.Low:
                    {
                        return GameTiltDefinitionPriority.Low;
                    }
                case TiltPriority.Medium:
                    {
                        return GameTiltDefinitionPriority.Medium;
                    }

                case TiltPriority.High:
                    {
                        return GameTiltDefinitionPriority.High;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltPriority));
            }
        }

        #endregion

        #region GamePlayBehavior Converters

        /// <summary>
        /// Convert a GameTiltDefinitionGamePlayBehavior to a TiltGamePlayBehavior.
        /// </summary>
        /// <param name="tiltDefGamePlayBehavior">The GameTiltDefinitionGamePlayBehavior to convert.</param>
        /// <returns>returns an equivalent TiltGamePlayBehavior</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltDefGamePlayBehavior"/> is an invalid value.</exception>
        public static TiltGamePlayBehavior ToTiltGamePlayBehavior(this GameTiltDefinitionGamePlayBehavior tiltDefGamePlayBehavior)
        {
            switch (tiltDefGamePlayBehavior)
            {
                case GameTiltDefinitionGamePlayBehavior.Blocking:
                    {
                        return TiltGamePlayBehavior.Blocking;
                    }
                case GameTiltDefinitionGamePlayBehavior.NonBlocking:
                    {
                        return TiltGamePlayBehavior.NonBlocking;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltDefGamePlayBehavior));
            }
        }

        /// <summary>
        /// Convert a TiltGamePlayBehavior to a GameTiltDefinitionGamePlayBehavior.
        /// </summary>
        /// <param name="tiltGamePlayBehavior">The TiltGamePlayBehavior to convert.</param>
        /// <returns>returns an equivalent GameTiltDefinitionGamePlayBehavior</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltGamePlayBehavior"/> is an invalid value.</exception>
        public static GameTiltDefinitionGamePlayBehavior ToTiltDefinitionGamePlayBehavior(this TiltGamePlayBehavior tiltGamePlayBehavior)
        {
            switch (tiltGamePlayBehavior)
            {
                case TiltGamePlayBehavior.Blocking:
                    {
                        return GameTiltDefinitionGamePlayBehavior.Blocking;
                    }
                case TiltGamePlayBehavior.NonBlocking:
                    {
                        return GameTiltDefinitionGamePlayBehavior.NonBlocking;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltGamePlayBehavior));
            }
        }

        #endregion

        #region DiscardBehavior Converters

        /// <summary>
        /// Convert a GameTiltDefinitionDiscardBehavior to a TiltDiscardBehavior.
        /// </summary>
        /// <param name="tiltDefDiscardBehavior">The GameTiltDefinitionDiscardBehavior to convert.</param>
        /// <returns>returns an equivalent TiltDiscardBehavior</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltDefDiscardBehavior"/> is an invalid value.</exception>
        public static TiltDiscardBehavior ToTiltDiscardBehavior (this GameTiltDefinitionDiscardBehavior tiltDefDiscardBehavior)
        {
            switch (tiltDefDiscardBehavior)
            {
                case GameTiltDefinitionDiscardBehavior.Never:
                    {
                        return TiltDiscardBehavior.Never;
                    }
                case GameTiltDefinitionDiscardBehavior.OnGameTermination:
                    {
                        return TiltDiscardBehavior.OnGameTermination;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltDefDiscardBehavior));
            }
        }

        /// <summary>
        /// Convert a TiltDiscardBehavior to a GameTiltDefinitionDiscardBehavior.
        /// </summary>
        /// <param name="tiltDiscardBehavior">The TiltDiscardBehavior to convert.</param>
        /// <returns>returns an equivalent GameTiltDefinitionDiscardBehavior</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="tiltDiscardBehavior"/> is an invalid value.</exception>
        public static GameTiltDefinitionDiscardBehavior ToTiltDefinitionDiscardBehavior(this TiltDiscardBehavior tiltDiscardBehavior)
        {
            switch (tiltDiscardBehavior)
            {
                case TiltDiscardBehavior.Never:
                    {
                        return GameTiltDefinitionDiscardBehavior.Never;
                    }
                case TiltDiscardBehavior.OnGameTermination:
                    {
                        return GameTiltDefinitionDiscardBehavior.OnGameTermination;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(tiltDiscardBehavior));
            }
        }

        #endregion
    }
}