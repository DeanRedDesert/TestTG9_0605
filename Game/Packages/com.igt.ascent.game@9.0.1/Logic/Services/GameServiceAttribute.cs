//-----------------------------------------------------------------------
// <copyright file = "GameServiceAttribute.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    /// <summary>
    ///    The GameServiceAttribute is used to flag a method, field, or property
    ///    as being accessible as a game service.
    ///    Items tagged with the GameServiceAttribute do not provide asynchronous updates.
    ///    If asynchronous updates are required then the AsynchronousGameServiceAttribute
    ///    should be used.
    /// </summary>
    /// <devdoc>
    ///    "CA1813:AvoidUnsealedAttributes" is suppressed for this attribute because it cannot be sealed
    ///    for inheritance reasons.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class GameServiceAttribute : Attribute {}

    /// <summary>
    ///    The AsynchronousGameServiceAttribute is used to flag a property as being asynchronously
    ///    accessible through a service.
    ///    The class containing the property needs to implement the INotifyAsynchronousProviderChanged interface,
    ///    and the property needs to post a AsynchronousProviderChanged.
    /// </summary>
    /// <devdoc>
    ///    "CA1813:AvoidUnsealedAttributes" is suppressed for this attribute because it cannot be sealed
    ///    for inheritance reasons.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class AsynchronousGameServiceAttribute : GameServiceAttribute { }

    /// <summary>
    ///    The WritableGameServiceAttribute is to flag a property or a method as writable.
    ///    A writable property can be updated by the Presentation in a Presentation complete message.
    ///    A writable method may be used within a Presentation complete message to store data.
    ///    A writable property can be accessed from a state transition message, but a writable method
    ///    may only be accessed from a Presentation complete message.
    /// </summary>
    /// <devdoc>
    ///    "CA1813:AvoidUnsealedAttributes" is suppressed for this attribute because it cannot be sealed
    ///    for inheritance reasons.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class WritableGameServiceAttribute : GameServiceAttribute {}
}
