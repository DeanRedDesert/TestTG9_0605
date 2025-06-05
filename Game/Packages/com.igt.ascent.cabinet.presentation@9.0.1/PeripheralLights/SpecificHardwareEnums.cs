//-----------------------------------------------------------------------
// <copyright file = "SpecificHardwareEnums.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights
{
    /// <summary>
    /// The topper light hardware that is supported.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum TopperHardware
    {
        /// <summary>
        /// The Suzo Happ USB topper.
        /// </summary>
        SuzoHapp = Hardware.SuzoHapp,
    }

    /// <summary>
    /// The button light hardware supported.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ButtonHardware
    {
        /// <summary>
        /// The button edge lights around dynamic buttons.
        /// </summary>
        ButtonEdge = Hardware.ButtonEdge,
    }

    /// <summary>
    /// The halo light hardware supported.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum HaloHardware
    {
        /// <summary>
        /// The halo lights surrounding the ticket printer.
        /// </summary>
        TicketPrinter = Hardware.TicketPrinterHalo,

        /// <summary>
        /// The halo lights surrounding the bill validator.
        /// </summary>
        BillValidator = Hardware.BillValidatorHalo,

        /// <summary>
        /// The lights on the sides of the top box.
        /// </summary>
        Sidebar = Hardware.Sidebar,

        /// <summary>
        /// The lights on the top of the top box.
        /// </summary>
        Crown = Hardware.Crown,

        /// <summary>
        /// The halo lights surrounding the speakers.
        /// </summary>
        Speaker = Hardware.SpeakerHalo,

        /// <summary>
        /// The lights on the landscape top box.
        /// </summary>
        LandscapeTopBox = Hardware.LandscapeTopBox
    }

    /// <summary>
    /// The light bar hardware supported.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LightBarHardware
    {
        /// <summary>
        /// The light bars at the top and bottom of the monitors.
        /// </summary>
        LightBars = Hardware.LightBars,
    }

    /// <summary>
    /// The facade hardware supported.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum FacadeHardware
    {
        /// <summary>
        /// The game specific facade lighting.
        /// </summary>
        Facade = Hardware.Facade,
    }

    /// <summary>
    /// The mechanical reel lighting.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LegacyReelLightHardware
    {
        /// <summary>
        /// Legacy mechanical reel back lights.
        /// </summary>
        BackLights = Hardware.LegacyReelBackLights,

        /// <summary>
        /// Legacy mechanical reel front lights.
        /// </summary>
        FrontLights = Hardware.LegacyReelFrontLights,
    }

    /// <summary>
    /// The crystal core cabinet lights.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum CrystalCoreLightHardware
    {
        /// <summary>
        /// The light ring around the DPP.
        /// </summary>
        DppLightRing = Hardware.DppLightRing,
    }

    /// <summary>
    /// The streaming light hardware.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public enum StreamingLightHardware
    {
        /// <summary>
        /// If the hardware device cannot be identified.
        /// </summary>
        /// <remarks>
        /// This is used to ensure there isn't a Foundation dependency on this enum.
        /// It also has a specific value to make sure it never changes.
        /// </remarks>
        Unknown = Hardware.Unknown,

        /// <summary>
        /// The reel back lights on the Catalina cabinet.
        /// </summary>
        CatalinaReelBackLights = Hardware.CatalinaReelBackLights,

        /// <summary>
        /// The side lights on the Catalina cabinet.
        /// </summary>
        CatalinaSideLights = Hardware.CatalinaSideLights,

        /// <summary>
        /// The frame lights on the Catalina cabinet.
        /// </summary>
        CatalinaFrameLights = Hardware.CatalinaFrameLights,

        /// <summary>
        /// The reel accent lights on the Catalina reel shelf.
        /// </summary>
        CatalinaReelAccentLights = Hardware.CatalinaReelAccentLights,

        /// <summary>
        /// The trim lights on the Catalina cabinet.
        /// </summary>
        CatalinaTrimLights = Hardware.CatalinaTrimLights,

        /// <summary>
        /// The streaming lights on the handle.
        /// </summary>
        StreamingHandleLights = Hardware.StreamingHandleLights,

        /// <summary>
        /// The lights in the cabinet skylight.
        /// </summary>
        Skylight = Hardware.Skylight,

        /// <summary>
        /// The tail light topper.
        /// </summary>
        TailLightTopper = Hardware.TailLightTopper,

        /// <summary>
        /// The gills on the Crystal Core cabinet.
        /// </summary>
        CrystalCoreGills = Hardware.StreamingGills,

        /// <summary>
        /// The 42" edge lights on the Crystal Core cabinet.
        /// </summary>
        CrystalCore42EdgeLights = Hardware.StreamingCrystalCoreEdge42,

        /// <summary>
        /// The 30" edge lights on the Crystal Core cabinet.
        /// </summary>
        CrystalCore30EdgeLights = Hardware.StreamingCrystalCoreEdge30,

        /// <summary>
        /// The 27" edge lights on the Crystal Core cabinet.
        /// </summary>
        CrystalCore27EdgeLights = Hardware.StreamingCrystalCoreEdge27,

        /// <summary>
        /// The generic backlit topper.
        /// </summary>
        GenericBacklitTopper = Hardware.GenericBacklitTopper,

        /// <summary>
        /// The video topper.
        /// </summary>
        VideoTopper = Hardware.VideoTopper,

        /// <summary>
        /// The edge lighting around a mechanical wheel.
        /// </summary>
        WheelEdgeLighting = Hardware.WheelEdgeLighting,

        /// <summary>
        /// The pointer lights on a mechanical wheel which can be
        /// a single pointer or seven pointers.
        /// </summary>
        WheelPointers = Hardware.WheelPointers,

        /// <summary>
        /// The ring around the outside of a mechanical wheel.
        /// </summary>
        WheelLightRing = Hardware.WheelLightRing,

        /// <summary>
        /// The facade around a mechanical wheel.
        /// </summary>
        WheelFacade = Hardware.WheelFacade,

        /// <summary>
        /// The hub of a mechanical wheel.
        /// </summary>
        WheelHub = Hardware.WheelHub,

        /// <summary>
        /// The light bezel around the the top monitor on the titan cabinet with the 32 inch top box.
        /// </summary>
        Titan32TopBoxMonitorBezel = Hardware.Titan32TopBoxMonitorBezel,

        /// <summary>
        /// The light bezel around both monitors on the titan cabinet with a 27 inch top box.
        /// </summary>
        Titan27TopBoxDualMonitorBezel = Hardware.Titan27TopBoxDualMonitorBezel,

        /// <summary>
        /// The light bezel around the left, top, and right sides of the main monitor on the
        /// Titan cabinet. This is used with the single monitor and 32 inch top box variants
        /// of the cabinet.
        /// </summary>
        TitanMainMonitorBezel = Hardware.TitanMainMonitorBezel,

        /// <summary>
        /// The trim lighting on the Australian Crystal Core 23 cabinet.
        /// </summary>
        AustraliaCrystalCoreTrim = Hardware.AustraliaCrystalCoreTrim,

        /// <summary>
        /// The Flatpack lights on the Alcove cabinet stand.
        /// </summary>
        AlcoveFlatpack = Hardware.AlcoveFlatpack,

        /// <summary>
        /// The End Cap lights on the Alcove cabinet stand.
        /// A cabinet typically connects to a single End Cap.
        /// </summary>
        AlcoveEndCap = Hardware.AlcoveEndCap,

        /// <summary>
        /// The Carousel lights on the Alcove cabinet stand.
        /// </summary>
        AlcoveCarousel = Hardware.AlcoveCarousel,

        /// <summary>
        /// The edge lights on the Axxis cabinet.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Axxis23_23 = Hardware.Axxis23_23,

        /// <summary>
        /// The topper light ring on the Axxis cabinet.
        /// </summary>
        AxxisVideoTopper = Hardware.AxxisVideoTopper,

        /// <summary>
        /// The monitor lights on the Quasar cabinet.
        /// </summary>
        QuasarMonitorLights = Hardware.QuasarMonitorLights,

        /// <summary>
        /// The bill validator lights on the Quasar cabinet.
        /// </summary>
        QuasarBillValidatorLights = Hardware.QuasarBillValidatorLights,

        /// <summary>
        /// The printer lights on the Quasar cabinet.
        /// </summary>
        QuasarPrinterLights = Hardware.QuasarPrinterLights,

        /// <summary>
        /// The coin tray lights on the Quasar cabinet.
        /// </summary>
        QuasarCoinTrayLights = Hardware.QuasarCoinTrayLights,

        /// <summary>
        /// The wall lights on the Quasar cabinet.
        /// </summary>
        QuasarWallLights = Hardware.QuasarWallLights,

        /// <summary>
        /// The streaming light ring around the DPP device.
        /// </summary>
        StreamingDppLightRing = Hardware.StreamingDppLightRing,

        /// <summary>
        /// The spacer lights on the titan cabinet.
        /// </summary>
        TitanSpacer = Hardware.TitanSpacer,

        /// <summary>
        /// Topper lights for the Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperTopperLights = Hardware.CrystalDualPlusStepperTopperLights,

        /// <summary>
        /// Frame lights for the Top Dollar Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperTopDollarFrameLights = Hardware.CrystalDualPlusStepperTopDollarFrameLights,

        /// <summary>
        /// Dollar lights for the Top Dollar Crystal Dual Plus Stepper frame.
        /// </summary>
        CrystalDualPlusStepperTopDollarDollarLights = Hardware.CrystalDualPlusStepperTopDollarDollarLights,

        /// <summary>
        /// Bottom door lights for the Crystal Dual Plus Stepper. 
        /// </summary>
        CrystalDualPlusStepperBottomDoorLights = Hardware.CrystalDualPlusStepperBottomDoorLights,

        /// <summary>
        /// Gill extension lights for the Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperGillExtensionLights = Hardware.CrystalDualPlusStepperGillExtensionLights,

        /// <summary>
        /// Edge lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynEdgeLights = Hardware.CrystalDualPlusStepperMarilynEdgeLights,

        /// <summary>
        /// Inner mask lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynInnerMaskLights = Hardware.CrystalDualPlusStepperMarilynInnerMaskLights,

        /// <summary>
        /// Diamond lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynDiamondLights = Hardware.CrystalDualPlusStepperMarilynDiamondLights,

        /// <summary>
        /// Light ring for Gamesman Project button.
        /// </summary>
        GamesmanButtonLightRing = Hardware.GamesmanButtonLightRing,

        /// <summary>
        /// Frame lights for the Megabuck Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMegabucksFrameLights = Hardware.CrystalDualPlusStepperMegabucksFrameLights,

        /// <summary>
        /// Monitor light-bezel for the Crystal Curve cabinet.
        /// </summary>
        CrystalCurveMonitorLights = Hardware.CrystalCurveMonitorLights,

        /// <summary>
        /// Monitor light-bezel for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DMonitorBezelLights = Hardware.CrystalDualPlusVideo4DMonitorBezelLights,

        /// <summary>
        /// Side trim lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DSideTrimLights = Hardware.CrystalDualPlusVideo4DSideTrimLights,

        /// <summary>
        /// Lower trim lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DLowerTrimLights = Hardware.CrystalDualPlusVideo4DLowerTrimLights,

        /// <summary>
        /// Stand lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DStandLights = Hardware.CrystalDualPlusVideo4DStandLights,

        /// <summary>
        /// Gills for the Crystal Curve cabinet.
        /// </summary>
        CrystalCurveGills = Hardware.CrystalCurveGills,

        /// <summary>
        /// Placeholder for virtual devices.
        /// </summary>
        Virtual = Hardware.Virtual,

        /// <summary>
        /// The halo lights surrounding the Double Up (left) button on the Dual Smash DPP.
        /// </summary>
        DualSmashDoubleUpHalo = Hardware.DualSmashDoubleUpHalo,

        /// <summary>
        /// The halo lights surrounding the Repeat Bet (right) button on the Dual Smash DPP.
        /// </summary>
        DualSmashRepeatBetHalo = Hardware.DualSmashRepeatBetHalo,

        /// <summary>
        /// The handle accent lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerHandleAccentLights = Hardware.MegaTowerHandleAccentLights,

        /// <summary>
        /// The wheel bezel lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelBezelLights = Hardware.MegaTowerWheelBezelLights,

        /// <summary>
        /// The wheel hub lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelHubLights = Hardware.MegaTowerWheelHubLights,

        /// <summary>
        /// The wheel pointer lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelPointerLights = Hardware.MegaTowerWheelPointerLights,

        /// <summary>
        /// The generic marquee lights.
        /// </summary>
        MarqueeLights = Hardware.MarqueeLights,

        /// <summary>
        /// The generic marquee backlight.
        /// </summary>
        MarqueeBacklight = Hardware.MarqueeBacklight,

        /// <summary>
        /// The spectrum display device.
        /// </summary>
        Spectrum = Hardware.Spectrum,

        /// <summary>
        /// Wheel bezel lights for the poker cabinet.
        /// </summary>
        PokerWheelBezel = Hardware.PokerWheelBezel,

        /// <summary>
        /// Left justified poker wheel lights.
        /// </summary>
        PokerWheelLeft = Hardware.PokerWheelLeft,

        /// <summary>
        /// Center justified poker wheel lights.
        /// </summary>
        PokerWheelCenter = Hardware.PokerWheelCenter,

        /// <summary>
        /// Right justified poker wheel lights.
        /// </summary>
        PokerWheelRight = Hardware.PokerWheelRight,

        /// <summary>
        /// Configuration 0 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig0 = Hardware.EtgLightsConfig0,

        /// <summary>
        /// Configuration 1 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig1 = Hardware.EtgLightsConfig1,

        /// <summary>
        /// Configuration 2 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig2 = Hardware.EtgLightsConfig2,

        /// <summary>
        /// Configuration 3 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig3 = Hardware.EtgLightsConfig3,

        /// <summary>
        /// Configuration 4 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig4 = Hardware.EtgLightsConfig4,

        /// <summary>
        /// Configuration 5 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig5 = Hardware.EtgLightsConfig5,

        /// <summary>
        /// Configuration 6 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig6 = Hardware.EtgLightsConfig6,

        /// <summary>
        /// Configuration 7 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig7 = Hardware.EtgLightsConfig7,

        /// <summary>
        /// Accent lights for Crystal Dual Plus Stepper Temple of Treasure cabinet.
        /// </summary>
        CrystalDualPlusStepperTotAccentLights = Hardware.CrystalDualPlusStepperTotAccentLights,

        /// <summary>
        /// Generic facade rig for the Crystal Dual Plus Stepper cabinet.
        /// </summary>
        CrystalDualPlusStepperGenericLights = Hardware.CrystalDualPlusStepperGenericLights,

        /// <summary>
        /// Trim lights for the MegaTower cabinet.
        /// </summary>
        MegatowerUpperTrimLights = Hardware.MegaTowerUpperTrimLights,

        /// <summary>
        /// Topper Lights for the Wheel of Fortune on the Peak Slant.
        /// </summary>
        PeakSlantWoFTopper = Hardware.PeakSlantWoFTopper,

        /// <summary>
        /// Top Box lights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperTopBoxLights = Hardware.DiamondReelStepperTopBoxLights,

        /// <summary>
        /// Side lights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperSideLights = Hardware.DiamondReelStepperSideLights,

        /// <summary>
        /// Reel Backlights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperReelBacklights = Hardware.DiamondReelStepperReelBacklights,

        /// <summary>
        /// Left Button Edge for SL cabinet
        /// </summary>
        SkyLineButtonEdgeLeft = Hardware.SkyLineButtonEdgeLeft,

        /// <summary>
        /// Right Button Edge for SL cabinet
        /// </summary>
        SkyLineButtonEdgeRight = Hardware.SkyLineButtonEdgeRight,

        /// <summary>
        /// Button Deck for SL Cabinet
        /// </summary>
        SkyLineButtonDeck = Hardware.SkyLineButtonDeck,

        /// <summary>
        /// Sky line Console Ligts
        /// </summary>
        SkylineConsole = Hardware.SkylineConsole,

        /// <summary>
        /// Dual Curve Lights for SL Cabinet
        /// </summary>
        SkyLineDualCurve = Hardware.SkyLineDualCurve,

        /// <summary>
        /// Peak Univ 27in Topper
        /// </summary>
        PeakUniv27Topper = Hardware.PeakUniv27Topper,

        /// <summary>
        /// Wheel of Fortune Trio Lights
        /// </summary>
        WOFTrioLights = Hardware.WOFTrioLights,

        /// <summary>
        /// Wheel of Fortune Trio Hub
        /// </summary>
        WOFTrioHub = Hardware.WOFTrioHub,

        /// <summary>
        /// Wheel of Fortune Bonus Game Lights
        /// </summary>
        WOFTrioBonus = Hardware.WOFTrioBonus,

        /// <summary>
        /// Wheel of Fortune Bonus Game Pointer Lights
        /// </summary>
        WOFTrioPointers = Hardware.WOFTrioPointers,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Top box
        /// </summary>
        WOFDRSTopBox = Hardware.WOFDRSTopBox,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Crystal Edge
        /// </summary>
        WOFDRSCrystalEdge = Hardware.WOFDRSCrystalEdge,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Sign
        /// </summary>
        WOFDRSSign = Hardware.WOFDRSSign,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Light Ring
        /// </summary>
        WOFDRSLightRing = Hardware.WOFDRSLightRing,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Pointers
        /// </summary>
        WOFDRSPointers = Hardware.WOFDRSPointers,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Wheel
        /// </summary>
        WOFDRSWheel = Hardware.WOFDRSWheel,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Hub
        /// </summary>
        WOFDRSHub = Hardware.WOFDRSHub
    }
    // ReSharper restore UnusedMember.Global

    /// <summary>
    /// Streaming Light hardware that support symbol highlights.
    /// </summary>
    public enum SymbolHighlightSupportedStreamingLightHardware
    {
        /// <summary>
        /// If the hardware device cannot be identified.
        /// </summary>
        /// <remarks>
        /// This is used to ensure there isn't a Foundation dependency on this enum.
        /// It also has a specific value to make sure it never changes.
        /// </remarks>
        Unknown = Hardware.Unknown,

        /// <summary>
        /// The reel back lights on the Catalina cabinet.
        /// </summary>
        CatalinaReelBackLights = Hardware.CatalinaReelBackLights
    }

    /// <summary>
    /// The cabinet handle lighting.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum HandleLightHardware
    {
        /// <summary>
        /// The lights on the cabinet handle.
        /// </summary>
        Handle = Hardware.HandleLights
    }

    /// <summary>
    /// The nested wheels light hardware.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum NestedwheelsLightHardware
    {
        /// <summary>
        /// The lights on the small wheel.
        /// </summary>
        SmallNestedWheelLights = Hardware.SmallNestedWheelLights,

        /// <summary>
        /// The lights on the medium wheel.
        /// </summary>
        MediumNestedWheelLights = Hardware.MediumNestedWheelLights,

        /// <summary>
        /// The lights on the large wheel.
        /// </summary>
        LargeNestedWheelLights = Hardware.LargeNestedWheelLights,
    }

    /// <summary>
    /// The twilight zone 3D monitor bezel hardware.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum TwilightZone3DMonitorBezelHardware
    {
        /// <summary>
        /// The monitor bezel lights.
        /// </summary>
        TwilightZone3DMonitorBezel = Hardware.TwilightZone3DMonitorBezelLights,
    }

    /// <summary>
    /// Legacy back light hardware.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LegacyBacklightHardware
    {
        /// <summary>
        /// Legacy Topper back light.
        /// </summary>
        LegacyTopperBacklight = Hardware.TopperBacklight,

        /// <summary>
        /// Mega Tower Wheel back light.
        /// </summary>
        LegacyWheelBacklight = Hardware.WheelBacklight
    }
}
