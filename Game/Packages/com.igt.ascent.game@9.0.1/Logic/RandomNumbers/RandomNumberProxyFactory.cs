//-----------------------------------------------------------------------
// <copyright file = "RandomNumberProxyFactory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// This is a factory class used to produce <see cref="IRandomNumberProxy"/> objects, which are used by the game
    /// to request random numbers for use in determining game logic outcomes. This implementation returns proxies
    /// that use <see cref="IGameLib"/> to request random numbers from the Foundation.
    /// </summary>
    public class RandomNumberProxyFactory : IRandomNumberProxyFactory
    {
        private readonly IGameLib gameLib;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNumberProxyFactory"/> class with the given
        /// game lib instance.
        /// </summary>
        /// <param name="gameLib">The <see cref="IGameLib"/> reference to use.</param>
        public RandomNumberProxyFactory(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib");
            }
            this.gameLib = gameLib;
        }

        #region IRandomNumberProxyFactory implementation

        /// <inheritdoc/>
        public IPrepickedValueProvider PrepickedProvider
        {
            set
            {
                // Forward this provider to the GameLib if it is supported.
                var restrictedGameLib = gameLib as IGameLibRestricted;
                if(restrictedGameLib != null)
                {
                    restrictedGameLib.SetPrepickedValueProvider(value);
                }
            }
        }

        /// <inheritdoc/>
        public IRandomNumberProxy CreateRngProxy()
        {
            return new RandomNumberProxy(gameLib);
        }

        /// <inheritdoc/>
        public IAuditlessRandomNumberProxy CreateAuditlessRngProxy()
        {
            return new AuditlessRandomNumberProxy(gameLib);
        }
        
        #endregion
    }
}