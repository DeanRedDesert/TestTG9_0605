//-----------------------------------------------------------------------
// <copyright file = "TypeConverters.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp
{
    using System.Collections.Generic;
    using MachineConfiguration;
    using Pid;
    using Progressive;
    using ProgressiveAward;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2X schema types.
    /// </summary>
    internal static class TypeConverters
    {
        /// <summary>
        /// Mappings from F2X <see cref="UgpMachineConfigurationWinCapStyleEnum"/> to <see cref="UgpMachineConfigurationWinCapStyle"/>.
        /// </summary>
        private static readonly Dictionary<UgpMachineConfigurationWinCapStyleEnum, UgpMachineConfigurationWinCapStyle> MapToPublicMachineConfigurationWinCapStyle =
            new Dictionary<UgpMachineConfigurationWinCapStyleEnum, UgpMachineConfigurationWinCapStyle>
            {
                { UgpMachineConfigurationWinCapStyleEnum.None,            UgpMachineConfigurationWinCapStyle.None },
                { UgpMachineConfigurationWinCapStyleEnum.Clip,            UgpMachineConfigurationWinCapStyle.Clip },
                { UgpMachineConfigurationWinCapStyleEnum.ClipAndBreakout, UgpMachineConfigurationWinCapStyle.ClipAndBreakout },
            };

        /// <summary>
        /// Extension method to convert a F2X <see cref="UgpMachineConfigurationWinCapStyleEnum"/> to
        /// a <see cref="UgpMachineConfigurationWinCapStyle"/>.
        /// </summary>
        /// <param name="winCapStyle">The internal win cap style to convert.</param>
        /// <returns>The conversion result.</returns>
        public static UgpMachineConfigurationWinCapStyle ToPublic(this UgpMachineConfigurationWinCapStyleEnum winCapStyle)
        {
            return MapToPublicMachineConfigurationWinCapStyle[winCapStyle];
        }

        /// <summary>
        /// Converts an instance of the F2X <see cref="ProgressiveLevelInfo"/> class to
        /// a new instance of the public <see cref="ProgressiveLevel"/> class.
        /// </summary>
        /// <param name="progressiveLevelInfo">
        /// The F2X type to convert.
        /// </param>
        /// <returns>
        /// The converted result.
        /// </returns>
        public static ProgressiveLevel ToPublic(this ProgressiveLevelInfo progressiveLevelInfo)
        {
            return new ProgressiveLevel(progressiveLevelInfo);
        }

        /// <summary>
        /// Mappings from F2X <see cref="GameInformationDisplayStyleEnum"/> to <see cref="GameInformationDisplayStyle"/>.
        /// </summary>
        private static readonly Dictionary<GameInformationDisplayStyleEnum, GameInformationDisplayStyle> MapToPublicGameInformationDisplayStyle =
            new Dictionary<GameInformationDisplayStyleEnum, GameInformationDisplayStyle>
            {
                { GameInformationDisplayStyleEnum.None,       GameInformationDisplayStyle.None },
                { GameInformationDisplayStyleEnum.Victoria,   GameInformationDisplayStyle.Victoria },
                { GameInformationDisplayStyleEnum.Queensland, GameInformationDisplayStyle.Queensland },
                { GameInformationDisplayStyleEnum.NewZealand, GameInformationDisplayStyle.NewZealand },
            };

        /// <summary>
        /// Mappings from F2X <see cref="SessionTrackingOptionEnum"/> to <see cref="SessionTrackingOption"/>.
        /// </summary>
        private static readonly Dictionary<SessionTrackingOptionEnum, SessionTrackingOption> MapToPublicSessionTrackingOption =
            new Dictionary<SessionTrackingOptionEnum, SessionTrackingOption>
            {
                { SessionTrackingOptionEnum.Disabled,         SessionTrackingOption.Disabled },
                { SessionTrackingOptionEnum.Viewable,         SessionTrackingOption.Viewable },
                { SessionTrackingOptionEnum.PlayerControlled, SessionTrackingOption.PlayerControlled },
            };

        /// <summary>
        /// Extension method to convert a F2X <see cref="GameInformationDisplayStyleEnum"/> to
        /// a <see cref="GameInformationDisplayStyle"/>.
        /// </summary>
        /// <param name="displayStyle">The internal game information display style to convert.</param>
        /// <returns>The conversion result.</returns>
        public static GameInformationDisplayStyle ToPublic(this GameInformationDisplayStyleEnum displayStyle)
        {
            return MapToPublicGameInformationDisplayStyle[displayStyle];
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="SessionTrackingOptionEnum"/> to
        /// a <see cref="SessionTrackingOption"/>.
        /// </summary>
        /// <param name="option">The internal session tracking option to convert.</param>
        /// <returns>The conversion result.</returns>
        public static SessionTrackingOption ToPublic(this SessionTrackingOptionEnum option)
        {
            return MapToPublicSessionTrackingOption[option];
        }

        /// <summary>
        /// Mappings from F2X <see cref="ProgressiveAwardPayTypeEnum"/> to <see cref="ProgressiveAwardPayType"/>.
        /// </summary>
        private static readonly Dictionary<ProgressiveAwardPayTypeEnum, ProgressiveAwardPayType> MapToPublicProgressiveAwardPayType =
            new Dictionary<ProgressiveAwardPayTypeEnum, ProgressiveAwardPayType>
            {
                { ProgressiveAwardPayTypeEnum.CreditMeter, ProgressiveAwardPayType.CreditMeter },
                { ProgressiveAwardPayTypeEnum.WinMeter,    ProgressiveAwardPayType.WinMeter },
                { ProgressiveAwardPayTypeEnum.Attendant,   ProgressiveAwardPayType.Attendant },
                { ProgressiveAwardPayTypeEnum.Cashless,    ProgressiveAwardPayType.Cashless },
            };

        /// <summary>
        /// Extension method to convert a F2X <see cref="ProgressiveAwardPayTypeEnum"/> to
        /// a <see cref="ProgressiveAwardPayType"/>.
        /// </summary>
        /// <param name="payType">The internal progressive award pay type to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ProgressiveAwardPayType ToPublic(this ProgressiveAwardPayTypeEnum payType)
        {
            return MapToPublicProgressiveAwardPayType[payType];
        }

        /// <summary>
        /// Converts an instance of the F2X <see cref="PidConfigurationInfo"/> class to
        /// a new instance of the public <see cref="PidConfiguration"/> class.
        /// </summary>
        /// <param name="pidConfigurationInfo">
        /// The F2X type to convert.
        /// </param>
        /// <returns>
        /// The converted result.
        /// </returns>
        public static PidConfiguration ToPublic(this PidConfigurationInfo pidConfigurationInfo)
        {
            return new PidConfiguration(pidConfigurationInfo);
        }

        /// <summary>
        /// Converts an instance of the F2X <see cref="PidSessionDataInfo"/> class to
        /// a new instance of the public <see cref="PidSessionData"/> class.
        /// </summary>
        /// <param name="pidSessionDataInfo">
        /// The F2X type to convert.
        /// </param>
        /// <returns>
        /// The converted result.
        /// </returns>
        public static PidSessionData ToPublic(this PidSessionDataInfo pidSessionDataInfo)
        {
            return new PidSessionData(pidSessionDataInfo);
        }
    }
}
