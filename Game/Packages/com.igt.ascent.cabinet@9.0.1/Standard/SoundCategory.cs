//-----------------------------------------------------------------------
// <copyright file = "SoundCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CSI.Schemas;
    using CsiTransport;
    using Foundation.Transport;
    using CsiInternal = CSI.Schemas.Internal;

    /// <summary>
    /// Category for managing sound.
    /// </summary>
    internal class SoundCategory : CategoryBase<CsiInternal.CsiSound>, ISound
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="SoundCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region CategoryBase<CSIInternal.CsiSound> Implementation

        /// <inheritdoc/>
        public override CsiInternal.Category Category => CsiInternal.Category.CsiSound;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentN01SeriesSb))
                {
                    return 9;
                }

                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentN01Series))
                {
                    return 8;
                }

                return 4;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var soundMessage = message as CsiInternal.CsiSound;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(soundMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiInternal.CsiSound),
                                                                message.GetType()));
            }

            if(soundMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Sound message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(soundMessage.Item.GetType()))
            {
                eventHandlers[soundMessage.Item.GetType()](soundMessage.Item);
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + soundMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var soundMessage = message as CsiInternal.CsiSound;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(soundMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiInternal.CsiSound),
                                                                message.GetType()));
            }

            if(soundMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Sound message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + soundMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiSound> Implementation

        #region Events

        /// <summary>
        /// Event which is fired upon a volume change notification by the CSI manager.
        /// </summary>
        public event EventHandler<SoundVolumeChangedEventArgs> SoundVolumeChangedEvent;

        /// <summary>
        /// Event which is fired upon a Mute All status change notification by the CSI manager.
        /// </summary>
        public event EventHandler<SoundVolumeMuteAllStatusChangedEventArgs> SoundVolumeMuteAllStatusChangedEvent;

        /// <summary>
        /// Event which is fired upon a Mute All status change notification by the CSI manager.
        /// </summary>
        public event EventHandler<SoundVolumePlayerSelectableStatusChangedEventArgs> SoundVolumePlayerSelectableStatusChangedEvent;

        /// <summary>
        /// Event which is fired upon a player volume level change notification by the CSI manager.
        /// </summary>
        public event EventHandler<SoundVolumePlayerLevelChangedEventArgs> SoundVolumePlayerLevelChangedEvent;

        /// <summary>
        /// Event which is fired when CSI Manager notifies the client that the state of the headphone jack has changed.
        /// </summary>
        public event EventHandler<HeadphoneJackChangedEventArgs> HeadphoneJackChangedEvent;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// It is currently not used, but maybe needed in the future.
        /// </param>
        // ReSharper disable once UnusedParameter.Local
        public SoundCategory(FoundationTarget target)
        {
            foundationTarget = target;

            eventHandlers[typeof(CsiInternal.VolumeChangedEvent)] =
                message => HandleSoundVolumeChangedEvent(message as CsiInternal.VolumeChangedEvent);

            eventHandlers[typeof(CsiInternal.VolumeMuteAllStatusChangedEvent)] =
                message => HandleSoundVolumeMuteAllStatusChangedEvent(message as CsiInternal.VolumeMuteAllStatusChangedEvent);

            eventHandlers[typeof(CsiInternal.VolumePlayerSelectableChangedEvent)] =
                message => HandleSoundVolumePlayerSelectableStatusChangedEvent(message as CsiInternal.VolumePlayerSelectableChangedEvent);

            eventHandlers[typeof(CsiInternal.VolumePlayerLevelChangedEvent)] =
                message => HandleVolumePlayerLevelChangedEvent(message as CsiInternal.VolumePlayerLevelChangedEvent);

            eventHandlers[typeof(CsiInternal.HeadphoneJackChangedEvent)] =
                message => HandleHeadphoneJackChangedEvent(message as CsiInternal.HeadphoneJackChangedEvent);
        }

        #endregion Constructors

        #region ISound Implementation

        #region Sound Chair Methods

        /// <summary>
        /// Enable/disable the sound chair power.
        /// </summary>
        /// <param name="enable">Flag indicating if the chair power should be enabled.</param>
        /// <returns>True if the change was applied.</returns>
        public bool EnableSoundChairPower(bool enable)
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.ChairEnablePowerRequest { EnablePower = enable } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.ChairEnablePowerResponse>();
            CheckResponse(response.SoundResponse);

            return response.Success;
        }

        /// <summary>
        /// Check if the sound chair is powered.
        /// </summary>
        /// <returns>True if the chair is powered.</returns>
        public bool IsSoundChairPowered()
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.ChairPoweredRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.ChairPoweredResponse>();
            CheckResponse(response.SoundResponse);

            return response.Powered;
        }

        #endregion Sound Chair Methods

        #region Volume Methods

        /// <summary>
        /// Handle a sound volume change event.
        /// </summary>
        private void HandleSoundVolumeChangedEvent(CsiInternal.VolumeChangedEvent soundVolumeChangedEvent)
        {
            SoundVolumeChangedEvent?.Invoke(
                this, 
                new SoundVolumeChangedEventArgs(soundVolumeChangedEvent.GroupVolume));
        }

        /// <summary>
        /// Handle a Mute All status change event.
        /// </summary>
        /// <param name="muteAllStatusChangedEvent">the event.</param>
        private void HandleSoundVolumeMuteAllStatusChangedEvent(CsiInternal.VolumeMuteAllStatusChangedEvent muteAllStatusChangedEvent)
        {
            SoundVolumeMuteAllStatusChangedEvent?.Invoke(
                this,
                new SoundVolumeMuteAllStatusChangedEventArgs(muteAllStatusChangedEvent.MuteAll));
        }

        /// <summary>
        /// Handle a sound volume player selectable status change event.
        /// </summary>
        /// <param name="soundVolumePlayerSelectableStatusChangedEvent">the event.</param>
        private void HandleSoundVolumePlayerSelectableStatusChangedEvent(CsiInternal.VolumePlayerSelectableChangedEvent soundVolumePlayerSelectableStatusChangedEvent)
        {
            SoundVolumePlayerSelectableStatusChangedEvent?.Invoke(
                this,
                new SoundVolumePlayerSelectableStatusChangedEventArgs(soundVolumePlayerSelectableStatusChangedEvent.VolumePlayerSelectable,
                                                                      soundVolumePlayerSelectableStatusChangedEvent.PlayerMuteSelectable));
        }

        /// <summary>
        /// Handle a volume player level changed event.
        /// </summary>
        /// <param name="volumePlayerLevelChangedEvent">the event.</param>
        private void HandleVolumePlayerLevelChangedEvent(CsiInternal.VolumePlayerLevelChangedEvent volumePlayerLevelChangedEvent)
        {
            SoundVolumePlayerLevelChangedEvent?.Invoke(
                this,
                new SoundVolumePlayerLevelChangedEventArgs(volumePlayerLevelChangedEvent.VolumePlayerLevel));
        }

        /// <summary>
        /// Get the volume of the specified sound group.
        /// </summary>
        /// <param name="soundGroup">The sound group to get the volume for.</param>
        /// <returns>
        /// The level of sound attenuation between 0 and 10000. 0 indicates no attenuation and 10000 represent maximum attenuation (mute).
        /// </returns>
        public uint GetVolume(GroupName soundGroup)
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.VolumeRequest { SoundGroup = soundGroup } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.VolumeResponse>();
            CheckResponse(response.SoundResponse);

            return response.GroupVolume == null ? 0 : response.GroupVolume.VolumeLevel;
        }

        /// <summary>
        /// Get the volume of each sound group.
        /// </summary>
        /// <returns>
        /// The level of sound attenuation for all sound groups.
        /// </returns>
        public IEnumerable<GroupVolumeSetting> GetVolumeAll()
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.VolumeAllGroupsRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.VolumeAllGroupsResponse>();
            CheckResponse(response.SoundResponse);

            return response.Volumes.GroupVolume;
        }

        /// <summary>
        /// Get the status of Mute All.
        /// </summary>
        /// <returns>A boolean indicating the status of Mute All.</returns>
        public bool IsMuteAll()
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.VolumeMuteAllStatusRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.VolumeMuteAllStatusResponse>();
            CheckResponse(response.SoundResponse);

            return response.MuteAll;
        }

        /// <inheritdoc/>
        public VolumeSelectableInfo GetVolumePlayerSelectableInfo()
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.VolumePlayerSelectableRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.VolumePlayerSelectableResponse>();
            CheckResponse(response.SoundResponse);

            return new VolumeSelectableInfo(response.PlayerSelectable, response.PlayerMuteSelectable);
        }

        /// <inheritdoc/>
        public PlayerVolumeSettings GetPlayerVolumeSettings()
        {
            var request = new CsiInternal.CsiSound { Item = new CsiInternal.VolumePlayerLevelRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.VolumePlayerLevelResponse>();
            CheckResponse(response.SoundResponse);

            float? defaultVolumePlayerLevel = null;
            if(response.DefaultVolumePlayerLevelSpecified)
            {
                defaultVolumePlayerLevel = response.DefaultVolumePlayerLevel;
            }

            return new PlayerVolumeSettings(response.VolumePlayerLevel,
                                            response.PlayerMuteSelected,
                                            defaultVolumePlayerLevel);
        }

        /// <inheritdoc/>
        public bool SetPlayerVolumeInfo(PlayerVolumeInfo volumeInfo)
        {
            var level = volumeInfo.VolumePlayerLevel;

            // restrict the set level between 0 and 1
            level = level < 0 ? 0 : level;
            level = level > 1 ? 1 : level;

            var request = new CsiInternal.CsiSound
                          {
                              Item = new CsiInternal.SetVolumePlayerLevelRequest
                                     {
                                         VolumePlayerLevel = level,
                                         PlayerMuteSelected = volumeInfo.PlayerMuteSelected,
                                         PlayerMuteSelectedSpecified = true
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.SetVolumePlayerLevelResponse>();
            CheckResponse(response.SoundResponse);

            return (response.SoundResponse.ErrorCode == CsiInternal.SoundErrorCode.NONE);
        }

        #endregion Volume Methods

        #region Volume Mixer Methods

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">
        /// Thrown if the foundation target does not match the minimum CsiSoundV1 version to invoke SetMuteStates.
        /// </exception>
        IEnumerable<PriorityMuteState> ISound.SetMuteStates(IEnumerable<PriorityMuteState> muteStates)
        {
            var currentSchemaVersion = new Version(VersionMajor, VersionMinor);
            var minimumSchemaVersionRequiredForSetMuteStates = new Version(1, 7);
            if(currentSchemaVersion < minimumSchemaVersionRequiredForSetMuteStates)
            {
                throw new NotSupportedException("SetMuteStates requires targeting CsiSoundV1 schema version 1.7 or newer.");
            }

            var muteClientsRequest = new CsiInternal.MuteClientsRequest();
            foreach(var coreMuteState in muteStates)
            {
                muteClientsRequest.PriorityMuteState.Add(ConvertPriorityMuteState(coreMuteState));
            }

            var request = new CsiInternal.CsiSound { Item = muteClientsRequest };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.MuteClientsResponse>();
            CheckResponse(response.SoundResponse);

            var coreMuteStates = new List<PriorityMuteState>();
            foreach(var csiMuteState in response.PriorityMuteState)
            {
                coreMuteStates.Add(ConvertPriorityMuteState(csiMuteState));
            }

            return coreMuteStates;
        }

        #endregion Volume Mixer Methods

        #region Headphone Jack

        /// <summary>
        /// Handle a headphone jack changed event.
        /// </summary>
        /// <param name="headphoneJackChangedEvent">the event.</param>
        private void HandleHeadphoneJackChangedEvent(CsiInternal.HeadphoneJackChangedEvent headphoneJackChangedEvent)
        {
            if(HeadphoneJackChangedEvent != null)
            {
                var state = (headphoneJackChangedEvent.HeadphoneJackState == CSI.Schemas.Internal.HeadphoneJackState.Inserted)
                                ? HeadphoneJackState.Inserted
                                : HeadphoneJackState.Removed;

                HeadphoneJackChangedEvent(this, new HeadphoneJackChangedEventArgs(state));
            }
        }

        #endregion Headphone Jack

        #region Audio Endpoints

        /// <summary>
        /// Returns a list of audio endpoints present on the EGM.
        /// </summary>
        /// <returns>
        /// A list of audio endpoints present on the EGM.
        /// </returns>
        public IEnumerable<string> GetAudioEndpoints()
        {
            var currentSchemaVersion = new Version(VersionMajor, VersionMinor);
            var minimumSchemaVersionRequiredForAudioEndpoints = new Version(1, 9);
            if(currentSchemaVersion < minimumSchemaVersionRequiredForAudioEndpoints)
            {
                throw new NotSupportedException("GetAudioEndpoints requires targeting CsiSoundV1 schema version 1.9 or newer.");
            }

            var getAudioEndpointsRequest = new CsiInternal.GetAudioEndpointsRequest
                                           {
                                               UnusedSpecified = false
                                           };

            var request = new CsiInternal.CsiSound { Item = getAudioEndpointsRequest };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.GetAudioEndpointsResponse>();
            CheckResponse(response.SoundResponse);

            return response.Endpoints.Endpoint.ToArray();
        }

        /// <summary>
        /// Sets the default audio endpoint for the EGM.
        /// </summary>
        /// <param name="endpoint">
        /// Name of the audio endpoint to use as the default audio endpoint.
        /// </param>
        /// <returns>
        /// True if the endpoint specified is now the default audio endpoint.
        /// </returns>
        public bool SetDefaultAudioEndpoint(string endpoint)
        {
            if(string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("endpoint null or empty");
            }

            var currentSchemaVersion = new Version(VersionMajor, VersionMinor);
            var minimumSchemaVersionRequiredForAudioEndpoints = new Version(1, 9);
            if(currentSchemaVersion < minimumSchemaVersionRequiredForAudioEndpoints)
            {
                throw new NotSupportedException("SetDefaultAudioEndpoint requires targeting CsiSoundV1 schema version 1.9 or newer.");
            }

            var setDefaultAudioEndpointRequest = new CsiInternal.SetDefaultAudioEndpointRequest
                                                 {
                                                     Endpoint = endpoint
                                                 };

            var request = new CsiInternal.CsiSound { Item = setDefaultAudioEndpointRequest };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetSoundResponse<CsiInternal.SetDefaultAudioEndpointResponse>();
            CheckResponse(response.SoundResponse);

            return response.SoundResponse.ErrorCode == CsiInternal.SoundErrorCode.NONE;
        }

        #endregion Audio Endpoints

        #endregion ISound Implementation

        #region Private Methods

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetSoundResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="SoundCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(CsiInternal.SoundResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case CsiInternal.SoundErrorCode.NONE:
                case CsiInternal.SoundErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case CsiInternal.SoundErrorCode.CHAIR_NOT_INSTALLED:
                case CsiInternal.SoundErrorCode.CHAIR_COMMUNICATION_FAILURE:
                case CsiInternal.SoundErrorCode.CSI_SCHEMA_VERSION_MISMATCH:
                case CsiInternal.SoundErrorCode.WASAPI_FAILURE:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new SoundCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        /// <summary>
        /// Helper function to convert a IGT.Game.Core.Communication.Cabinet.PriorityMuteState instance
        /// to its respective CSI.Schemas.Internal.PriorityMuteState instance.
        /// </summary>
        /// <param name="coreMuteState">
        /// The IGT.Game.Core.Communication.Cabinet.PriorityMuteState instance to convert.
        /// </param>
        /// <returns>
        /// The converted CSI.Schemas.Internal.PriorityMuteState instance.
        /// </returns>
        private static CsiInternal.PriorityMuteState ConvertPriorityMuteState(PriorityMuteState coreMuteState)
        {
            var csiMuteState = new CsiInternal.PriorityMuteState
                               {
                                   PriorityType = coreMuteState.PriorityType,
                                   Muted = coreMuteState.Muted
                               };

            return csiMuteState;
        }

        /// <summary>
        /// Helper function to convert a CSI.Schemas.Internal.PriorityMuteState instance
        /// to its respective IGT.Game.Core.Communication.Cabinet.PriorityMuteState instance.
        /// </summary>
        /// <param name="csiMuteState">
        /// The CSI.Schemas.Internal.PriorityMuteState instance to convert.
        /// </param>
        /// <returns>
        /// The converted IGT.Game.Core.Communication.Cabinet.PriorityMuteState instance.
        /// </returns>
        private static PriorityMuteState ConvertPriorityMuteState(CsiInternal.PriorityMuteState csiMuteState)
        {
            return new PriorityMuteState(csiMuteState.PriorityType, csiMuteState.Muted);
        }

        #endregion Private Methods
    }
}
