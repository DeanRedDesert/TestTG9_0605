// -----------------------------------------------------------------------
// <copyright file = "GameReportBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Communication;
    using Interfaces;
    using StandardReportLib = Communication.Foundation.Standard.ReportLib;
    using StandaloneReportLib = Communication.Foundation.Standalone.ReportLib;

    /// <summary>
    /// Primary class for game reporting that communicates with the Foundation
    /// through <see cref="IReportLib"/>.
    /// </summary>
    /// <devdoc>
    /// To add support on a new reporting service,
    /// 1. Create a new virtual method and let the derived class to create the concrete service handler;
    /// 2. In the method <see cref="InitializeServices"/>, construct the new service by the new service
    ///    handler created by the derived class in step 1;
    /// 3. Add this new service to the collection <see cref="reportingServices"/>.
    /// </devdoc>
    public abstract class GameReportBase
    {
        /// <summary>
        /// The foundation target this game report is built for.
        /// Used for debugging purpose only.
        /// </summary>
        private FoundationTarget foundationTarget;

        /// <summary>
        /// The restricted game report interface used to connect with the Foundation
        /// and process events.
        /// </summary>
        private IReportLibRestricted reportLibRestricted;

        /// <summary>
        /// Whether the report lib is actively communicating with and
        /// processing events from the Foundation.
        /// </summary>
        private bool isRunning;

        /// <summary>
        /// Event that is triggered when <see cref="isRunning"/> changes.
        /// </summary>
        private readonly ManualResetEvent isRunningChangedEvent = new ManualResetEvent(false);

        /// <summary>
        /// The list of reporting services that will be supported.
        /// </summary>
        private List<IReportingService> reportingServices;

        /// <summary>
        /// Gets the game report interface to the Foundation.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected IReportLib ReportLib { get; private set; }

        #region Abstract/Virtual Members

        /// <summary>
        /// Creates the service handler for itemized game data inspection.
        /// </summary>
        /// <remarks>
        /// The base class returns null to indicate that <see cref="ReportingServiceType.GameDataInspection"/>
        /// is not supported. Derived classes should override it to return a valid service handler if
        /// this reporting service will be supported.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        protected virtual IGameDataInspectionServiceHandler CreateGameDataInspectionServiceHandler(IReportLib reportLib)
        {
            return null;
        }

        /// <summary>
        /// Creates the service handler for Html based game data inspection.
        /// </summary>
        /// <remarks>
        /// The base class returns null to indicate that <see cref="ReportingServiceType.GameDataHtmlInspection"/>
        /// is not supported. A derived class should override it to return a valid service handler if
        /// this reporting service will be supported.
        /// Note: Html reporting is supported only in foundation versions N01+.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        protected virtual IGameDataHtmlInspectionServiceHandler CreateGameDataHtmlInspectionServiceHandler(IReportLib reportLib)
        {
            return null;
        }       

        /// <summary>
        /// Creates the service handler for game level award.
        /// </summary>
        /// <remarks>
        /// The base class returns null to indicate that <see cref="ReportingServiceType.GameLevelAward"/>
        /// is not supported. Derived class should override it to return a valid service handler if
        /// the reporting service will be supported.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        protected virtual IGameLevelAwardServiceHandler CreateGameLevelAwardServiceHandler(IReportLib reportLib)
        {
            return null;
        }

        /// <summary>
        /// Creates the service handler for setup validation.
        /// </summary>
        /// <remarks>
        /// The base class returns null to indicate that <see cref="ReportingServiceType.SetupValidation"/>
        /// is not supported. Derived class should override it to return a valid service handler if
        /// the reporting service will be supported.
        /// </remarks>
        protected virtual ISetupValidationServiceHandler CreateSetupValidationServiceHandler(IReportLib reportLib)
        {
            return null;
        }

        /// <summary>
        /// Creates the service handler for game performance report.
        /// </summary>
        /// <remarks>
        /// The base class returns null to indicate that <see cref="ReportingServiceType.GamePerformance"/>
        /// is not supported. Derived class should override it to return a valid service handler if
        /// the reporting service will be supported.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        protected virtual IGamePerformanceServiceHandler CreateGamePerformanceServiceHandler(IReportLib reportLib)
        {
            return null;
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// This is needed by MPT paytable loader to add to the system's PATH var in order to find dependent unmanaged DLLs.
        /// We need to check if the PATH environment variable is already set in this process environment (it usually isn't
        /// when launched by the foundation's game manager.) If it is set then append to the end of it,
        /// else create it initially.
        /// If MPT changes its searching behavior for unmanaged dlls, we might have to modify this code as well.
        /// </summary>
        /// <param name="path">The full path to append to the existing PATH var.</param>
        private static void SetEnvironmentPathing(string path)
        {
            var existingPath = Environment.GetEnvironmentVariable(@"PATH");
            Environment.SetEnvironmentVariable(@"PATH",
                !string.IsNullOrEmpty(existingPath) ? existingPath + ";" + path : path,
                EnvironmentVariableTarget.Process);
        }

        #endregion

        #region Report Lib Processing

        /// <summary>
        /// Tells the <see cref="GameReportBase"/> to connect to the Foundation
        /// and start processing events.
        /// </summary>
        /// <param name="isStandalone">
        /// If true, uses a standalone implementation of <see cref="IReportLib"/>
        /// for development and testing without the Foundation.
        /// If false, uses a standard implementation of <see cref="IReportLib"/> that communicates
        /// with the Foundation. 
        /// </param>
        /// <param name="foundationTargetString">
        /// Case-insensitive string representing the  <see cref="IGT.Game.Core.Communication.FoundationTarget"/>
        /// used for communicating with the Foundation.
        /// </param>
        /// <remarks>
        /// This method will run synchronously until a shutdown event comes from the Foundation or
        /// an exception occurs.
        /// </remarks>
        public void Run(bool isStandalone, string foundationTargetString)
        {
            try
            {
                foundationTarget = (FoundationTarget)Enum.Parse(typeof(FoundationTarget), foundationTargetString, true);
            }
            catch(Exception exception)
            {
                throw new ArgumentException(
                    $"Cannot parse '{foundationTargetString}' as a valid value of " +
                    "IGT.Game.Core.Communication.FoundationTarget",
                                            nameof(foundationTargetString),
                                            exception);
            }

            IReportLib reportLib;

            if(isStandalone)
            {
                reportLib = new StandaloneReportLib();
            }
            else
            {
                reportLib = new StandardReportLib(foundationTarget);
            }

            Run(reportLib);
        }

        /// <summary>
        /// Tells the <see cref="GameReportBase"/> to connect to the Foundation
        /// and start processing events.
        /// </summary>
        /// <param name="reportLib">Game report interface to the Foundation.</param>
        /// <remarks>
        /// This method is internal for testing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reportLib"/> is null.
        /// </exception>
        internal void Run(IReportLib reportLib)
        {
            ReportLib = reportLib ?? throw new ArgumentNullException(nameof(reportLib), "Parameter may not be null");

            try
            {
                // Initializing the logging.
                GameReportLog.Instance.InitializeGameReportLog();

                InitializeReportLib();

                // Process Events until a Shutdown event occurs
                ProcessEvents();

                UnregisterReportLibEvents();

                reportLibRestricted.DisconnectFromFoundation();
            }
            finally
            {
                if(reportingServices != null)
                {
                    foreach(var reportingService in reportingServices)
                    {
                        reportingService.CleanUpResources(ReportLib);
                    }
                }

                GameReportLog.Instance.CleanUp();

                // Dispose objects
                foreach(var disposable in new object[] { ReportLib, isRunningChangedEvent }.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Initializes the report lib, registers for events, and connects to the Foundation.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Thrown if the current <see cref="IReportLib"/> cannot be cast to a <see cref="IReportLibRestricted"/>.
        /// </exception>
        private void InitializeReportLib()
        {
            reportLibRestricted = ReportLib as IReportLibRestricted;

            if(reportLibRestricted == null)
            {
                throw new InvalidCastException($"{ReportLib.GetType().FullName} must support IReportLibRestricted.");
            }

            InitializeServices();

            RegisterReportLibEvents();

            isRunning = reportLibRestricted.ConnectToFoundation(GetSupportedReportingServiceTypes());

            // MountPoint becomes valid only after ConnectToFoundation.
            SetEnvironmentPathing(ReportLib.MountPoint);

            if(!isRunning)
            {
                Console.WriteLine("Game Report failed to connect to Foundation.  " +
                                  "Possible reasons include (but not limited to):  " +
                                  "The report object (built for Foundation target {0}) is running on an incompatible Foundation.  " +
                                  "Please check F2R (Foundation to Report) message logs and system event logs for more information.",
                                  foundationTarget);
            }
        }

        /// <summary>
        /// Initializes the list of reporting services according to the service handlers.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="reportingServices"/> is empty after it has been initialized.
        /// </exception>
        /// <devdoc>
        /// This method should be updated to support new reporting services.
        /// </devdoc>
        private void InitializeServices()
        {
            reportingServices = new List<IReportingService>();

            var gameDataInspectionServiceHandler = CreateGameDataInspectionServiceHandler(ReportLib);
            if(gameDataInspectionServiceHandler != null)
            {
                reportingServices.Add(new GameDataInspectionService(gameDataInspectionServiceHandler));
            }

            var gameDataHtmlInspectionServiceHandler = CreateGameDataHtmlInspectionServiceHandler(ReportLib);
            if(gameDataHtmlInspectionServiceHandler != null)
            {
                reportingServices.Add(new GameDataHtmlInspectionService(gameDataHtmlInspectionServiceHandler));
            }

            var gameLevelAwardServiceHandler = CreateGameLevelAwardServiceHandler(ReportLib);
            if(gameLevelAwardServiceHandler != null)
            {
                reportingServices.Add(new GameLevelAwardService(gameLevelAwardServiceHandler));
            }

            var setupValidationServiceHandler = CreateSetupValidationServiceHandler(ReportLib);
            if(setupValidationServiceHandler != null)
            {
                reportingServices.Add(new SetupValidationService(setupValidationServiceHandler));
            }

            var gamePerformanceServiceHandler = CreateGamePerformanceServiceHandler(ReportLib);
            if(gamePerformanceServiceHandler != null)
            {
                reportingServices.Add(new GamePerformanceService(gamePerformanceServiceHandler));
            }

            if(!reportingServices.Any())
            {
                throw new InvalidOperationException("There should be at least one reporting service supported.");
            }
        }

        /// <summary>
        /// Gets the flags of reporting services that will be supported according to
        /// the <see cref="reportingServices"/>.
        /// </summary>
        /// <returns>
        /// The flags of reporting services that will be supported.
        /// </returns>
        private IEnumerable<ReportingServiceType> GetSupportedReportingServiceTypes()
        {
            return
                new HashSet<ReportingServiceType>(
                    reportingServices.Select(reportingService => reportingService.ReportingServiceType));
        }

        /// <summary>
        /// Continually processes events from the Foundation until a shutdown
        /// event comes from the Foundation.
        /// </summary>
        private void ProcessEvents()
        {
            while(isRunning)
            {
                reportLibRestricted.ProcessEvents(Timeout.Infinite, new WaitHandle[] { isRunningChangedEvent });
            }
        }

        /// <summary>
        /// Registers event handlers with the <see cref="IReportLib"/>.
        /// </summary>
        private void RegisterReportLibEvents()
        {
            foreach(var reportingService in reportingServices)
            {
                reportingService.RegisterReportLibEvents(ReportLib);
            }

            ReportLib.ShutDownEvent += HandleShutDownEvent;
        }

        /// <summary>
        /// Un-registers event handlers with the <see cref="IReportLib"/>.
        /// </summary>
        private void UnregisterReportLibEvents()
        {
            foreach(var reportingService in reportingServices)
            {
                reportingService.UnregisterReportLibEvents(ReportLib);
            }

            ReportLib.ShutDownEvent -= HandleShutDownEvent;
        }

        #endregion

        #region Report Lib Event Handlers

        /// <summary>
        /// Handles the shut down event from the Foundation by stopping
        /// events from being processed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Empty event data.</param>
        private void HandleShutDownEvent(object sender, EventArgs e)
        {
            isRunning = false;
            isRunningChangedEvent.Set();
        }

        #endregion
    }
}