// -----------------------------------------------------------------------
// <copyright file = "CategorySupportRegistry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// This static class keeps a registry of all F2X categories (excluding Legacy F2L categories)
    /// that are currently supported by Ascent SDK.
    /// </summary>
    public static class CategorySupportRegistry
    {
        #region Supported Message Category Versions

        // The order of the following items matches the order in the MessageCategory enum.

        /// <summary>
        /// Link Control Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation LinkControlCategory1P1 =
            new CategoryVersionInformation((int)MessageCategory.F2XLinkControl, 1, 1);

        /// <summary>
        /// Link Control Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation LinkControlCategory1P2 =
            new CategoryVersionInformation((int)MessageCategory.F2XLinkControl, 1, 2);

        /// <summary>
        /// Action Request Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ActionRequest1P0 =
            new CategoryVersionInformation((int)MessageCategory.ActionRequest, 1, 0);

        /// <summary>
        /// Activate Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation Activation1P0 =
            new CategoryVersionInformation((int)MessageCategory.Activation, 1, 0);

        /// <summary>
        /// Progressive Data Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ProgressiveData1P0 =
            new CategoryVersionInformation((int)MessageCategory.ProgressiveData, 1, 0);

        /// <summary>
        /// Custom Configuration Read Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation CustomConfigurationRead1P1 =
            new CategoryVersionInformation((int)MessageCategory.CustomConfigurationRead, 1, 1);

        /// <summary>
        /// Game Information Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation GameInformation1P1 =
            new CategoryVersionInformation((int)MessageCategory.GameInformation, 1, 1);

        /// <summary>
        /// Game Information Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation GameInformation1P2 =
            new CategoryVersionInformation((int)MessageCategory.GameInformation, 1, 2);

        /// <summary>
        /// Localization Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation Localization1P1 =
            new CategoryVersionInformation((int)MessageCategory.Localization, 1, 1);

        /// <summary>
        /// Localization Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation Localization1P2 =
            new CategoryVersionInformation((int)MessageCategory.Localization, 1, 2);

        /// <summary>
        /// EGM Config Data Category, version 1.3.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P3 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 3);

        /// <summary>
        /// EGM Config Data Category, version 1.4.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P4 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 4);

        /// <summary>
        /// EGM Config Data Category, version 1.5.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P5 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 5);

        /// <summary>
        /// EGM Config Data Category, version 1.6.
        /// </summary>
        private static readonly CategoryVersionInformation EgmConfigData1P6 =
            new CategoryVersionInformation((int)MessageCategory.EgmConfigData, 1, 6);

        /// <summary>
        /// Non-transactional Reading Critical Data Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation NonTransactionalCritDataRead1P2 =
            new CategoryVersionInformation((int)MessageCategory.NonTransactionalCritDataRead, 1, 2);

        /// <summary>
        /// Culture Read Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation CultureRead1P0 =
            new CategoryVersionInformation((int)MessageCategory.CultureRead, 1, 0);

        /// <summary>
        /// Culture Read Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation CultureRead1P2 =
            new CategoryVersionInformation((int)MessageCategory.CultureRead, 1, 2);

        /// <summary>
        /// Transactional Reading Critical Data Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation TransactionalCritDataRead1P1 =
            new CategoryVersionInformation((int)MessageCategory.TransactionalCritDataRead, 1, 1);

        /// <summary>
        /// Game Group Information Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GameGroupInformation1P0 =
            new CategoryVersionInformation((int)MessageCategory.GameGroupInformation, 1, 0);

        /// <summary>
        /// Transactional Writing Critical Data Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation TransactionalCritDataWrite1P0 =
            new CategoryVersionInformation((int)MessageCategory.TransactionalCritDataWrite, 1, 0);

        /// <summary>
        /// Tilt Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation TiltControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.TiltControl, 1, 0);

        /// <summary>
        /// Tilt Control Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation TiltControl1P2 =
            new CategoryVersionInformation((int)MessageCategory.TiltControl, 1, 2);

        /// <summary>
        /// Tilt Control Category, version 1.3.
        /// </summary>
        private static readonly CategoryVersionInformation TiltControl1P3 =
            new CategoryVersionInformation((int)MessageCategory.TiltControl, 1, 3);

        /// <summary>
        /// Parcel Comm Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation ParcelComm1P1 =
            new CategoryVersionInformation((int)MessageCategory.ParcelComm, 1, 1);

        /// <summary>
        /// Parcel Comm Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation ParcelComm1P2 =
            new CategoryVersionInformation((int)MessageCategory.ParcelComm, 1, 2);

        /// <summary>
        /// Parcel Comm Category, version 1.3.
        /// </summary>
        private static readonly CategoryVersionInformation ParcelComm1P3 =
            new CategoryVersionInformation((int)MessageCategory.ParcelComm, 1, 3);

        /// <summary>
        /// Display Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation DisplayControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.DisplayControl, 1, 0);

        /// <summary>
        /// Bank Status Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation BankStatus1P0 =
            new CategoryVersionInformation((int)MessageCategory.BankStatus, 1, 0);

        /// <summary>
        /// Bank Status Control Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation BankStatus1P1 =
            new CategoryVersionInformation((int)MessageCategory.BankStatus, 1, 1);

        /// <summary>
        /// Chooser Services Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ChooserServices1P0 =
            new CategoryVersionInformation((int)MessageCategory.ChooserServices, 1, 0);

        /// <summary>
        /// Shell API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellApiControl, 1, 0);

        /// <summary>
        /// Shell Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellActivation, 1, 0);

        /// <summary>
        /// Coplayer Management Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation CoplayerManagement1P0 =
            new CategoryVersionInformation((int)MessageCategory.CoplayerManagement, 1, 0);

        /// <summary>
        /// Coplayer API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation CoplayerApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.CoplayerApiControl, 1, 0);

        /// <summary>
        /// Coplayer Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation CoplayerActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.CoplayerActivation, 1, 0);

        /// <summary>
        /// Shell Theme Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellThemeControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellThemeControl, 1, 0);

        /// <summary>
        /// Session Management Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation SessionManagement1P0 =
            new CategoryVersionInformation((int)MessageCategory.SessionManagement, 1, 0);

        /// <summary>
        /// Game Cycle Betting Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GameCycleBetting1P0 =
            new CategoryVersionInformation((int)MessageCategory.GameCycleBetting, 1, 0);

        /// <summary>
        /// Shell Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellStore, 1, 0);

        /// <summary>
        /// Game Cycle Play Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GameCyclePlay1P0 =
            new CategoryVersionInformation((int)MessageCategory.GameCyclePlay, 1, 0);

        /// <summary>
        /// Theme Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ThemeStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.ThemeStore, 1, 0);

        /// <summary>
        /// Payvar Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation PayvarStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.PayvarStore, 1, 0);

        /// <summary>
        /// Coplayer History Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation CoplayerHistoryStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.CoplayerHistoryStore, 1, 0);

        /// <summary>
        /// Shell History Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellHistoryStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellHistoryStore, 1, 0);

        /// <summary>
        /// Action Request Lite Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ActionRequestLite1P0 =
            new CategoryVersionInformation((int)MessageCategory.ActionRequestLite, 1, 0);

        /// <summary>
        /// Bank Play Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation BankPlay1P0 =
            new CategoryVersionInformation((int)MessageCategory.BankPlay, 1, 0);

        /// <summary>
        /// Game Play Status Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GamePlayStatus1P0 =
            new CategoryVersionInformation((int)MessageCategory.GamePlayStatus, 1, 0);

        /// <summary>
        /// Game Play Store Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GamePlayStore1P0 =
            new CategoryVersionInformation((int)MessageCategory.GamePlayStore, 1, 0);

        /// <summary>
        /// Show Demo Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShowDemo1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShowDemo, 1, 0);

        /// <summary>
        /// Game Presentation Behavior Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GamePresentationBehavior1P0 =
            new CategoryVersionInformation((int)MessageCategory.GamePresentationBehavior, 1, 0);

        /// <summary>
        /// Game Presentation Behavior Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation GamePresentationBehavior1P1 =
            new CategoryVersionInformation((int)MessageCategory.GamePresentationBehavior, 1, 1);

        /// <summary>
        /// Shell History Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ShellHistoryControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.ShellHistoryControl, 1, 0);

        /// <summary>
        /// Theme API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ThemeApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.ThemeApiControl, 1, 0);

        /// <summary>
        /// Theme Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ThemeActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.ThemeActivation, 1, 0);

        /// <summary>
        /// Theme Activation Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation ThemeActivation1P1 =
            new CategoryVersionInformation((int)MessageCategory.ThemeActivation, 1, 1);

        /// <summary>
        /// TSM API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation TsmApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.TsmApiControl, 1, 0);

        /// <summary>
        /// TSM Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation TsmActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.TsmActivation, 1, 0);

        /// <summary>
        /// System API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation SystemApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.SystemApiControl, 1, 0);

        /// <summary>
        /// System Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation SystemActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.SystemActivation, 1, 0);

        /// <summary>
        /// Ascribed Shell Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation AscribedShellActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.AscribedShellActivation, 1, 0);

        /// <summary>
        /// Ascribed Game API Control Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation AscribedGameApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.AscribedGameApiControl, 1, 0);

        /// <summary>
        /// App API Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation AppApiControl1P0 =
            new CategoryVersionInformation((int)MessageCategory.AppApiControl, 1, 0);

        /// <summary>
        /// App Activation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation AppActivation1P0 =
            new CategoryVersionInformation((int)MessageCategory.AppActivation, 1, 0);

        /// <summary>
        /// Inspection Report Category, version 1.1.
        /// </summary>
        private static readonly CategoryVersionInformation ReportGameDataInspection1P1 =
            new CategoryVersionInformation((int)MessageCategory.ReportGameDataInspection, 1, 1);

        /// <summary>
        /// Inspection Report Category, version 1.2.
        /// </summary>
        private static readonly CategoryVersionInformation ReportGameDataInspection1P2 =
            new CategoryVersionInformation((int)MessageCategory.ReportGameDataInspection, 1, 2);

        /// <summary>
        /// Inspection Report Category, version 1.3.
        /// </summary>
        private static readonly CategoryVersionInformation ReportGameDataInspection1P3 =
            new CategoryVersionInformation((int)MessageCategory.ReportGameDataInspection, 1, 3);

        /// <summary>
        /// Game Level Award Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation GameLevelAward1P0 =
            new CategoryVersionInformation((int)MessageCategory.GameLevelAward, 1, 0);

        /// <summary>
        /// Game Performance Report Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation ReportGamePerformance1P0 =
            new CategoryVersionInformation((int)MessageCategory.ReportGamePerformance, 1, 0);

        /// <summary>
        /// Setup Validation Category, version 1.0.
        /// </summary>
        private static readonly CategoryVersionInformation SetupValidation1P0 =
            new CategoryVersionInformation((int)MessageCategory.SetupValidation, 1, 0);

        #endregion Supported Message Category Versions

        #region Public Methods

        /// <summary>
        /// Gets the list of supported link control categories.
        /// </summary>
        public static IEnumerable<CategoryRequest> GetSupportedLinkControlList()
        {
            return new List<CategoryRequest>
                       {
                           new CategoryRequest(LinkControlCategory1P1, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentJSeriesMps),
                                               dependencies => new LinkControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).LinkControlCategoryCallbacks)),

                           new CategoryRequest(LinkControlCategory1P2, FoundationTarget.AscentKSeriesCds.AndHigher(),
                                               dependencies => new LinkControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).LinkControlCategoryCallbacks)),
                       };
        }

        /// <summary>
        /// Get the list of supported categories for the given negotiation level.
        /// </summary>
        /// <param name="negotiationLevel">The negotiation level of supported categories to get.</param>
        /// <returns>
        /// The list of supported categories for the given negotiation level.
        /// </returns>
        public static IEnumerable<CategoryRequest> GetSupportedApiList(CategoryNegotiationLevel negotiationLevel)
        {
            IEnumerable<CategoryRequest> result;

            switch(negotiationLevel)
            {
                case CategoryNegotiationLevel.Link:
                    result = CreateSupportedLinkApiList();
                    break;

                case CategoryNegotiationLevel.System:
                    result = CreateSupportedSystemApiList();
                    break;

                case CategoryNegotiationLevel.Theme:
                    result = CreateSupportedThemeApiList();
                    break;

                case CategoryNegotiationLevel.Tsm:
                    result = CreateSupportedTsmApiList();
                    break;

                case CategoryNegotiationLevel.Shell:
                    result = CreateSupportedShellApiList();
                    break;

                case CategoryNegotiationLevel.Coplayer:
                    result = CreateSupportedCoplayerApiList();
                    break;

                case CategoryNegotiationLevel.AscribedGame:
                    result = CreateSupportedAscribedGameApiList();
                    break;

                case CategoryNegotiationLevel.App:
                    result = CreateSupportedAppApiList();
                    break;

                default:
                    result = new List<CategoryRequest>();
                    break;
            }

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Creates a list of Link API control supported categories.
        /// </summary>
        /// <returns>Supported Link API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedLinkApiList()
        {
            // The order of the following items matches the order in the MessageCategory enum.
            // Please keep the foundation target range at the same line of the message category version,
            // as this makes searching easier.
            return new List<CategoryRequest>
                       {
                           // Common Categories

                           new CategoryRequest(ActionRequest1P0, FoundationTarget.AllAscent,
                                               dependencies => new ActionRequestCategory(
                                                   dependencies.Transport,
                                                   new ActionRequestCallbackHandler(CastToNegotiationDependencies(dependencies).TransactionCallbacks))),

                           new CategoryRequest(Activation1P0, FoundationTarget.AllAscent,
                                               dependencies => new ActivationCategory(
                                                   dependencies.Transport,
                                                   new ActivationCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(ProgressiveData1P0, FoundationTarget.AllAscent,
                                               dependencies => new ProgressiveDataCategory(dependencies.Transport)),

                           new CategoryRequest(CustomConfigurationRead1P1, FoundationTarget.AllAscent,
                                               dependencies => new CustomConfigurationReadCategory(dependencies.Transport)),

                           new CategoryRequest(GameInformation1P1, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentJSeriesMps),
                                               dependencies => new GameInformationCategory(dependencies.Transport)),

                           new CategoryRequest(GameInformation1P2,
                                               FoundationTarget.AscentKSeriesCds.AndHigher(),
                                               dependencies => new GameInformationCategory(dependencies.Transport)),

                           new CategoryRequest(Localization1P1, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentKSeriesCds),
                                               dependencies => new LocalizationCategory(dependencies.Transport)),

                           new CategoryRequest(Localization1P2, FoundationTarget.AscentMSeries.AndHigher(),
                                               dependencies => new LocalizationCategory(dependencies.Transport)),

                           new CategoryRequest(EgmConfigData1P3, FoundationTarget.AscentHSeriesCds,
                                               dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                           new CategoryRequest(EgmConfigData1P4, FoundationTarget.AscentISeriesCds.UpTo(FoundationTarget.AscentQ2Series),
                                               dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                           new CategoryRequest(EgmConfigData1P5, true, FoundationTarget.AscentQ3Series.UpTo(FoundationTarget.AscentR1Series),
                                               dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                           new CategoryRequest(EgmConfigData1P6, true, FoundationTarget.AscentR2Series.AndHigher(),
                                               dependencies => new EgmConfigDataCategory(dependencies.Transport)),

                           new CategoryRequest(NonTransactionalCritDataRead1P2, FoundationTarget.AllAscent,
                                               dependencies => new NonTransactionalCritDataReadCategory(dependencies.Transport)),

                           new CategoryRequest(CultureRead1P0, FoundationTarget.AscentN03Series.AndLower(),
                                               dependencies => new CultureReadCategory(
                                                   dependencies.Transport,
                                                   new CultureReadCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(CultureRead1P2, FoundationTarget.AscentP1Dynasty.AndHigher(),
                               dependencies => new CultureReadCategory(
                                   dependencies.Transport,
                                   new CultureReadCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(TransactionalCritDataRead1P1, FoundationTarget.AllAscent,
                                               dependencies => new TransactionalCritDataReadCategory(dependencies.Transport)),

                           new CategoryRequest(TransactionalCritDataWrite1P0, FoundationTarget.AllAscent,
                                               dependencies => new TransactionalCritDataWriteCategory(dependencies.Transport)),

                           new CategoryRequest(GameGroupInformation1P0, FoundationTarget.AllAscent,
                                               dependencies => new GameGroupInformationCategory(dependencies.Transport)),

                           new CategoryRequest(ParcelComm1P1, FoundationTarget.AscentJSeriesMps.UpTo(FoundationTarget.AscentJSeriesMps),
                                               dependencies => new ParcelCommCategory(
                                                   dependencies.Transport,
                                                   new ParcelCommCallbackHandler(dependencies.EventCallbacks,
                                                                                 dependencies.NonTransactionalEventCallbacks))),

                           new CategoryRequest(ParcelComm1P2, FoundationTarget.AscentKSeriesCds,
                                               dependencies => new ParcelCommCategory(
                                                   dependencies.Transport,
                                                   new ParcelCommCallbackHandler(dependencies.EventCallbacks,
                                                                                 dependencies.NonTransactionalEventCallbacks))),

                           new CategoryRequest(ParcelComm1P3, FoundationTarget.AscentMSeries.AndHigher(),
                                               dependencies => new ParcelCommCategory(
                                                   dependencies.Transport,
                                                   new ParcelCommCallbackHandler(dependencies.EventCallbacks,
                                                                                 dependencies.NonTransactionalEventCallbacks))),

                           new CategoryRequest(BankStatus1P0, FoundationTarget.AscentKSeriesCds,
                                               dependencies => new BankStatusCategory(
                                                   dependencies.Transport,
                                                   new BankStatusCallbacks(dependencies.EventCallbacks))),

                           new CategoryRequest(BankStatus1P1, FoundationTarget.AscentMSeries.AndHigher(),
                                               dependencies => new BankStatusCategory(
                                                   dependencies.Transport,
                                                   new BankStatusCallbacks(dependencies.EventCallbacks))),

                           // F2L/F2B Categories
                           new CategoryRequest(ShellApiControl1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                               dependencies => new ShellApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).ShellApiControlCategoryCallbacks)),

                           new CategoryRequest(CoplayerApiControl1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                               dependencies => new CoplayerApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).CoplayerApiControlCategoryCallbacks)),

                           new CategoryRequest(SessionManagement1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                               dependencies => new SessionManagementCategory(dependencies.Transport)),

                           new CategoryRequest(ActionRequestLite1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ActionRequestLiteCategory(
                                                 dependencies.Transport,
                                                 new ActionRequestLiteCallbackHandler(dependencies.EventCallbacks))),

                           // F2E Categories
                           new CategoryRequest(ThemeApiControl1P0, FoundationTarget.AllAscent,
                                               dependencies => new ThemeApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).ThemeApiControlCategoryCallbacks)),

                           new CategoryRequest(TsmApiControl1P0, FoundationTarget.AllAscent,
                                               dependencies => new TsmApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).TsmApiControlCategoryCallbacks)),

                           new CategoryRequest(SystemApiControl1P0, FoundationTarget.AllAscent,
                                               dependencies => new SystemApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).SystemApiControlCategoryCallbacks)),

                           new CategoryRequest(TiltControl1P0, FoundationTarget.AscentHSeriesCds.UpTo(FoundationTarget.AscentJSeriesMps),
                                               dependencies => new TiltControlCategory(
                                                   dependencies.Transport,
                                                   new TiltControlCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(TiltControl1P2, FoundationTarget.AscentKSeriesCds.UpTo(FoundationTarget.AscentQ2Series),
                                               dependencies => new TiltControlCategory(
                                                   dependencies.Transport,
                                                   new TiltControlCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(TiltControl1P3, FoundationTarget.AscentQ3Series.AndHigher(),
                                               dependencies => new TiltControlCategory(
                                                   dependencies.Transport,
                                                   new TiltControlCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(AscribedGameApiControl1P0, FoundationTarget.AscentMSeries.AndHigher(),
                                               dependencies => new AscribedGameApiControlCategory(
                                                   dependencies.Transport,
                                                   CastToNegotiationDependencies(dependencies).AscribedGameApiControlCategoryCallbacks)),

                           new CategoryRequest(AppApiControl1P0, FoundationTarget.AscentQ3Series.AndHigher(),
                                                dependencies => new AppApiControlCategory(
                                                    dependencies.Transport,
                                                    CastToNegotiationDependencies(dependencies).AppApiControlCategoryCallbacks)),

                            // F2R Categories

                            new CategoryRequest(ReportGameDataInspection1P1, FoundationTarget.AscentMSeries.AndLower(),
                                           dependencies => new ReportGameDataInspectionCategory(
                                               dependencies.Transport,
                                               new ReportGameDataInspectionCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(ReportGameDataInspection1P2, FoundationTarget.AscentN01Series.UpTo(FoundationTarget.AscentQ3Series),
                                               dependencies => new ReportGameDataInspectionCategory(
                                                   dependencies.Transport,
                                                   new ReportGameDataInspectionCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(ReportGameDataInspection1P3, FoundationTarget.AscentR1Series.AndHigher(),
                                               dependencies => new ReportGameDataInspectionCategory(
                                                   dependencies.Transport,
                                                   new ReportGameDataInspectionCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(GameLevelAward1P0, FoundationTarget.AllAscent,
                                               dependencies => new GameLevelAwardCategory(
                                                   dependencies.Transport,
                                                   new GameLevelAwardCallbackHandler(dependencies.EventCallbacks,
                                                                                     dependencies.NonTransactionalEventCallbacks))),

                           new CategoryRequest(ReportGamePerformance1P0, FoundationTarget.AllAscent,
                                               dependencies => new ReportGamePerformanceCategory(
                                                   dependencies.Transport,
                                                   new ReportGamePerformanceCallbackHandler(dependencies.EventCallbacks))),

                           new CategoryRequest(SetupValidation1P0, FoundationTarget.AllAscent,
                                               dependencies => new SetupValidationCategory(
                                                   dependencies.Transport,
                                                   new SetupValidationCallbackHandler(dependencies.EventCallbacks))),
                       };
        }

        /// <summary>
        /// Creates a list of System API control supported categories.
        /// </summary>
        /// <returns>Supported System API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedSystemApiList()
        {
            yield return new CategoryRequest(SystemActivation1P0, FoundationTarget.AllAscent,
                                             dependencies => new SystemActivationCategory(
                                                 dependencies.Transport,
                                                 new SystemActivationCallbackHandler(dependencies.EventCallbacks)));
        }

        /// <summary>
        /// Creates a list of Theme API control supported categories.
        /// </summary>
        /// <returns>Supported Theme API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedThemeApiList()
        {
            yield return new CategoryRequest(ThemeActivation1P0, FoundationTarget.AscentHSeriesCds,
                                             dependencies => new ThemeActivationCategory(
                                                 dependencies.Transport,
                                                 new ThemeActivationCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(ThemeActivation1P1, FoundationTarget.AscentISeriesCds.AndHigher(),
                                             dependencies => new ThemeActivationCategory(
                                                 dependencies.Transport,
                                                 new ThemeActivationCallbackHandler(dependencies.EventCallbacks)));
        }

        /// <summary>
        /// Creates a list of TSM API control supported categories.
        /// </summary>
        /// <returns>Supported TSM API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedTsmApiList()
        {
            yield return new CategoryRequest(TsmActivation1P0, FoundationTarget.AllAscent,
                                             dependencies => new TsmActivationCategory(
                                                 dependencies.Transport,
                                                 new TsmActivationCallbackHandler(dependencies.EventCallbacks)));
        }

        /// <summary>
        /// Creates a list of Shell API control supported categories.
        /// </summary>
        /// <returns>Supported Shell API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedShellApiList()
        {
            yield return new CategoryRequest(Localization1P2, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new LocalizationCategory(dependencies.Transport));

            yield return new CategoryRequest(CultureRead1P2, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new CultureReadCategory(
                                                 dependencies.Transport,
                                                 new CultureReadPlatformCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(DisplayControl1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new DisplayControlCategory(
                                                 dependencies.Transport,
                                                 new DisplayControlCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(ChooserServices1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ChooserServicesCategory(
                                                 dependencies.Transport,
                                                 new ChooserServicesCallbackHandler(
                                                     dependencies.NonTransactionalEventCallbacks)));

            yield return new CategoryRequest(ShellActivation1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShellActivationCategory(
                                                 dependencies.Transport,
                                                 new ShellActivationCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(CoplayerManagement1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new CoplayerManagementCategory(dependencies.Transport));

            yield return new CategoryRequest(ShellThemeControl1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShellThemeControlCategory(dependencies.Transport));

            yield return new CategoryRequest(ShellStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShellStoreCategory(dependencies.Transport));

            yield return new CategoryRequest(ShellHistoryStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShellHistoryStoreCategory(
                                                 dependencies.Transport,
                                                 new ShellHistoryStoreCallbackHandler(
                                                     dependencies.EventCallbacks)));

            yield return new CategoryRequest(BankPlay1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new BankPlayCategory(
                                                 dependencies.Transport,
                                                 new BankPlayCallbackHandler(
                                                     dependencies.EventCallbacks,
                                                     dependencies.NonTransactionalEventCallbacks)));

            yield return new CategoryRequest(ShowDemo1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShowDemoCategory(dependencies.Transport));

            yield return new CategoryRequest(GamePlayStatus1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new GamePlayStatusCategory(
                                                 dependencies.Transport,
                                                 new GamePlayStatusCallbackHandler(
                                                     dependencies.EventCallbacks)));

            yield return new CategoryRequest(GamePresentationBehavior1P0, FoundationTarget.AscentP1Dynasty.UpTo(FoundationTarget.AscentQ2Series),
                                             dependencies => new GamePresentationBehaviorCategory(dependencies.Transport));

            yield return new CategoryRequest(GamePresentationBehavior1P1, FoundationTarget.AscentQ3Series.AndHigher(),
                                             dependencies => new GamePresentationBehaviorCategory(dependencies.Transport));

            yield return new CategoryRequest(ShellHistoryControl1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ShellHistoryControlCategory(dependencies.Transport));

            yield return new CategoryRequest(TiltControl1P2, FoundationTarget.AscentP1Dynasty.UpTo(FoundationTarget.AscentQ2Series),
                                             dependencies => new TiltControlCategory(
                                                 dependencies.Transport,
                                                 new TiltControlCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(TiltControl1P3, FoundationTarget.AscentQ3Series.AndHigher(),
                                             dependencies => new TiltControlCategory(
                                                 dependencies.Transport,
                                                 new TiltControlCallbackHandler(dependencies.EventCallbacks)));
        }

        /// <summary>
        /// Creates a list of Coplayer API control supported categories.
        /// </summary>
        /// <returns>Supported Coplayer API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedCoplayerApiList()
        {
            yield return new CategoryRequest(CoplayerActivation1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new CoplayerActivationCategory(
                                                 dependencies.Transport,
                                                 new CoplayerActivationCallbackHandler(
                                                     dependencies.EventCallbacks)));

            yield return new CategoryRequest(GameCyclePlay1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new GameCyclePlayCategory(
                                                 dependencies.Transport,
                                                 new GameCyclePlayCallbackHandler(dependencies.NonTransactionalEventCallbacks)));

            yield return new CategoryRequest(GameCycleBetting1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new GameCycleBettingCategory(dependencies.Transport));

            yield return new CategoryRequest(ThemeStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new ThemeStoreCategory(dependencies.Transport));

            yield return new CategoryRequest(PayvarStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new PayvarStoreCategory(dependencies.Transport));

            yield return new CategoryRequest(CoplayerHistoryStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new CoplayerHistoryStoreCategory(dependencies.Transport));

            yield return new CategoryRequest(GamePlayStore1P0, FoundationTarget.AscentP1Dynasty.AndHigher(),
                                             dependencies => new GamePlayStoreCategory(dependencies.Transport));
        }

        /// <summary>
        /// Creates a list of AscribedGame API control supported categories.
        /// </summary>
        /// <returns>Supported AscribedGame API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedAscribedGameApiList()
        {
            yield return new CategoryRequest(ThemeActivation1P1, FoundationTarget.AscentMSeries.AndHigher(),
                                             dependencies => new ThemeActivationCategory(
                                                 dependencies.Transport,
                                                 new ThemeActivationCallbackHandler(dependencies.EventCallbacks)));

            yield return new CategoryRequest(AscribedShellActivation1P0, FoundationTarget.AscentMSeries.AndHigher(),
                                             dependencies => new AscribedShellActivationCategory(
                                                 dependencies.Transport,
                                                 new AscribedShellActivationCallbackHandler(
                                                     dependencies.EventCallbacks)));
        }

        /// <summary>
        /// Creates a list of App API control supported categories.
        /// </summary>
        /// <returns>Supported App API list.</returns>
        private static IEnumerable<CategoryRequest> CreateSupportedAppApiList()
        {
            yield return new CategoryRequest(AppActivation1P0, FoundationTarget.AscentQ3Series.AndHigher(),
                                             dependencies => new AppActivationCategory(
                                                 dependencies.Transport,
                                                 new AppActivationCallbackHandler(dependencies.EventCallbacks)));
            yield return new CategoryRequest(DisplayControl1P0, FoundationTarget.AscentQ3Series.AndHigher(),
                                             dependencies => new DisplayControlCategory(
                                                 dependencies.Transport,
                                                 new DisplayControlCallbackHandler(dependencies.EventCallbacks)));
            yield return new CategoryRequest(ChooserServices1P0, FoundationTarget.AscentQ3Series.AndHigher(),
                                             dependencies => new ChooserServicesCategory(
                                                 dependencies.Transport,
                                                 new ChooserServicesCallbackHandler(
                                                     dependencies.NonTransactionalEventCallbacks)));
        }

        /// <summary>
        /// Casts an <see cref="ICategoryCreationDependencies"/> to an <see cref="ICategoryNegotiationDependencies"/>.
        /// </summary>
        /// <param name="dependencies">The object to cast from.</param>
        /// <returns>The object after a successful casting.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when the casting failed.
        /// </exception>
        private static ICategoryNegotiationDependencies CastToNegotiationDependencies(ICategoryCreationDependencies dependencies)
        {
            var negotiationDependencies = dependencies as ICategoryNegotiationDependencies;

            if(negotiationDependencies == null)
            {
                throw new ApplicationException("Failed to cast the ICategoryCreationDependencies object into an ICategoryNegotiationDependencies.");
            }

            return negotiationDependencies;
        }

        #endregion Private Methods
    }
}