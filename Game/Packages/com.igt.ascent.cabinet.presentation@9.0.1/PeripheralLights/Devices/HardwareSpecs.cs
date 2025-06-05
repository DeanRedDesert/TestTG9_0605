// -----------------------------------------------------------------------
// <copyright file = "HardwareSpecs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;

    #region Class HardwareSpec

    /// <summary>
    /// This delegate defines the method signature for creating a USB light device.
    /// </summary>
    /// <param name="name">The feature name of the light device to create.</param>
    /// <param name="description">The feature description of the light device to create.</param>
    /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
    /// <returns>The USB light device created.</returns>
    internal delegate UsbLightBase LightCreator(string name,
                                                LightFeatureDescription description,
                                                object lightInterface);

    /// <summary>
    /// This class defines the specification of a USB light.
    /// </summary>
    internal class HardwareSpec
    {
        /// <summary>
        /// Gets or sets the hardware type of the light device.
        /// </summary>
        public Hardware HardwareType { get; set; }

        /// <summary>
        /// Gets or sets the feature name of the light device.
        /// </summary>
        public string FeatureName { get; set; }

        /// <summary>
        /// Gets or sets the coupled feature name of the light device.
        /// A coupled light device is a light device that is able to be controlled by multiple interfaces.
        /// </summary>
        public string CoupledFeatureName { get; set; }

        /// <summary>
        /// Gets or sets flag indicating whether the light device is a streaming light or not.
        /// </summary>
        public bool IsStreamingLight { get; set; }

        /// <summary>
        /// Gets or sets the creator function for the light device.
        /// </summary>
        public LightCreator CreateDevice { get; set; }
    }

    #endregion

    /// <summary>
    /// This class defines the specifications of all USB light devices supported by SDK.
    /// </summary>
    public static class HardwareSpecs
    {
        /// <summary>
        /// Gets the feature names all known peripheral light devices.
        /// </summary>
        /// <remarks>
        /// This method comes in handy for tools like Aurora and Light Conductor.
        /// </remarks>
        /// <returns>
        /// The features names of all known peripheral light devices,
        /// keyed by the hardware type.
        /// </returns>
        public static IDictionary<Hardware, string> GetPeripheralLightFeatureNames()
        {
            return GetPeripheralLightSpecs().ToDictionary(spec => spec.HardwareType, spec => spec.FeatureName);
        }

        /// <summary>
        /// Gets the feature names all known streaming light devices.
        /// </summary>
        /// <remarks>
        /// This method comes in handy for tools like Aurora and Light Conductor.
        /// </remarks>
        /// <returns>
        /// The features names of all known streaming light devices,
        /// keyed by the hardware type.
        /// </returns>
        public static IDictionary<Hardware, string> GetStreamingLightFeatureNames()
        {
            return GetStreamingLightSpecs().ToDictionary(spec => spec.HardwareType, spec => spec.FeatureName);
        }

        /// <summary>
        /// Gets the specifications of all peripheral light devices.
        /// </summary>
        /// <returns>The list of specifications of all peripheral light devices.</returns>
        internal static IList<HardwareSpec> GetPeripheralLightSpecs()
        {
            return HardwareSpecList.Where(spec => !spec.IsStreamingLight).ToList();
        }

        /// <summary>
        /// Gets the specifications of all streaming light devices.
        /// </summary>
        /// <returns>The list of specifications of all streaming light devices.</returns>
        internal static IList<HardwareSpec> GetStreamingLightSpecs()
        {
            return HardwareSpecList.Where(spec => spec.IsStreamingLight).ToList();
        }

        /// <summary>
        /// The list of specs of all light devices that we know of.
        /// </summary>
        /// <remarks>
        /// Leading spaces in some of the feature names is intentional. Case is also important.
        /// </remarks>
        private static readonly List<HardwareSpec> HardwareSpecList =
            new List<HardwareSpec>
            {
                new HardwareSpec
                {
                    HardwareType = Hardware.SuzoHapp,
                    FeatureName = "Topper Light Ring",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbTopperLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.ButtonEdge,
                    FeatureName = " Dynamic Button Light Bezel",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbButtonEdgeLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TicketPrinterHalo,
                    FeatureName = " Ticket Printer Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SpeakerHalo,
                    FeatureName = " Speaker Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.BillValidatorHalo,
                    FeatureName = " Bill Validator Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Sidebar,
                    FeatureName = " sAVP Top Box Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Crown,
                    FeatureName = " Crown Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.LightBars,
                    FeatureName = "Light Bars",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbLightBars(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Facade,
                    FeatureName = " Door Lights",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbFacadeLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.LegacyReelBackLights,
                    FeatureName = "Reel Backlights",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbReelBackLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.LegacyReelFrontLights,
                    FeatureName = "Reel Front Lights",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbReelFrontLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Gills,
                    FeatureName = " Gill Lighting",
                    CreateDevice = CreateUsbCrystalCoreLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DppLightRing,
                    FeatureName = " DPP Light Ring",
                    CreateDevice = CreateUsbCrystalCoreLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CatalinaReelBackLights,
                    FeatureName = " Runtime Reel Backlights",
                    IsStreamingLight = true,
                    CreateDevice = CheckForSymbolHighlightsAndCreateStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CatalinaSideLights,
                    FeatureName = " Runtime Reel Dividers",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CatalinaFrameLights,
                    FeatureName = " Runtime Reel Framelights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CatalinaReelAccentLights,
                    FeatureName = " Runtime Reel Highlights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CatalinaTrimLights,
                    FeatureName = " Runtime Trim Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.HandleLights,
                    FeatureName = " Handle Lights",
                    CreateDevice = (name, description, lightInterface) =>
                        new UsbHandleLight(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingHandleLights,
                    FeatureName = " Runtime Handle Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Skylight,
                    FeatureName = " Runtime Skylite",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TailLightTopper,
                    FeatureName = " Runtime Tail Light Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.LandscapeTopBox,
                    FeatureName = " Landscape Top Box Lights",
                    CreateDevice = CreateUsbHaloLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingGills,
                    FeatureName = " Runtime Gill Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingCrystalCoreEdge42,
                    FeatureName = " Runtime 42in Edge Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingCrystalCoreEdge30,
                    FeatureName = " Runtime 30in Edge Lighting",
                    CoupledFeatureName = " Runtime Crystal Edge Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingCrystalCoreEdge27,
                    FeatureName = " Runtime Crystal Edge Lights",
                    CoupledFeatureName = " Runtime 30in Edge Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.GenericBacklitTopper,
                    FeatureName = " Runtime Generic Backlit Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.VideoTopper,
                    FeatureName = " Runtime Video Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelEdgeLighting,
                    FeatureName = " Runtime WOF Edge Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelPointers,
                    FeatureName = " Runtime WOF Pointers",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelFacade,
                    FeatureName = " Runtime WOF Facade",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelHub,
                    FeatureName = " Runtime WOF Hub",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelLightRing,
                    FeatureName = " Runtime WOF Light Ring",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Titan32TopBoxMonitorBezel,
                    FeatureName = " Runtime 32 Landscape Topbox Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Titan27TopBoxDualMonitorBezel,
                    FeatureName = " Runtime Monitor Bezel-Base LR TB LRT",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TitanMainMonitorBezel,
                    FeatureName = " Runtime Monitor Bezel-Base LRT",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.AustraliaCrystalCoreTrim,
                    FeatureName = " Runtime Aus Trim Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.AlcoveFlatpack,
                    FeatureName = " Runtime Flatpack Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.AlcoveEndCap,
                    FeatureName = " Runtime End Cap Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.AlcoveCarousel,
                    FeatureName = " Runtime Carousel Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Axxis23_23,
                    FeatureName = " Runtime Axxis 23in Edge Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.AxxisVideoTopper,
                    FeatureName = " Runtime Axxis Video Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.QuasarMonitorLights,
                    FeatureName = " Runtime Quasar Monitor Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.QuasarBillValidatorLights,
                    FeatureName = " Runtime Quasar Bill Validator Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.QuasarPrinterLights,
                    FeatureName = " Runtime Quasar Printer Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.QuasarCoinTrayLights,
                    FeatureName = " Runtime Quasar Coin Tray Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.QuasarWallLights,
                    FeatureName = " Runtime Quasar Wall Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.StreamingDppLightRing,
                    FeatureName = " Runtime DPP Light Ring",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TitanSpacer,
                    FeatureName = " Runtime Crystal Slant 32in Spacer",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperTopperLights,
                    FeatureName = " Runtime CDPS Topper Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperTopDollarFrameLights,
                    FeatureName = " Runtime CDPS Frame Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperTopDollarDollarLights,
                    FeatureName = " Runtime CDPS Dollar Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperBottomDoorLights,
                    FeatureName = " Runtime Door Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperGillExtensionLights,
                    FeatureName = " Runtime CDPS Gill Extension Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperMarilynEdgeLights,
                    FeatureName = " Runtime CDPS Edge Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperMarilynInnerMaskLights,
                    FeatureName = " Runtime CDPS Inner Mask Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperMarilynDiamondLights,
                    FeatureName = " Runtime CDPS Marilyn Diamonds Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.GamesmanButtonLightRing,
                    FeatureName = " Runtime Gamesman Button Light Ring",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperMegabucksFrameLights,
                    FeatureName = " Runtime CDPS MB Frame Lighting",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalCurveMonitorLights,
                    FeatureName = " Runtime Crystal Curve Monitor",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusVideo4DMonitorBezelLights,
                    FeatureName = " Runtime 4D Monitor Bezel",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusVideo4DSideTrimLights,
                    FeatureName = " Runtime 4D Side Trim Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusVideo4DLowerTrimLights,
                    FeatureName = " Runtime 4D Lower Trim Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusVideo4DStandLights,
                    FeatureName = " Runtime 4D Stand Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SmallNestedWheelLights,
                    FeatureName = " Small Wheel Lights",
                    CreateDevice = CreateUsbNestedWheelLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MediumNestedWheelLights,
                    FeatureName = " Medium Wheel Lights",
                    CreateDevice = CreateUsbNestedWheelLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.LargeNestedWheelLights,
                    FeatureName = " Large Wheel Lights",
                    CreateDevice = CreateUsbNestedWheelLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TwilightZone3DMonitorBezelLights,
                    FeatureName = " Monitor Bezel Lights",
                    CreateDevice = (name, description, lightInterface) =>
                        new TwilightZone3DMonitorBezel(name, description, lightInterface as IPeripheralLights)
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalCurveGills,
                    FeatureName = " Runtime Crystal Curve Gills",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DualSmashDoubleUpHalo,
                    FeatureName = " Runtime Double Up Halo",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DualSmashRepeatBetHalo,
                    FeatureName = " Runtime Repeat Bet Halo",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MegaTowerHandleAccentLights,
                    FeatureName = " Runtime Handle Accent Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MegaTowerWheelBezelLights,
                    FeatureName = " Runtime MT Wheel Bezel Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MegaTowerWheelHubLights,
                    FeatureName = " Runtime MT Wheel Hub Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MegaTowerWheelPointerLights,
                    FeatureName = " Runtime MT Wheel Ptr Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MarqueeLights,
                    FeatureName = " Runtime Marquee Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MarqueeBacklight,
                    FeatureName = " Runtime Marquee Backlight",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.Spectrum,
                    FeatureName = " Runtime Spectrum Display",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PokerWheelBezel,
                    FeatureName = " Runtime Poker Bezel",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PokerWheelLeft,
                    FeatureName = " Runtime Wheel Lights Left",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PokerWheelCenter,
                    FeatureName = " Runtime Wheel Lights Center",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PokerWheelRight,
                    FeatureName = " Runtime Wheel Lights Right",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.TopperBacklight,
                    FeatureName = " Topper Backlight",
                    CreateDevice = CreateUsbLegacyBacklight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WheelBacklight,
                    FeatureName = " Wheel Backlight",
                    CreateDevice = CreateUsbLegacyBacklight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig0,
                    FeatureName = " Runtime ETG Lights Config 0",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig1,
                    FeatureName = " Runtime ETG Lights Config 1",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig2,
                    FeatureName = " Runtime ETG Lights Config 2",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig3,
                    FeatureName = " Runtime ETG Lights Config 3",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig4,
                    FeatureName = " Runtime ETG Lights Config 4",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig5,
                    FeatureName = " Runtime ETG Lights Config 5",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig6,
                    FeatureName = " Runtime ETG Lights Config 6",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.EtgLightsConfig7,
                    FeatureName = " Runtime ETG Lights Config 7",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperTotAccentLights,
                    FeatureName = " Runtime CDPS ToT Frame Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.CrystalDualPlusStepperGenericLights,
                    FeatureName = " Runtime Generic CDPS Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.MegaTowerUpperTrimLights,
                    FeatureName = " Runtime Upper Monitor Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PeakSlantWoFTopper,
                    FeatureName = " Runtime WoF Video Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DiamondReelStepperTopBoxLights,
                    FeatureName = " DRS Topbox",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DiamondReelStepperSideLights,
                    FeatureName = " DRS Cabinet Side",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.DiamondReelStepperReelBacklights,
                    FeatureName = " DRS Reel Backlights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.PeakUniv27Topper,
                    FeatureName = " Runtime Peak Univ 27in Topper",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SkyLineButtonEdgeLeft,
                    FeatureName = " Runtime LFM Button Edge Left",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SkyLineButtonEdgeRight,
                    FeatureName = " Runtime LFM Button Edge Right",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SkyLineButtonDeck,
                    FeatureName = " Runtime LFM Button Deck",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SkylineConsole,
                    FeatureName = " Runtime LFM Console Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.SkyLineDualCurve,
                    FeatureName = " Runtime LFM Dual Curve Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFTrioLights,
                    FeatureName = " Runtime WoF Trio Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFTrioHub,
                    FeatureName = " Runtime WOFT Hub",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFTrioBonus,
                    FeatureName = " Runtime WOFT Lights",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFTrioPointers,
                    FeatureName = " Runtime WOFT Pointers",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSTopBox,
                    FeatureName = " WOF DRS Topbox",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSCrystalEdge,
                    FeatureName = " WOF DRS Crystal Edge",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSSign,
                    FeatureName = " WOF DRS Sign",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSLightRing,
                    FeatureName = " WOF DRS Light Ring",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSPointers,
                    FeatureName = " WOF DRS Pointers",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSWheel,
                    FeatureName = " WOF DRS Wheel",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                },

                new HardwareSpec
                {
                    HardwareType = Hardware.WOFDRSHub,
                    FeatureName = " WOF DRS Hub",
                    IsStreamingLight = true,
                    CreateDevice = CreateUsbStreamingLight
                }
            };

        #region Creator Methods

        /// <summary>
        /// Creates a <see cref="UsbHaloLight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateUsbHaloLight(
            string name, LightFeatureDescription description, object lightInterface)
        {
            return new UsbHaloLight(name, description, lightInterface as IPeripheralLights);
        }

        /// <summary>
        /// Creates a <see cref="UsbCrystalCoreLight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateUsbCrystalCoreLight(
            string name, LightFeatureDescription description, object lightInterface)
        {
            return new UsbCrystalCoreLight(name, description, lightInterface as IPeripheralLights);
        }

        /// <summary>
        /// Creates a <see cref="UsbStreamingLight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateUsbStreamingLight(
            string name, LightFeatureDescription description, object lightInterface)
        {
            return new UsbStreamingLight(name, description, lightInterface as IStreamingLights);
        }

        /// <summary>
        /// Creates a <see cref="UsbSymbolHighlightSupportedStreamingLight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateSymbolHighlightSupportedStreamingLight(
            string name, LightFeatureDescription description, object lightInterface)
        {
            return new UsbSymbolHighlightSupportedStreamingLight(name, description,
                lightInterface as IStreamingLights);
        }

        /// <summary>
        /// Creates a <see cref="UsbSymbolHighlightSupportedStreamingLight"/> if the device supports symbol highlights, 
        /// otherwise this method will create a <see cref="UsbSymbolHighlightSupportedStreamingLight"/>.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CheckForSymbolHighlightsAndCreateStreamingLight(string name,
            LightFeatureDescription description,
            object lightInterface)
        {
            return description?.Groups.Cast<StreamingLightGroup>().Any(group => group.SymbolHighlightsSupported) == true
                ? CreateSymbolHighlightSupportedStreamingLight(name, description, lightInterface)
                : CreateUsbStreamingLight(name, description, lightInterface);
        }

        /// <summary>
        /// Creates a <see cref="UsbNestedWheelLight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateUsbNestedWheelLight(
            string name, LightFeatureDescription description, object lightInterface)
        {
            return new UsbNestedWheelLight(name, description, lightInterface as IPeripheralLights);
        }

        /// <summary>
        /// Creates a <see cref="UsbLegacyBacklight"/> device.
        /// </summary>
        /// <param name="name">The feature name of the light device to create.</param>
        /// <param name="description">The feature description of the light device to create.</param>
        /// <param name="lightInterface">The interface object for the light device to communicate with the CSI manager.</param>
        /// <returns>The USB light device created.</returns>
        private static UsbLightBase CreateUsbLegacyBacklight(string name, LightFeatureDescription description,
            object lightInterface)
        {
            return new UsbLegacyBacklight(name, description, lightInterface as IPeripheralLights);
        }

        #endregion

    }
}