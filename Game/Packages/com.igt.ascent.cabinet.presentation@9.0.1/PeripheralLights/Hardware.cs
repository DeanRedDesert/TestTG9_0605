//-----------------------------------------------------------------------
// <copyright file = "Hardware.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights
{
    /// <summary>
    /// The different peripheral lights hardware supported.
    /// </summary>
    public enum Hardware
    {
        /// <summary>
        /// The hardware is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The Suzo Happ USB topper.
        /// </summary>
        SuzoHapp,

        /// <summary>
        /// The button edge lights around dynamic buttons.
        /// </summary>
        ButtonEdge,

        /// <summary>
        /// The halo lights surrounding the ticket printer.
        /// </summary>
        TicketPrinterHalo,

        /// <summary>
        /// The halo lights surrounding the speakers.
        /// </summary>
        SpeakerHalo,

        /// <summary>
        /// The halo lights surrounding the bill validator.
        /// </summary>
        BillValidatorHalo,

        /// <summary>
        /// The lights on the sides of the top box.
        /// </summary>
        Sidebar,

        /// <summary>
        /// The lights on the top of the top box.
        /// </summary>
        Crown,

        /// <summary>
        /// The light bars at the top and bottom of the monitors.
        /// </summary>
        LightBars,

        /// <summary>
        /// The game specific facade lighting.
        /// </summary>
        Facade,

        /// <summary>
        /// Mechanical reel backlighting.
        /// </summary>
        LegacyReelBackLights,

        /// <summary>
        /// Mechanical reel front lighting.
        /// </summary>
        LegacyReelFrontLights,

        /// <summary>
        /// The gill cabinet lights.
        /// </summary>
        Gills,

        /// <summary>
        /// The light ring around the DPP.
        /// </summary>
        DppLightRing,

        /// <summary>
        /// The reel back lights on the Catalina cabinet.
        /// </summary>
        CatalinaReelBackLights,

        /// <summary>
        /// The side lights on the Catalina cabinet.
        /// </summary>
        CatalinaSideLights,

        /// <summary>
        /// The frame lights on the Catalina cabinet.
        /// </summary>
        CatalinaFrameLights,

        /// <summary>
        /// The reel accent lights on the Catalina reel shelf.
        /// </summary>
        CatalinaReelAccentLights,

        /// <summary>
        /// The cabinet trim lights on the Catalina cabinet.
        /// </summary>
        CatalinaTrimLights,

        /// <summary>
        /// The lights on the handle.
        /// </summary>
        HandleLights,

        /// <summary>
        /// The streaming lights on the handle.
        /// </summary>
        StreamingHandleLights,

        /// <summary>
        /// The lights in the cabinet skylight.
        /// </summary>
        Skylight,

        /// <summary>
        /// The tail light topper.
        /// </summary>
        TailLightTopper,

        /// <summary>
        /// The lights on the landscape top box.
        /// </summary>
        LandscapeTopBox,

        /// <summary>
        /// The streaming version of the gills on the Crystal Core cabinet.
        /// </summary>
        StreamingGills,

        /// <summary>
        /// The streaming version of the 42 inch edge lights on the Crystal Core cabinet.
        /// </summary>
        StreamingCrystalCoreEdge42,

        /// <summary>
        /// The streaming version of the 30 inch edge lights on the Crystal Core cabinet.
        /// </summary>
        StreamingCrystalCoreEdge30,

        /// <summary>
        /// The generic backlit topper.
        /// </summary>
        GenericBacklitTopper,

        /// <summary>
        /// The video topper.
        /// </summary>
        VideoTopper,

        /// <summary>
        /// The edge lighting around a mechanical wheel.
        /// </summary>
        WheelEdgeLighting,

        /// <summary>
        /// The pointer lights on a mechanical wheel which can be
        /// a single pointer or seven pointers.
        /// </summary>
        WheelPointers,

        /// <summary>
        /// The ring around the outside of a mechanical wheel.
        /// </summary>
        WheelLightRing,

        /// <summary>
        /// The facade around a mechanical wheel.
        /// </summary>
        WheelFacade,

        /// <summary>
        /// The hub of a mechanical wheel.
        /// </summary>
        WheelHub,

        /// <summary>
        /// The light bezel around the the top monitor on the titan cabinet with the 32 inch top box.
        /// </summary>
        Titan32TopBoxMonitorBezel,

        /// <summary>
        /// The light bezel around both monitors on the titan cabinet with a 27 inch top box.
        /// </summary>
        Titan27TopBoxDualMonitorBezel,

        /// <summary>
        /// The light bezel around the left, top, and right sides of the main monitor on the
        /// Titan cabinet. This is used with the single monitor and 32 inch top box variants
        /// of the cabinet.
        /// </summary>
        TitanMainMonitorBezel,

        /// <summary>
        /// The trim lighting on the Australian Crystal Core 23 cabinet.
        /// </summary>
        AustraliaCrystalCoreTrim,

        /// <summary>
        /// The Flatpack lights on the Alcove cabinet stand.
        /// </summary>
        AlcoveFlatpack,

        /// <summary>
        /// The End Cap lights on the Alcove cabinet stand.
        /// A cabinet typically connects to a single End Cap.
        /// </summary>
        AlcoveEndCap,

        /// <summary>
        /// The Carousel lights on the Alcove cabinet stand.
        /// </summary>
        AlcoveCarousel,

        /// <summary>
        /// The edge lights on the Axxis cabinet.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Axxis23_23,

        /// <summary>
        /// The topper light ring on the Axxis cabinet.
        /// </summary>
        AxxisVideoTopper,

        /// <summary>
        /// The monitor lights on the Quasar cabinet.
        /// </summary>
        QuasarMonitorLights,

        /// <summary>
        /// The bill validator lights on the Quasar cabinet.
        /// </summary>
        QuasarBillValidatorLights,

        /// <summary>
        /// The printer lights on the Quasar cabinet.
        /// </summary>
        QuasarPrinterLights,

        /// <summary>
        /// The coin tray lights on the Quasar cabinet.
        /// </summary>
        QuasarCoinTrayLights,

        /// <summary>
        /// The wall lights on the Quasar cabinet.
        /// </summary>
        QuasarWallLights,

        /// <summary>
        /// The streaming light ring around the DPP device.
        /// </summary>
        StreamingDppLightRing,

        /// <summary>
        /// The spacer light device for the titan cabinet.
        /// </summary>
        TitanSpacer,

        /// <summary>
        /// Topper lights for the Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperTopperLights,

        /// <summary>
        /// Frame lights for the Top Dollar Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperTopDollarFrameLights,

        /// <summary>
        /// Dollar lights for the Top Dollar Crystal Dual Plus Stepper frame.
        /// </summary>
        CrystalDualPlusStepperTopDollarDollarLights,

        /// <summary>
        /// Bottom door lights for the Crystal Dual Plus Stepper. 
        /// </summary>
        CrystalDualPlusStepperBottomDoorLights,

        /// <summary>
        /// Gill extension lights for the Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperGillExtensionLights,

        /// <summary>
        /// Edge lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynEdgeLights,

        /// <summary>
        /// Inner mask lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynInnerMaskLights,

        /// <summary>
        /// Diamond lights for the Marilyn Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMarilynDiamondLights,

        /// <summary>
        /// Light ring for Gamesman Project button.
        /// </summary>
        GamesmanButtonLightRing,

        /// <summary>
        /// Frame lights for the Megabucks Crystal Dual Plus Stepper.
        /// </summary>
        CrystalDualPlusStepperMegabucksFrameLights,

        /// <summary>
        /// Monitor light-bezel for the Crystal Curve cabinet.
        /// </summary>
        CrystalCurveMonitorLights,

        /// <summary>
        /// Monitor light-bezel for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DMonitorBezelLights,

        /// <summary>
        /// Side trim lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DSideTrimLights,

        /// <summary>
        /// Lower trim lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DLowerTrimLights,

        /// <summary>
        /// Stand lights for the Crystal Dual Plus Video 4D cabinet.
        /// </summary>
        CrystalDualPlusVideo4DStandLights,

        /// <summary>
        /// The lights on the small wheel from the triple nested wheels hardware.
        /// </summary>
        SmallNestedWheelLights,

        /// <summary>
        /// The lights on the medium wheel from the triple nested wheels hardware.
        /// </summary>
        MediumNestedWheelLights,

        /// <summary>
        /// The lights on the large wheel from the triple nested wheels hardware.
        /// </summary>
        LargeNestedWheelLights,

        /// <summary>
        /// The lights on the twilight zone 3D monitor.
        /// </summary>
        TwilightZone3DMonitorBezelLights,

        /// <summary>
        /// Gills for the Crystal Curve cabinet.
        /// </summary>
        CrystalCurveGills,

        /// <summary>
        /// Placeholder for virtual devices.
        /// </summary>
        Virtual,

        /// <summary>
        /// The halo lights surrounding the Double Up (left) button on the Dual Smash DPP.
        /// </summary>
        DualSmashDoubleUpHalo,

        /// <summary>
        /// The halo lights surrounding the Repeat Bet (right) button on the Dual Smash DPP.
        /// </summary>
        DualSmashRepeatBetHalo,

        /// <summary>
        /// The handle accent lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerHandleAccentLights,

        /// <summary>
        /// The wheel bezel lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelBezelLights,

        /// <summary>
        /// The wheel hub lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelHubLights,

        /// <summary>
        /// The wheel pointer lights on the MegaTower cabinet.
        /// </summary>
        MegaTowerWheelPointerLights,

        /// <summary>
        /// The generic marquee lights.
        /// </summary>
        MarqueeLights,

        /// <summary>
        /// The generic marquee backlight.
        /// </summary>
        MarqueeBacklight,

        /// <summary>
        /// The spectrum display device.
        /// </summary>
        Spectrum,

        /// <summary>
        /// Wheel bezel lights for the poker cabinet.
        /// </summary>
        PokerWheelBezel,

        /// <summary>
        /// Left justified poker wheel lights.
        /// </summary>
        PokerWheelLeft,

        /// <summary>
        /// Center justified poker wheel lights.
        /// </summary>
        PokerWheelCenter,

        /// <summary>
        /// Right justified poker wheel lights.
        /// </summary>
        PokerWheelRight,

        /// <summary>
        /// Mega Tower Wheel back light.
        /// </summary>
        WheelBacklight,

        /// <summary>
        /// Legacy Topper back light.
        /// </summary>
        TopperBacklight,

        /// <summary>
        /// Configuration 0 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig0,

        /// <summary>
        /// Configuration 1 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig1,

        /// <summary>
        /// Configuration 2 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig2,

        /// <summary>
        /// Configuration 3 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig3,

        /// <summary>
        /// Configuration 4 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig4,

        /// <summary>
        /// Configuration 5 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig5,

        /// <summary>
        /// Configuration 6 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig6,

        /// <summary>
        /// Configuration 7 lights for the Electronic Table Games cabinet.
        /// </summary>
        EtgLightsConfig7,

        /// <summary>
        /// The streaming version of the 27 inch edge lights on the Crystal Core cabinet.
        /// </summary>
        StreamingCrystalCoreEdge27,

        /// <summary>
        /// Accent lights for Crystal Dual Plus Stepper Temple of Treasure cabinet.
        /// </summary>
        CrystalDualPlusStepperTotAccentLights,

        /// <summary>
        /// Generic facade rig for the Crystal Dual Plus Stepper cabinet.
        /// </summary>
        CrystalDualPlusStepperGenericLights,

        /// <summary>
        /// Trim lights for the MegaTower cabinet.
        /// </summary>
        MegaTowerUpperTrimLights,

        /// <summary>
        /// Topper Lights for the Wheel of Fortune on the Peak Slant.
        /// </summary>
        PeakSlantWoFTopper,

        /// <summary>
        /// Top Box lights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperTopBoxLights,

        /// <summary>
        /// Side lights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperSideLights,

        /// <summary>
        /// Reel Backlights for the DRS cabinet.
        /// </summary>
        DiamondReelStepperReelBacklights,

        /// <summary>
        /// Left Button Edge for SL cabinet
        /// </summary>
        SkyLineButtonEdgeLeft,

        /// <summary>
        /// Right Button Edge for SL cabinet
        /// </summary>
        SkyLineButtonEdgeRight,

        /// <summary>
        /// Button Deck for SL Cabinet
        /// </summary>
        SkyLineButtonDeck,

        /// <summary>
        /// Sky line Console Lights
        /// </summary>
        SkylineConsole,

        /// <summary>
        /// Dual Curve Lights for SL Cabinet
        /// </summary>
        SkyLineDualCurve,

        /// <summary>
        /// Peak Univ 27in Topper
        /// </summary>
        PeakUniv27Topper,

        /// <summary>
        /// Wheel of Fortune Trio Lights
        /// </summary>
        WOFTrioLights,

        /// <summary>
        /// Wheel of Fortune Trio Hub
        /// </summary>
        WOFTrioHub,

        /// <summary>
        /// Wheel of Fortune Bonus Game Lights
        /// </summary>
        WOFTrioBonus,

        /// <summary>
        /// Wheel of Fortune Bonus Game Pointer Lights
        /// </summary>
        WOFTrioPointers,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Top box
        /// </summary>
        WOFDRSTopBox,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Crystal Edge
        /// </summary>
        WOFDRSCrystalEdge,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Sign
        /// </summary>
        WOFDRSSign,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Light Ring
        /// </summary>
        WOFDRSLightRing,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Pointers
        /// </summary>
        WOFDRSPointers,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Wheel
        /// </summary>
        WOFDRSWheel,

        /// <summary>
        /// Wheel of Fortune Diamond Reel Shelf Hub
        /// </summary>
        WOFDRSHub
    }
}
