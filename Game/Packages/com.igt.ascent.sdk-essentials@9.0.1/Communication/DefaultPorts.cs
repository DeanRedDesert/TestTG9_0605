//-----------------------------------------------------------------------
// <copyright file = "DefaultPorts.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    /// <summary>
    /// The default ports for services in the Ascent SDK.
    /// </summary>
    /// <remarks>
    /// These ports have been selected because they don't conflict with
    /// programs and services on typical development environments.
    /// 
    /// Port 6002 should not be used since it conflicts with LightWave.
    /// </remarks>
    public static class DefaultPorts
    {
        /// <summary>
        /// The default port for the game logic interceptor service.
        /// </summary>
        public const int GameLogicInterceptorService = 6001;

        /// <summary>
        /// The default port for the presentation interceptor service.
        /// </summary>
        public const int PresentationInterceptorService = 6004;

        /// <summary>
        /// The default port for the fast play controller.
        /// </summary>
        public const int FastPlayController = 6003;
    }
}
