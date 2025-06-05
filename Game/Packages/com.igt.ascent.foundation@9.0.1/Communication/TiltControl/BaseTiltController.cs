// -----------------------------------------------------------------------
// <copyright file = "BaseTiltController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.TiltControl
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using Tilts;

    /// <summary>
    /// A base class that manages the registration of a list of <see cref="IActiveTilt"/> objects.
    /// </summary>
    internal abstract class BaseTiltController
    {
        #region Internal Data Types

        /// <summary>
        /// A tuple that contains information about what tilts needs to be registers and/or unregistered because of an action.
        /// </summary>
        internal class TiltInfoTuple
        {
            /// <summary>
            /// Registered tilt that needs to be posted.
            /// </summary>
            public TiltInfo Register;

            /// <summary>
            /// Unregistered tilt that needs to be cleared.
            /// </summary>
            public TiltInfo Unregister;
        }

        #endregion

        #region Fields

        /// <summary>
        /// The list of tilts that are being managed.
        /// </summary>
        protected readonly List<TiltInfo> TiltInfoList;

        #endregion Fields

        #region Constants

        /// <summary>
        /// The total number of tilts that the application can have registered with the Foundation.
        /// </summary>
        /// <remarks>This isn't coordinated anywhere, but coordinated out of band.</remarks>
        protected const int RegisteredFoundationTiltTotalCount = 3;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="BaseTiltController"/>.
        /// </summary>
        internal BaseTiltController()
        {
            TiltInfoList = new List<TiltInfo>();
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Posts a tilt that implements the <see cref="IActiveTilt"/> interface.
        /// </summary>
        /// <remarks>This operation requires an open transaction.</remarks>
        /// <param name="tilt">The ITilt and ICompactSerializable object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null or empty, or exceeds the maximum number of characters allowed.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <exception cref="TiltLogicException">
        /// Thrown if a tilt is asked to be posted but has already been posted.
        /// </exception>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        public virtual bool PostTilt(IActiveTilt tilt, string key, IEnumerable<object> titleFormat,
                                     IEnumerable<object> messageFormat)
        {
            if(tilt == null)
            {
                throw new ArgumentNullException(nameof(tilt));
            }

            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("A tilt key must be provided", nameof(key));
            }

            if(key.Length > MaxTiltNameLength)
            {
                throw new ArgumentException(
                    $"The key that will be used to track this tilt cannot exceed {MaxTiltNameLength} " + "characters.");
            }

            if(TiltPresent(key))
            {
                throw new TiltLogicException(key, "Tilt cannot be posted, it is already posted.");
            }

            var addTilt = new TiltInfo(key, tilt, titleFormat, messageFormat);

            var registrationInfo = AddTilt(addTilt);

            return HandleRegistration(registrationInfo);
        }

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <remarks>
        /// This operation requires an open transaction.
        /// </remarks>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        public virtual bool ClearTilt(string key)
        {
            return ClearTilt(key, true);
        }

        /// <summary>
        /// Returns the tilt status of the application.
        /// </summary>
        /// <returns>
        /// Returns true if at least one tilt is currently posted by the application.
        /// </returns>
        public virtual bool IsTilted()
        {
            return TiltInfoList.Count > 0;
        }

        /// <summary>
        /// Returns the tilt status of a particular tilt key.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <returns>
        /// Returns true if the tilt is currently posted.
        /// </returns>
        public virtual bool TiltPresent(string key)
        {
            return TiltInfoList.Exists(tilt => tilt.Key == key);
        }

        #endregion Public Methods

        #region Protected Fields

        /// <summary>
        /// The max length of the tilt name allowed by the Foundation.
        /// </summary>
        protected virtual int MaxTiltNameLength => 39;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sends a request to the Foundation to clear a tilt.
        /// </summary>
        /// <param name="tiltKey">
        /// The key that was used to track a tilt.
        /// </param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        protected abstract bool SendClearTilt(string tiltKey);

        /// <summary>
        /// Sends a request to the Foundation to create a tilt.
        /// </summary>
        /// <param name="registeredTiltInfo">
        /// The <see cref="TiltInfo"/> that contains information on a tilt that needs to be created.
        /// </param>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        protected abstract bool SendRequestTilt(TiltInfo registeredTiltInfo);

        /// <summary>
        /// Protected version of clear tilt.  
        /// It clears a tilt with the option to not notify the foundation.
        /// </summary>
        /// <remarks>
        /// This operation requires an open transaction.
        /// This is protected because the option to not tell the foundation should not be public.  
        /// </remarks>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <param name="notifyFoundation">
        /// True if the foundation needs to be notified of the clear tilt.  
        /// </param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        protected bool ClearTilt(string key, bool notifyFoundation)
        {
            if(!TiltPresent(key))
            {
                throw new TiltLogicException(key, "Tilt cannot be cleared, it is not currently posted.");
            }

            var registrationInfo = RemoveTilt(key, notifyFoundation);

            return HandleRegistration(registrationInfo);
        }
        
        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Calls the appropriate handler for the supplied <see cref="TiltInfoTuple"/>.
        /// </summary>
        /// <param name="registrationInfo">
        /// The tilts that may need to be registered/unregistered.
        /// </param>
        /// <returns>Returns true if the tilt was successfully handled.</returns>
        private bool HandleRegistration(TiltInfoTuple registrationInfo)
        {
            var result = true;
            if(registrationInfo.Unregister != null)
            {
                result = SendClearTilt(registrationInfo.Unregister.Key);
            }
            if(registrationInfo.Register != null)
            {
                result = result && SendRequestTilt(registrationInfo.Register);
            }

            return result;
        }

        #endregion Private Methods

        #region TiltList Manipulation

        /// <summary>
        /// Adds a <paramref name="tiltInfo"/> to the tilt list at the correct point.
        /// </summary>
        /// <param name="tiltInfo">
        /// The tilt information to add to the list.
        /// </param>
        /// <returns>
        /// A tuple containing the information about the tilts that may need to be registered/unregistered because
        /// of this action.
        /// </returns>
        /// <DevDoc>
        /// Marked as internal for testing only.
        /// </DevDoc>
        internal TiltInfoTuple AddTilt(TiltInfo tiltInfo)
        {
            var registrationData = new TiltInfoTuple();
            var insertIndex = TiltInfoList.FindIndex(tilt => tilt.Tilt.Priority < tiltInfo.Tilt.Priority);

            // The tilt to be inserted has lower or equal priority compared to the others in the list.
            // Higher priority tilts are inserted at the front of the list.
            if(insertIndex == -1)
            {
                insertIndex = TiltInfoList.Count;
            }

            // Check if this tilt needs to be inserted into the tilts registered with the foundation.
            if(insertIndex < RegisteredFoundationTiltTotalCount)
            {
                //Record that it will need to be registered.
                registrationData.Register = tiltInfo;

                //If the foundation registered tilts are already full.
                if(TiltInfoList.Count >= RegisteredFoundationTiltTotalCount)
                {
                    //Record that this tilt will need to be unregistered.
                    registrationData.Unregister = TiltInfoList[RegisteredFoundationTiltTotalCount - 1];
                }
            }

            TiltInfoList.Insert(insertIndex, tiltInfo);

            return registrationData;
        }

        /// <summary>
        /// Removes a <see cref="TiltInfo"/> object from the tilt list.
        /// </summary>
        /// <param name="key">
        /// The key for the <see cref="TiltInfo"/> to be removed.
        /// </param>
        /// <param name="notifyFoundation">
        /// True if the foundation needs to be notified of the clear tilt.  False will just remove it from 
        /// the list and still register any queued tilts.  Defaults to true;
        /// </param>
        /// <returns>
        /// A tuple containing the information about the tilts that may need to be registered/unregistered because
        /// of this action.
        /// </returns>
        /// <DevDoc>
        /// Marked as internal for testing only.
        /// </DevDoc>
        internal TiltInfoTuple RemoveTilt(string key, bool notifyFoundation = true)
        {
            var registrationData = new TiltInfoTuple();
            var removeIndex = TiltInfoList.FindIndex(t => t.Key == key);

            //Check if this tilt needs to be removed from the tilts registered with the foundation.
            if(removeIndex < RegisteredFoundationTiltTotalCount)
            {
                if(notifyFoundation)
                {
                    //Record that it will need to be unregistered.
                    registrationData.Unregister = TiltInfoList[removeIndex];
                }

                //Check if there is a tilt in the list that has not been registered with the foundation yet.
                if(TiltInfoList.Count > RegisteredFoundationTiltTotalCount)
                {
                    //Record that it will need to be registered.
                    registrationData.Register = TiltInfoList[RegisteredFoundationTiltTotalCount];
                }
            }

            TiltInfoList.RemoveAt(removeIndex);

            return registrationData;
        }

        #endregion TiltList Manipulation
    }
}
