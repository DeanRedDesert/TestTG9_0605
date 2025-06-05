//-----------------------------------------------------------------------
// <copyright file = "IRandomNumberProxyFactory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Interface used to create random number proxies.
    /// </summary>
    public interface IRandomNumberProxyFactory
    {
        /// <summary>
        /// Property used to set the prepicked value provider.
        /// </summary>
        IPrepickedValueProvider PrepickedProvider { set; }

        /// <summary>
        /// Create a random number generator proxy.
        /// </summary>
        /// <returns>A new random number generator proxy.</returns>
        IRandomNumberProxy CreateRngProxy();

        /// <summary>
        /// Create an auditless random number generator proxy.
        /// </summary>
        /// <returns>A new random number generator proxy, without audit capabilities.</returns>
        IAuditlessRandomNumberProxy CreateAuditlessRngProxy();
    }
}