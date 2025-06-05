//-----------------------------------------------------------------------
// <copyright file = "TouchScreenService.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Cabinet
{
    using CabinetServices;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// The cabinet service that provides access to touch screen functionalities.
    /// </summary>
    /// <devdoc>
    /// Apparently TouchScreen is a device that can be acquired and released, but the acquiring and releasing
    /// functionality is not part of the SDK support at the moment.  In the future if needs come up, we can
    /// add the functionality here by probably converting this service to a DeviceService.
    ///
    /// NOTE: Most of CsiTouchScreen category messages are used by attendant/operator menu (CSIClient.dll in Foundation).
    /// The few APIs exposed here are used by a handful of games and the sports betting app.
    /// </devdoc>
    public class TouchScreenService : CabinetServiceBase, ITouchScreenService
    {
        #region Overrides of CabinetServiceBase

        /// <inheritdoc />
        protected override void OnAsyncConnect()
        {
            base.OnAsyncConnect();

            // Reset the software debounce interval for the Dpp's digitizer when a new theme is loaded.
            ResetDigitizerDebounceInterval(DigitizerRole.ButtonPanel);
        }

        #endregion

        #region Implementation of ITouchScreenService

        /// <inheritdoc />
        public void SetDigitizerToMinimumDebounce(DigitizerRole role)
        {
            VerifyCabinetIsConnected();

            var touchScreenInterface = CabinetLib.GetInterface<ITouchScreen>();
            if(touchScreenInterface != null)
            {
                // If the digitizer or role does not allow setting the interval, then that is not critical.
                // It could be because the functionality is no longer supported, the device doesn't support the
                // adjustment, or that it cannot be adjusted for reliability or regulatory reason.

                // We do not return a value to the client, because the client could use this value in a way that is
                // not forward compatible with the foundation. In this way they call the method to indicate if they
                // need better digitizer performance, but they will continue to work even if that method is no
                // longer supported by the foundation.
                try
                {
                    touchScreenInterface.SetDigitizerToMinimumDebounce(role);
                }
                catch(TouchScreenCategoryException)
                {
                }
            }
        }

        /// <inheritdoc />
        public void ResetDigitizerDebounceInterval(DigitizerRole role)
        {
            VerifyCabinetIsConnected();

            var touchScreenInterface = CabinetLib.GetInterface<ITouchScreen>();
            if(touchScreenInterface != null)
            {
                // If the digitizer or role does not allow setting the interval, then that is not critical.
                // It could be because the functionality is no longer supported, the device doesn't support the
                // adjustment, or that it cannot be adjusted for reliability or regulatory reason.

                // We do not return a value to the client, because the client could use this value in a way that is
                // not forward compatible with the foundation. In this way they call the method to indicate if they
                // need better digitizer performance, but they will continue to work even if that method is no
                // longer supported by the foundation.
                try
                {
                    touchScreenInterface.ResetDigitizerDebounceInterval(role);
                }
                catch(TouchScreenCategoryException)
                {
                }
            }
        }

        /// <inheritdoc />
        public void SetDigitizerExclusiveMode(DigitizerRole role, bool exclusive)
        {
            VerifyCabinetIsConnected();

            var touchScreenInterface = CabinetLib.GetInterface<ITouchScreen>();
            if (touchScreenInterface != null)
            {
                try
                {
                    touchScreenInterface.SetDigitizerExclusiveMode(role, exclusive);
                }
                catch(TouchScreenCategoryException)
                {
                }
            }
        }

        #endregion
    }
}
