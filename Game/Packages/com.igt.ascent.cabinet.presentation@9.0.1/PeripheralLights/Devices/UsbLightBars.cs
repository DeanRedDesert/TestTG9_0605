//-----------------------------------------------------------------------
// <copyright file = "UsbLightBars.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas.Internal;

    /// <summary>
    /// USB light bars class.
    /// </summary>
    public class UsbLightBars : UsbPeripheralLight
    {
        #region Private members

        /// <summary>
        /// The light controller used to store and parse data from the .light files.
        /// </summary>
        private readonly LightControllerImporter lightControllerImporter;

        /// <summary>
        /// The number of the group to turn on or off in the light bars hardware.
        /// </summary>
        /// <remarks>There is only one group for the light bars hardware.</remarks>
        private const byte GroupNumber = 0;

        /// <summary>
        /// The first light to control.
        /// </summary>
        /// <remarks>The starting light to begin the sequence is always the first light in the hardware.</remarks>
        private const byte StartingLight = 0;

        #endregion

        #region Public members

        /// <summary>
        /// The path where the game executable is installed.
        /// </summary>
        public string GameMountPoint { get; internal set; }

        #endregion

        /// <summary>
        /// Construct an USB light bars instance.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbLightBars(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {
            lightControllerImporter = new LightControllerImporter();
        }

        /// <summary>
        /// Get the light sequence data from the <see cref="LightControllerImporter"/>.
        /// </summary>
        /// <param name="fileName">The .light file containing the animation sequences 
        /// including the full directory path.</param>
        /// <returns>
        /// Return the <see cref="MainLightControllerData"/> that contains the animation data.
        /// </returns>
        public MainLightControllerData GetLightSequencesData(string fileName)
        {
            lightControllerImporter.ParseFile(fileName);

            return lightControllerImporter.MainLightControllerData;
        }

        /// <summary>
        /// Run a specified light sequence on the light bars for a specified duration.
        /// </summary>
        /// <param name="lightColors">The light sequence data <see cref="AnimationData"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lightColors"/> is null.
        /// </exception>
        /// <exception cref="LightCategoryException">
        /// Thrown if an error occurs when setting the color.
        /// </exception>
        public void RunLightSequence(IEnumerable<Rgb6> lightColors)
        {
            if(lightColors == null)
            {
                throw new ArgumentNullException(nameof(lightColors));
            }

            try
            {
                if(CanLightCommandBeSent)
                {
                    LightInterface.BitwiseLightControl(FeatureName, GroupNumber, StartingLight, lightColors);
                }
            }
            catch(LightCategoryException ex)
            {
                if (ShouldLightCategoryErrorBeReported(ex))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Suppress several light category exceptions due to possible race conditions when the device
        /// loses and regains its connection and acquisition.
        /// </summary>
        /// <param name="exception">
        /// Exception thrown when an error status is received from the light category.
        /// </param>
        protected override bool ShouldLightCategoryErrorBeReported(Exception exception)
        {
            var lightCategoryException = exception as LightCategoryException;

            return base.ShouldLightCategoryErrorBeReported(exception) &&
                   lightCategoryException != null &&
                   lightCategoryException.ErrorCode != LightErrorCode.INVALID_FEATURE_ID.ToString();
        }
    }
}
