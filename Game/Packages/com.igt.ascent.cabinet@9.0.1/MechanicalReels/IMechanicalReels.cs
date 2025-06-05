//-----------------------------------------------------------------------
// <copyright file = "IMechanicalReels.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for controlling mechanical reel devices.
    /// </summary>
    public interface IMechanicalReels
    {
        /// <summary>
        /// Event posted whenever a registered reel status changes.
        /// </summary>
        event EventHandler<ReelStatusEventArgs> ReelStatusChangedEvent;

        /// <summary>
        /// Event posted whenever the currently spinning reels go through a state change of type <see cref="ReelsSpunEventArgs"/>.
        /// </summary>
        event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChangedEvent;

        /// <summary>
        /// Get a list of the connected reel devices.
        /// </summary>
        /// <returns>A description of each of the available reel devices.</returns>
        ICollection<ReelFeatureDescription> GetReelDevices();

        /// <summary>                                           
        /// Indicate that a device is required for game play. This allows for the system to behave correctly in cases
        ///  where the device is not present.
        /// </summary>
        /// <param name="featureId">The device to require.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="featureId"/>  is null or empty.</exception>    
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult RequireDevice(string featureId);

        /// <summary>
        /// Used to set a reel device online or offline. Online means the device is connected and acquired;
        /// offline means either the device is disconnected or not acquired.
        /// Used for initializing and synchronizing reel events and structures.
        /// </summary>
        /// <param name="featureId">The device to set the status for.</param>
        /// <param name="online">If the device is currently online or offline.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/>  is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SetOnlineStatus(string featureId, bool online);

        /// <summary>
        /// Used to set the reel recovery behavior. Depending on the state of the game the recovery behavior of the
        /// reels needs to be specified. The foundation uses this behavior to recover the reels from a safe state.
        /// A safe state can be initiated by tilt conditions.
        /// </summary>
        /// <param name="featureId">The device to set the recovery behavior for.</param>
        /// <param name="order">The order the reels should recover.</param>
        /// <param name="direction">The direction the reels should spin while recovering.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/>  is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SetRecoveryBehavior(string featureId, RecoveryOrder order, ReelDirection direction);

        /// <summary>
        /// Used to tell the foundation to move reels to the specified reel stops in the shortest and fastest possible manner.
        /// Depending on the implementation, this may or may not move reels that are already at the desired stop location.
        /// This feature is used for various recovery scenarios and helps to differentiate this type of movement from a 
        /// true spin.
        /// </summary>
        /// <param name="featureId">The device target.</param>
        /// <param name="reelStops">A collection of reel stops, one for each reel.</param>
        /// <param name="foundationHandlesTiltWhileRecovering">Out parameter: true if the implementation (ie. a standard foundation)
        /// tilts during the handling of this command, and the game does not have to manage or check if the reels are spinning; false
        /// if the implementation does not do this.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/>  is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SetToPosition(string featureId, ICollection<byte> reelStops, out bool foundationHandlesTiltWhileRecovering);

        /// <summary>
        /// Used to force the stop order of the reels. This ensures that the reels will stop in the correct order even
        /// if the spin times are not close enough to guarantee it. The function takes a list of reel indexes; the order 
        /// of these indexes will be the desired stop order. This stop order persists for all subsequent spins until
        /// the order is removed by sending an empty collection for the reels parameter.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="reels">
        /// Collection containing reel numbers. The order of the collection indicates the order the reels should stop.
        /// An empty collection removes any previously set stop order.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="reels"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SetStopOrder(string featureId, ICollection<byte> reels);

        /// <summary>
        /// Sets stops to use for an active synchronous spin. If a synchronous spin has been initiated which spins
        /// indefinitely then the stops for that spin must be set before attempting to stop the reels.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="reelStops">A list of reels and where to stop them.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="reelStops"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reelStops"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SetSynchronousStops(string featureId, ICollection<ReelStop> reelStops);

        /// <summary>
        /// Instruct the reels specified by the profiles in the specified feature to spin. The parameters and
        /// attributes in the profile will be applied to the spin. This type of spin does not support synchronous stops
        /// (slamming).
        /// </summary>
        /// <param name="featureId">The device with reels to spin.</param>
        /// <param name="spinProfiles">List of <see cref="SpinProfile"/> profiles to use for this spin.
        /// If spin attributes are to be used, please see the comments for <see cref="ApplyAttributes"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="spinProfiles"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="spinProfiles"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult Spin(string featureId, ICollection<SpinProfile> spinProfiles);

        /// <summary>
        /// Stop the specified reels at the specified stops as soon as is possible. This is most commonly used when a
        /// reel has been told to spin indefinitely.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="reelStops">A list of reels and where to stop them.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="reelStops"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reelStops"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult Stop(string featureId, ICollection<ReelStop> reelStops);

        /// <summary>
        /// Used to spin the reels in a synchronous manner that allows them to be stopped in unison if desired.
        /// Synchronous spins do not have access to all the same capabilities as normal spins.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="speedIndex">The speed to use. This is an index into the supported list of speeds.</param>
        /// <param name="spinProfiles">List of <see cref="SynchronousSpinProfile"/> profiles to use for the synchronous spin.
        /// Note that spin attributes are not availabe for this kind of spin.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="spinProfiles"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="InvalidSpeedIndexException">
        /// Thrown if the given speed index is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="spinProfiles"/> is empty.
        /// </exception>
        /// <exception cref="InvalidReelDirectionException">
        /// Thrown if any of <paramref name="spinProfiles"/> uses
        /// <see cref="ReelDirection.Shortest"/> as the reel direction.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <exception cref="InvalidReelStopException">
        /// Thrown if any of the specified stops are out of range.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SynchronousSpin(string featureId, ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles);

        /// <summary>
        /// Synchronously stop the specified reels. A synchronous stop needs to have been set using the set synchronous
        /// stop command or the synchronous spin command; if a stop was not set then the request to stop will be
        /// ignored.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="reels"> Collection containing the reels to synchronously stop. </param>  
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="reels"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="reels"/> is empty.
        /// </exception>
        /// <exception cref="DuplicateReelException">
        /// Thrown when a single reel is specified multiple times.
        /// </exception>
        /// <exception cref="InvalidReelException">
        /// Thrown when a specified reel is not within the range of valid reels.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult SynchronousStop(string featureId, ICollection<byte> reels);

        /// <summary>
        /// Apply attributes to reels without spinning. If they are stopped, they will begin the movement specified by the attribute.
        /// If they are moving with an existing attribute, and the new attribute turns it off, the reel will stop. If the specified
        /// attribute modifies an existing attribute, or turns on a different attribute, the behavior is unspecified.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="attributes">A collection of reel index vs. attributes to apply.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        /// <remarks>  Note for attributes: 
        /// - Attributes aren't supported (properly) in Pre-2.6.1 foundation versions.
        /// - Cock and bounce are not supported by the reel firmware so they are always ignored.
        /// - Shake should only be set in a <see cref="ApplyAttributes"/> command. If it is used in a spin, it will be ignored.
        ///</remarks>
        ReelCommandResult ApplyAttributes(string featureId, IDictionary<byte, SpinAttributes> attributes);

        /// <summary>
        /// Change the speed and/or direction of currently spinning reels.
        /// </summary>
        /// <param name="featureId">The device to control.</param>
        /// <param name="changeSpeedProfiles">A collection of reel index vs. <see cref="ChangeSpeedProfile"/> to apply this command to.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        ReelCommandResult ChangeSpeed(string featureId, IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles);
    }
}
