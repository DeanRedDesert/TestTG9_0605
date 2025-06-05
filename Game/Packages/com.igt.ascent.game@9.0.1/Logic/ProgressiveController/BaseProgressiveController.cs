//-----------------------------------------------------------------------
// <copyright file = "BaseProgressiveController.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// The base class for all progressive controllers. Should be inherited from to create custom progressive controllers.
    /// </summary>
    /// <remarks>
    /// Outside of SDK Core, this class should be inherited from to create new progressive controller types
    /// and not <see cref="AbstractProgressiveController"/>.
    /// </remarks>
    public class BaseProgressiveController : AbstractProgressiveController
    {
        /// <inheritdoc />
        public override event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        /// <inheritdoc />
        public BaseProgressiveController(IGameLib iGameLib, string name) : base(iGameLib, name)
        {
        }
    }
}
