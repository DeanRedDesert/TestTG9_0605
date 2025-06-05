// -----------------------------------------------------------------------
// <copyright file = "IParcelCallRouter.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System.Collections.Generic;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines the functionality to route an incoming
    /// parcel call from shell to coplayers.
    /// </summary>
    /// <remarks>
    /// Incoming parcel calls are only sent to the shell, who is then
    /// responsible for routing the calls to different coplayers.
    /// 
    /// The implementation of this interface will help the shell with
    /// the routing work.
    /// </remarks>
    public interface IParcelCallRouter
    {
        /// <summary>
        /// Gets the list of coplayers to which the incoming parcel call is to be routed by
        /// parsing the binary payload and/or recognizing the extension that sends the parcel call.
        /// </summary>
        /// <remarks>
        /// An overloaded version of the method is defined in case, even though unlikely,
        /// whether the incoming parcel call is transactional or not matters to the routing logic.
        /// </remarks>
        /// <param name="nonTransactionalParcelCall">
        /// The incoming non transactional parcel call to route.
        /// </param>
        /// <param name="shellStateMachine">
        /// The shell state machine object, which allows the router to access state machine specific data.
        /// </param>
        /// <returns>
        /// The list of coplayer IDs to which the incoming parcel call is to be routed;
        /// <see langword="null"/> if the parcel call should not be routed to any coplayer;
        /// An empty list if the parcel call should be routed to all running coplayers.
        /// </returns>
        IReadOnlyList<int> GetTargetCoplayers(NonTransactionalParcelCallReceivedEventArgs nonTransactionalParcelCall, object shellStateMachine);

        /// <summary>
        /// Gets the list of coplayers to which the incoming parcel call is to be routed by
        /// parsing the binary payload and/or recognizing the extension that sends the parcel call.
        /// </summary>
        /// <remarks>
        /// An overloaded version of the method is defined in case, even though unlikely,
        /// whether the incoming parcel call is transactional or not matters to the routing logic.
        /// </remarks>
        /// <param name="transactionalParcelCall">
        /// The incoming transactional parcel call.
        /// </param>
        /// <param name="shellStateMachine">
        /// The shell state machine object, which allows the router to access state machine specific data.
        /// </param>
        /// <returns>
        /// The list of coplayer IDs to which the incoming parcel call is to be routed;
        /// <see langword="null"/> if the parcel call should not be routed to any coplayer;
        /// An empty list if the parcel call should be routed to all running coplayers.
        /// </returns>
        IReadOnlyList<int> GetTargetCoplayers(TransactionalParcelCallReceivedEventArgs transactionalParcelCall, object shellStateMachine);
    }
}