// -----------------------------------------------------------------------
//  <copyright file = "UtpController.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable once CheckNamespace
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Communications;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UtilityObjects;

    /// <summary>
    /// The main controller and coordinator for all UTP actions.
    /// </summary>
    [ModuleEvent("PauseStatusUpdated", "bool Paused, bool CloseSocketOnPause", "Event occurs if the game has paused or resumed")]
    [ModuleEvent("GameShutdown", "void", "Event occurs if the game is being shut down")]
    public class UtpController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Hard coded version of the UTP framework.
        /// </summary>
        private const string version = "4.1.1";

        #endregion Constants

        #region Attributes

        /// <summary>
        /// Static UtpController used to ensure only 1 instance exists
        /// </summary>
        private static UtpController instance;

        /// <summary>
        /// List of modules available for use in UTP.
        /// </summary>
        /// <remarks>
        /// Don't serialize this field because it will be erased/repopulated at runtime on the first connection anyway.
        /// </remarks>
        [NonSerialized]
        [HideInInspector]
        public List<AutomationModule> TestModules = new List<AutomationModule>();

        /// <summary>
        /// The connection to send data from.
        /// </summary>
        public IUtpConnection Connection;

        /// <summary>
        /// Initializes UTP when starting the game.
        /// </summary>
        public bool InitializeOnStart = true;

        /// <summary>
        /// Reinitialize modules when a scene is loaded
        /// </summary>
        public bool InitializeModulesOnSceneLoaded = true;

        /// <summary>
        /// Will close the socket so other games can use the same port when the game has been shelved
        /// </summary>
        public bool CloseSocketOnPause = true;

        /// <summary>
        /// Restricts access to local connections.
        /// </summary>
        /// <remarks>
        /// Hidden in inspector to reduce clutter. Update this to restrict connection as
        /// you would with automation services.
        /// </remarks>
        [HideInInspector]
        public string RestrictToAddress = "";

        /// <summary>
        /// Port number to use with websocket server.
        /// </summary>
        public int Port = 5780;

        /// <summary>
        /// Indicates if UTP services should be available. UTP is available in Development mode, or
        /// when secured by the FI when installed on the EGM.
        /// </summary>
        private bool? utpServices;

        /// <summary>
        /// Flag used to determine if modules have been initialized yet.
        /// </summary>
        private bool modulesInitialized;

        /// <summary>
        /// The queue for incoming automation commands.
        /// </summary>
        private readonly Queue<AutomationCommander> commandQueue = new Queue<AutomationCommander>();

        /// <summary>
        /// Collection of all game states that is populated during "Get Modules".
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private string[] allGameStates;

        /// <summary>
        /// Returns the collection of all game states that is populated during "Get Modules".
        /// </summary>
        public IReadOnlyCollection<string> AllGameStates
        {
            get => allGameStates;
            set => allGameStates = value == null ? null : value.ToArray();
        }

        /// <summary>
        /// Set when the game starts for the first time
        /// </summary>
        private bool gameStarted;

        /// <summary>
        /// The game paused status
        /// </summary>
        [HideInInspector]
        public bool GamePaused { get; private set; }

        /// <summary>
        /// FI path information.
        /// </summary>
        private const string fiPath = @"C:\IGTRuntime\Features";

        /// <summary>
        /// FI name.
        /// </summary>
        private const string fiName = "*TTTEST*";

        /// <summary>
        /// The regulator FI name.
        /// </summary>
        private const string fiNameReg = "*TTREG*";

        /// <summary>
        /// FI actual file name.
        /// </summary>
        private const string fiFileName = "RNG.exe";

        /// <summary>
        /// List of module commands.
        /// </summary>
        private List<ModuleCommand> commands;

        /// <summary>
        /// List of module events.
        /// </summary>
        private List<ModuleEvent> events;

        #endregion

        #region Controller Processing

        /// <summary>
        /// Verifies that UTP can run.
        /// </summary>
        /// <returns>True if in development mode or secured by the FI.</returns>
        private bool SecurityVerification()
        {
            if (utpServices.HasValue)
            {
                return utpServices.Value;
            }

            if (Debug.isDebugBuild)
            {
                utpServices = true;
                return true;
            }

            try
            {
                // Only load the service if the FI is installed.
                // NOTE: the FI is the only planned Feature package
                //   that will be allowed to be installed for the
                //   Ascent Platform
                if (!Directory.Exists(fiPath))
                {
                    utpServices = false;
                    return false;
                }

                var fiComms = new FiCommunicator();

                //  If TEST FI is found, UTP should be enabled unless explicitly set to Disabled through the FI
                var testFiDir = Directory.GetDirectories(fiPath, fiName).FirstOrDefault();
                if (testFiDir != null && File.Exists(Path.Combine(testFiDir, fiFileName)))
                {
                    utpServices = fiComms.GetUtpEnabled() != FiCommunicator.UtpFiStatus.Disabled;
                    return utpServices.Value;
                }

                //  If REGULATORY FI is found, UTP should only be enabled if it's explicitly set to Enabled through the test FI
                var regFiDir = Directory.GetDirectories(fiPath, fiNameReg).FirstOrDefault();
                if (regFiDir != null && File.Exists(Path.Combine(regFiDir, fiFileName)))
                {
                    utpServices = fiComms.GetUtpEnabled() == FiCommunicator.UtpFiStatus.Enabled;
                    return utpServices.Value;
                }

                //  No TEST or REG FI paths were found, so disable all together
                utpServices = false;
                return false;
            }
            catch
            {
                //  Something exploded, so play it safe and disable UTP
                utpServices = false;
                return false;
            }
        }

        /// <summary>
        /// Uses reflection to refresh the module listing.
        /// </summary>
        /// <param name="force">True will force a reload and reflection to run again. False means reflection occurs only if no modules are found.</param>
        public void RefreshModuleList(bool force)
        {
            if(!SecurityVerification())
                return;

            if(!force && TestModules != null && TestModules.Any())
                return;

            //  Any modules that were previously initialized aren't anymore, so reset this flag
            modulesInitialized = false;

            UnloadModules();
            TestModules = new List<AutomationModule>();

            //  Use reflection to get all AutomationModule types, create instances of each, and add them to the TestModules list
            var utpTypes =
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => !assembly.ReflectionOnly && !assembly.IsDynamic)
                    .SelectMany(assembly =>
                        assembly.GetExportedTypes()
                            .Where(t => t.IsSubclassOf(typeof(AutomationModule)) && !t.IsAbstract))
                    .ToList();
            foreach(var utpType in utpTypes)
            {
                try
                {
                    var automationModule = ScriptableObject.CreateInstance(utpType) as AutomationModule;
                    if(automationModule != null)
                    {
                        automationModule.ModuleStatus = ModuleStatuses.Found;
                        TestModules.Add(automationModule);
                    }
                }
                catch(NullReferenceException nullReferenceException)
                {
                    Debug.LogWarning(string.Format("UTP: Unable to create ScriptableObject from {0}:\r\n{1}", utpType.Name, nullReferenceException));
                }
            }
            
            //  PreInitialize all modules... this needs to occur here b/c components like GameStatesConsumer need to be attached in the Editor
            foreach (var module in TestModules)
            {
                try
                {
                    module.PreInitialize();
                }
                catch (Exception ex)
                {
                    Debug.Log("UTP: An error occurred during UTP module PreInitialize. Error: " + ex);
                    module.ModuleStatus = ModuleStatuses.Error;
                }
            }

            Debug.Log("UTP module list refreshed. Found " + TestModules.Count + " modules.");
        }

        /// <summary>
        /// Sends an event command to all subscribers.
        /// </summary>
        /// <param name="command">The command to send.</param>
        private void SendEventCommand(AutomationCommand command)
        {
            var module = command.Module;

            var subscribers = UtpEventSubscriber.UtpSubscriptions.ContainsKey(module)
                ? UtpEventSubscriber.UtpSubscriptions[module].GetSubscribers(command.Command)
                : null;

            if(subscribers != null)
            {
                foreach(IUtpCommunication subscriber in subscribers)
                {
                    subscriber.Send(command);
                }
            }
        }

        /// <summary>
        /// Processes all automation commands
        /// </summary>
        /// <param name="sender">The client it came from</param>
        /// <param name="data">The contents of the command received</param>
        private void AutomationCommandReceived(object sender, AutomationCommandArgs data)
        {
            var webSocketConnectionSender = sender as WebSocketConnection;
            var command = new AutomationCommander(webSocketConnectionSender, data.Command)
            {
                ConnectionType = UtpConnectionTypes.Websocket,
                Communication = webSocketConnectionSender
            };

            if(GamePaused)
            {
                if(!CloseSocketOnPause)
                {
                    // We can't run on update or use coroutines while paused
                    while(commandQueue.Count > 0)
                    {
                        // Process any residual requests
                        var automationCommander = commandQueue.Dequeue();
                        if(!ReferenceEquals(automationCommander, null) && !ReferenceEquals(automationCommander.AutoCommand, null))
                        {
                            ProcessCommand(automationCommander);
                        }
                    }

                    ProcessCommand(command);
                }
                else
                {
                    var pausedCommand = new AutomationCommand("UtpController", "GamePaused", null);
                    if(!ReferenceEquals(webSocketConnectionSender, null))
                    {
                        webSocketConnectionSender.Send(pausedCommand);
                    }
                }
            }
            else
            {
                commandQueue.Enqueue(command);
            }
        }

        /// <summary>
        /// Creates or opens a websocket connection
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void OpenConnection()
        {
            if(!SecurityVerification())
                return;

            if(ReferenceEquals(Connection, null))
            {
                Connection = new WebSocketLib();
            }

            if(!ReferenceEquals(Connection, null) && !Connection.IsOpen)
            {
                Connection.Connect(RestrictToAddress, Port);

                // Setup delegate to add a command to the Queue.
                Connection.AutomationCommandReceived += AutomationCommandReceived;

                foreach(var mod in TestModules.Where(m => !ReferenceEquals(m, null)))
                {
                    mod.Communications = Connection;
                }
                if(InitializeOnStart && !gameStarted)
                {
                    InitializeModules(Connection);
                    gameStarted = true;
                }

                // System is up and listening for incoming connections.
                Debug.Log("UTP Listening");
                utpServices = true;
            }
            else if(!ReferenceEquals(Connection, null) && Connection.IsOpen)
            {
                Debug.Log("UTP already listening...");
            }
        }

        /// <summary>
        /// Closes all connections including the receiving client socket
        /// </summary>
        private void CloseConnection()
        {
            if(!ReferenceEquals(Connection, null))
            {
                Debug.Log("Closing connection...");
                Connection.AutomationCommandReceived -= AutomationCommandReceived;
                Connection.Close();
            }
        }

        /// <summary>
        /// Processes the automation command triggered by the UTPCommunication Event.
        /// </summary>
        /// <param name="commander">The commander.</param>
        public void ProcessCommand(AutomationCommander commander)
        {
            // Only process if the automation services are available.
            if(!SecurityVerification())
                return;

            try
            {
                //  With force=false, this ensures modules will only be loaded once during runtime
                RefreshModuleList(false);

                if(!modulesInitialized)
                {
                    InitializeModules(null);
                    modulesInitialized = true;
                }

                // Process built in commands and return.
                if(commander.AutoCommand.Module.Equals("UtpController"))
                {
                    var cmd = commander.AutoCommand.Command;
                    if(cmd.ToLower().Equals("gethelp"))
                    {
                        GetHelp(commander.Communication);
                    }
                    else if(cmd.ToLower().Equals("initializemodules"))
                    {
                        InitializeModules(commander.Communication);
                    }
                    else if(cmd.ToLower().Equals("reinitializemodules"))
                    {
                        ReinitializeModules(commander.Communication);
                    }
                    else if(cmd.ToLower().Equals("getstatusinfo"))
                    {
                        GetStatusInfo(commander.Communication);
                    }
                    else if(cmd.ToLower().Equals("getmoduleinfos"))
                    {
                        GetModuleInfos(commander.Communication);
                    }
                    else if(cmd.ToLower().Equals("setmultimodulepriority"))
                    {
                        SetMultiModulePriority(commander.AutoCommand, commander.Communication);
                    }
                    else if(commander.AutoCommand.IsEvent)
                    {
                        UtpEventSubscriber.Subscribe(commander);
                    }
                    return;
                }

                //  Any modules that have the same AutomationModule.Name (i.e. sharing the same Base class) may be prioritized
                //  If the MultiModulePrioritizer handles command execution, it means we're done and can return
                //  If not, then multi-module processing didn't apply OR we're falling back to standard processing, so continue as normal
                if (MultiModulePrioritizer.HandlePrioritizedExecution(TestModules, commander))
                    return;

                //  Get all modules with names matching the received command
                var matchingModules = TestModules.Where(tm => tm != null && tm.Name == commander.AutoCommand.Module).ToList();

                //  If no module was found with a matching name, stop processing & let the client know
                if(!matchingModules.Any())
                {
                    SendModuleNotFound(commander.Communication, commander.AutoCommand.Module);
                    return;
                }

                // If a matching ENABLED module was found, execute the command
                var enabledModule = matchingModules.FirstOrDefault(m => m.ModuleStatus == ModuleStatuses.InitializedEnabled);
                if (enabledModule != null)
                {
                    enabledModule.Execute(commander);
                    return;
                }

                //  Handle special case of the Reinitialize command
                if (commander.AutoCommand.Command == "Reinitialize")
                {
                    // Attempt to reinitialize all modules with this name
                    foreach(var module in matchingModules)
                    {
                        try
                        {
                            //  Note:  AutomationModule.Reinitialize sets the ModuleStatus, so we don't need to do it here unless it fails
                            module.Reinitialize(commander.AutoCommand, commander.Communication);
                        }
                        catch(Exception)
                        {
                            module.ModuleStatus = ModuleStatuses.Error;
                        }
                    }
                    return;
                }

                //  Getting here means the module is disabled
                SendModuleDisabled(commander.Communication, commander.AutoCommand.Module);
            }
            catch(Exception ex)
            {
                //  Deal with exceptions here by logging to the Unity log & forwarding it to the client
                var errorMsg = "UTP handled an exception that occurred during ProcessCommand. The game may have an outdated module listing. Error details: " + ex;
                Debug.LogWarning(errorMsg);
                SendCannotProcess(commander.Communication, commander.AutoCommand.Module, errorMsg);
            }
        }

        /// <summary>
        /// Unloads all modules and removes all module references from the unity scene.
        /// </summary>
        private void UnloadModules()
        {
            if(TestModules != null)
                TestModules.Clear();

            //  Destroy all AutomationModule objects
            var automationModules = Resources.FindObjectsOfTypeAll(typeof (AutomationModule));
            foreach(var automationModule in automationModules)
            {
                try
                {
                    ((AutomationModule)automationModule).Dispose();
                }
                catch(Exception)
                {
                    //Suppress this message until null checks are added to all module's UnregisterEventHandlers()
                    //Debug.LogWarning(string.Format("UTP: An error occured while disposing {0}. {1}", ((AutomationModule)automationModule).Name, ex.Message));
                }
#if UNITY_EDITOR
                DestroyImmediate(automationModule);
#else
            Destroy(automationModule);
#endif
            }
        }

        /// <summary>
        /// Will insert the UTP controller into game and build the modules. This is used in the build process
        /// </summary>
        public void InsertUtp()
        {
            //  Check if there's already a UTP GameObject in the scene
            var utpController = (UtpController)FindObjectsOfType(typeof(UtpController)).FirstOrDefault();
            if(utpController != null)
            {
                Debug.Log("Existing UtpController found in scene. Refreshing module list & storing game states.");
                utpController.RefreshModuleList(true);
                return;
            }

            //  There's not a UTP GameObj in the scene, so Instantiate one now
            utpController = Resources.FindObjectsOfTypeAll(typeof(UtpController)).First() as UtpController;
            if(utpController != null)
            {
                utpController.RefreshModuleList(true);
                var utpInstance = Instantiate(utpController);
                utpInstance.name = "UnityTestPortal";
                Debug.Log("UTP inserted, module list is up to date, and game states are stored.");
                return;
            }

            Debug.LogError("Unable to find UTP Controller to add to game");
        }

        /// <summary>
        /// Sends a "ModuleDisabled" command to the sender, used when someone tries to send a command to a disabled module
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="moduleName">The module</param>
        private static void SendModuleDisabled(IUtpCommunication sender, string moduleName)
        {
            var paramsList = new List<AutomationParameter>
            {
                new AutomationParameter("Module", moduleName, "Text", "Name of disabled module")
            };
            var disabledCommand = new AutomationCommand(moduleName, "ModuleDisabled", paramsList);

            sender.Send(disabledCommand);
        }

        /// <summary>
        /// Sends a "ModuleNotFound" command to the sender, used when someone tries to send a command to a module that isn't loaded
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="moduleName">The module</param>
        private static void SendModuleNotFound(IUtpCommunication sender, string moduleName)
        {
            var paramsList = new List<AutomationParameter>
            {
                new AutomationParameter("Module", moduleName, "Text", "Name of the module")
            };
            var moduleNotFoundCommand = new AutomationCommand(moduleName, "ModuleNotFound", paramsList);

            sender.Send(moduleNotFoundCommand);
        }

        /// <summary>
        /// Sends a "CannotProcess" command to the sender, used to notify that an error occurred during command processing.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="moduleName">The module</param>
        /// <param name="error">The error that caused the shutdown</param>
        public static void SendCannotProcess(IUtpCommunication sender, string moduleName, string error)
        {
            var paramsList = new List<AutomationParameter>
            {
                new AutomationParameter("Module", moduleName),
                new AutomationParameter("Error", error)
            };
            var cannotProcessCommand = new AutomationCommand(moduleName, "CannotProcess", paramsList);

            sender.Send(cannotProcessCommand);
        }

        #endregion

        #region MonoBehaviour overrides

        /// <summary>
        /// Ensure that only 1 instance of this class exists.
        /// </summary>
        private void Awake()
        {
            if(instance != null && instance != this)
            {
                Debug.LogWarning("UTP: Only a single UtpController is allowed. Destroying instance from '" + gameObject.scene.name + "' scene.");
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        /// <summary>
        /// Initializes the connection to a specified connection address.
        /// </summary>
        private void Start()
        {
            if(SecurityVerification())
            {
                Invoke("OpenConnection", 0.0f);

#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
                {
                    if(UnityEditor.EditorApplication.isPaused != GamePaused)
                    {
                        OnApplicationPause(UnityEditor.EditorApplication.isPaused);
                    }
                };
#endif
                SceneManager.sceneLoaded += SceneManagerSceneLoaded;
            }
            else
            {
                if(!ReferenceEquals(TestModules, null))
                {
                    UnloadModules();
                    TestModules.Clear();
                    TestModules = null;
                }

                Destroy(gameObject);
            }
        }


        /// <summary>
        /// When a scene is loaded, reinitialize all modules so they have access to
        /// the most recent game objects
        /// </summary>
        private void SceneManagerSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if(InitializeModulesOnSceneLoaded)
            {
                InitializeModules(Connection);
            }
        }

        /// <summary>
        /// Detects if the application has paused.
        /// </summary>
        /// <param name="pausedStatus">The paused status.</param>
        private void OnApplicationPause(bool pausedStatus)
        {
            if(!SecurityVerification())
                return;

            GamePaused = pausedStatus;

            if(!pausedStatus && gameStarted && CloseSocketOnPause)
            {
#if DEVELOPMENT_BUILD
                OpenConnection();       // With development builds OnApplicationPause isn't on the main thread so Invoke can't be used
#else
                if(Application.isPlaying)
                {
                    Invoke("OpenConnection", 0.0f);
                }
#endif
            }

            // Send event
            var parameters = new List<AutomationParameter>
            {
                new AutomationParameter("Paused", pausedStatus.ToString(), "bool", "The paused status"),
                new AutomationParameter("CloseSocketOnPause", CloseSocketOnPause.ToString(), "bool", "Indicates is the game socket closes on pause")
            };

            var command = new AutomationCommand("UtpController", "PauseStatusUpdated", "Event occurs if the game has paused or resumed", "bool", parameters, true);
            try
            {
                SendEventCommand(command);
            }
            catch(Exception ex)
            {
                Debug.Log("Error sending UTP PauseStatusUpdated command: " + ex.Message);
            }

            if(pausedStatus && CloseSocketOnPause)
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Cleans up when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            CloseConnection();
            SceneManager.sceneLoaded -= SceneManagerSceneLoaded;
        }


        /// <summary>
        /// Runs when the game is shutdown nicely
        /// </summary>
        private void OnApplicationQuit()
        {
            if(!SecurityVerification())
                return;

            SceneManager.sceneLoaded -= SceneManagerSceneLoaded;

            SendEventCommand(new AutomationCommand("UtpController", "GameShutdown", "Game is shutting down", "void", null, true));

            CloseConnection();
        }

        /// <summary>
        /// Process one command from the queue.
        /// </summary>
        private void Update()
        {
            //This intentionally only processes one command per update.
            if(commandQueue.Count > 0)
            {
                var ac = commandQueue.Dequeue();
                if(!ReferenceEquals(ac, null) && !ReferenceEquals(ac.AutoCommand, null))
                {
                    ProcessCommand(ac);
                }
            }
        }

        #endregion

        #region Controller Commands

        /// <summary>
        /// Initializes the modules, if they don't initialize it'll disable the module
        /// </summary>
        /// <param name="sender">The client</param>
        // ReSharper disable once RedundantArgumentDefaultValue
        [ModuleCommand("InitializeModules", "string[] Enabled, string[] Disabled, string[] Errored", "Initializes the modules. If they don't initialize they will be disabled", null)]
        public void InitializeModules(IUtpCommunication sender)
        {
            var parameters = new List<AutomationParameter>();
            foreach(var module in TestModules.Where(m => m != null))
            {
                if(module.ModuleStatus <= ModuleStatuses.Found || module.ForceInitialize)
                {
                    try
                    {
                        var initSuccessful = module.Initialize();
                        module.ModuleStatus = initSuccessful ? ModuleStatuses.InitializedEnabled : ModuleStatuses.InitializedDisabled;
                    }
                    catch
                    {
                        module.ModuleStatus = ModuleStatuses.Error;
                    }
                }

                parameters.Add(new AutomationParameter(module.ModuleStatus == ModuleStatuses.InitializedEnabled ? "Enabled" : "Disabled", module.Name));

                if(module.ModuleStatus == ModuleStatuses.Error)
                {
                    parameters.Add(new AutomationParameter("Errored", module.Name));
                }
            }

            if(sender != null)
            {
                sender.Send(new AutomationCommand("UtpController", "InitializeModules", parameters));
            }
        }

        /// <summary>
        /// Initializes the modules, if they don't initialize it'll disable the module
        /// </summary>
        /// <param name="sender">The client</param>
        // ReSharper disable once RedundantArgumentDefaultValue
        [ModuleCommand("ReinitializeModules", "string[] Enabled, string[] Disabled, string[] Errored", "Initializes the modules. If they don't initialize they will be disabled", null)]
        public void ReinitializeModules(IUtpCommunication sender)
        {
            var parameters = new List<AutomationParameter>();
            foreach(var module in TestModules)
            {
                try
                {
                    var initSuccessful = module.Initialize();
                    module.ModuleStatus = initSuccessful ? ModuleStatuses.InitializedEnabled : ModuleStatuses.InitializedDisabled;
                }
                catch
                {
                    module.ModuleStatus = ModuleStatuses.Error;
                }

                parameters.Add(new AutomationParameter(module.ModuleStatus == ModuleStatuses.InitializedEnabled ? "Enabled" : "Disabled", module.Name));

                if(module.ModuleStatus == ModuleStatuses.Error)
                {
                    parameters.Add(new AutomationParameter("Errored", module.Name));
                }
            }

            if(sender != null)
            {
                sender.Send(new AutomationCommand("UtpController", "ReinitializeModules", parameters));
            }
        }

        /// <summary>
        /// Sends details about all modules and commands to the sender
        /// </summary>
        /// <param name="sender">The client</param>
        // ReSharper disable once RedundantArgumentDefaultValue
        [ModuleCommand("GetHelp", "AutomationCommand", "Gets details about all modules and commands", null)]
        // ReSharper disable once MemberCanBePrivate.Global
        public void GetHelp(IUtpCommunication sender)
        {
            if(sender == null)
                return;

            var helpResult = new AutomationCommand("UtpController", "GetHelp", new List<AutomationParameter>());
            helpResult.Parameters.Add(new AutomationParameter("Version", version, "string", "The UtpController version."));

            // Get all UTP commands
            var controllerCommands = new AutomationCommand("UtpController", "GetHelp", new List<AutomationParameter>());
            controllerCommands.Parameters.Add(new AutomationParameter("ModuleStatus", "Enabled", "Text", "The status of the module."));

            foreach(var command in CommandsList)
            {
                var parameters = !ReferenceEquals(command.Parameters, null)
                    ? command.Parameters.Select(param => new AutomationParameter(param)).ToList() : new List<AutomationParameter>();

                var ac = new AutomationCommand( "UtpController", command.Command, command.Description, command.Returns, parameters);
                controllerCommands.Parameters.Add(new AutomationParameter(command.Command, AutomationCommand.Serialize(ac, true)) { Type = "object" });
            }

            foreach(var mEvent in EventsList)
            {
                var ac = new AutomationCommand("UtpController", mEvent.EventName, mEvent.Description, mEvent.EventArgs, null, true);
                controllerCommands.Parameters.Add(new AutomationParameter(mEvent.EventName, AutomationCommand.Serialize(ac, true)) { Type = "object" });
            }

            helpResult.Parameters.Add(new AutomationParameter("UtpController", AutomationCommand.Serialize(controllerCommands, true)) { Type = "object" });

            // Get all the module commands.
            if(TestModules != null)
            {
                foreach(var module in TestModules.Where(m => m != null))
                {
                    var moduleCommand = GetModuleCommandDetails(module);
                    helpResult.Parameters.Add(new AutomationParameter(module.Name, AutomationCommand.Serialize(moduleCommand, true)) { Type = "object" });
                }
            }
            sender.Send(helpResult);
        }

        /// <summary>
        /// Gets various info regarding UTP's status in the game. Primarily meant for diagnostic purposes.
        /// </summary>
        /// <param name="sender">The client</param>
        [ModuleCommand("GetStatusInfo", "string[] Info", "Gets various info regarding UTP's status in the game.")]
        public void GetStatusInfo(IUtpCommunication sender)
        {
            var param = new AutomationParameter("Info");
            try
            {
                var utps = FindObjectsOfType<UtpController>().ToList();
                param.Value = "Found UtpController in " + utps.Count + " scenes. ThisScene=" + gameObject.scene.name + " AllScenes=" + string.Join(", ", utps.Select(u => u.gameObject.scene.name).ToArray());
            }
            catch (Exception ex)
            {
                param.Value = "Error getting UTP status info. Ex: " + ex;
            }

            if (sender != null)
            {
                sender.Send(new AutomationCommand("UtpController", "GetStatusInfo", new List<AutomationParameter> { param }));
            }
        }

        /// <summary>
        /// Gets additional information about modules that isn't included in GetHelp.  This command won't be used frequently
        /// and is purposefully separate from GetHelp to avoid bloating it since it has a high usage rate.
        /// </summary>
        /// <param name="sender">The client</param>
        // ReSharper disable once RedundantArgumentDefaultValue
        [ModuleCommand("GetModuleInfos", "ModuleInfo[] ModuleInfos", "Gets additional information about all modules.")]
        public void GetModuleInfos(IUtpCommunication sender)
        {
            if (sender == null || TestModules == null)
                return;

            var paramsList = TestModules.Select(module => new AutomationParameter("ModuleInfos", new ModuleInfo(module))).ToList();

            sender.Send(new AutomationCommand("UtpController", "GetModuleInfos", paramsList));
        }

        /// <summary>
        /// Sets priority when multiple enabled modules have the same AutomationModule.Name value for an incoming command.
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="sender">The client</param>
        [ModuleCommand("SetMultiModulePriority", "bool Success", "Sets which module to use when multiple enabled modules of the same type exist.",
            new[] {"FullNamespace|string|The full namespace (namespace + class name) of the module to be prioritized. Passing an empty string disables prioritization.",
                "ModuleName|string|The AutomationModule.Name property of the module to be prioritized. Passing an empty string disables prioritization.",
                "RevertToNext|bool|When the prioritized module is missing or disabled, this flag determines if we should revert to the next enabled module OR error out." })]
        public void SetMultiModulePriority(AutomationCommand command, IUtpCommunication sender)
        {
            if (sender == null || TestModules == null)
                return;

            //  Read parameters
            var errorMessage = AutomationParameter.GetIncomingParameterValidationErrors(command.Parameters, new List<string> { "FullNamespace", "ModuleName", "RevertToNext" });
            if (!string.IsNullOrEmpty(errorMessage))
            {
                SendCannotProcess(sender, "UtpController", errorMessage);
                return;
            }

            //  Get all modules with a matching namespace & module name
            var classFullNamespace = command.Parameters.First(p => p.Name == "FullNamespace").Value;
            var moduleName = command.Parameters.First(p => p.Name == "ModuleName").Value;
            var revertToNext = false;

            if (string.IsNullOrEmpty(classFullNamespace) || string.IsNullOrEmpty(moduleName))
            {
                //  Namespace and/or moduleName is missing, so assume the caller wants to toggle off module prioritization
                classFullNamespace = null;
                moduleName = null;
            }
            else
            {
                var matchingModules = TestModules.Where(m =>
                {
                    var fullName = m.GetType().FullName;
                    return fullName != null && fullName.Equals(classFullNamespace, StringComparison.InvariantCultureIgnoreCase) && m.Name == moduleName;
                }).ToList();

                if (matchingModules.Count != 1)
                {
                    SendCannotProcess(sender, "UtpController",
                        "Expected 1 module with class name " + classFullNamespace + " and module name " + moduleName + ", but found " + matchingModules.Count + ".");
                    return;
                }

                //  Parse the RevertToNext behavior... if parsing fails, then the safest thing is to treat failed-to-find-prioritized-module scenarios as failures
                if (!bool.TryParse(command.Parameters.First(p => p.Name == "RevertToNext").Value, out revertToNext))
                    revertToNext = true;
            }

            //  We've safely determined all parameter values... assign them now
            MultiModulePrioritizer.PrioritizedNamespace = classFullNamespace;
            MultiModulePrioritizer.PrioritizedModuleName = moduleName;
            MultiModulePrioritizer.RevertToNextOnFailure = revertToNext;

            var paramsList = new List<AutomationParameter> { new AutomationParameter("Success", true) };
            sender.Send(new AutomationCommand("UtpController", "SetMultiModulePriority", paramsList));
        }

        #endregion Controller Commands

        #region Private Methods

        /// <summary>
        /// Gets the ModuleCommand details of a module item.
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <returns>An AutomationCommand describing the module</returns>
        private AutomationCommand GetModuleCommandDetails(AutomationModule mod)
        {
            var moduleStatus = mod.ModuleStatus == ModuleStatuses.Error ? "Error" : mod.ModuleStatus == ModuleStatuses.InitializedEnabled ? "Enabled" : "Disabled";

            var moduleCommand = new AutomationCommand(mod.Name, "GetHelp", new List<AutomationParameter>());
            moduleCommand.Parameters.Add(new AutomationParameter("ModuleStatus", moduleStatus, "Text", "The status of the module."));
            moduleCommand.Parameters.Add(new AutomationParameter("ModuleVersion", mod.ModuleVersion.ToString(), "Version", "The version of the module."));

            // module commands
            foreach(var command in mod.Commands)
            {
                var prms = command.Parameters.Select(param => new AutomationParameter(param)).ToList();
                var automationCommand = new AutomationCommand(mod.Name, command.Command, command.Description, command.Returns, prms);
                moduleCommand.Parameters.Add(new AutomationParameter(command.Command, AutomationCommand.Serialize(automationCommand, true)) { Type = "object" });
            }

            // module events
            foreach(ModuleEvent mEvent in mod.Events)
            {
                var automationCommand = new AutomationCommand(mod.Name, mEvent.EventName, mEvent.Description, mEvent.EventArgs, new List<AutomationParameter>(), true);
                moduleCommand.Parameters.Add(new AutomationParameter(mEvent.EventName, AutomationCommand.Serialize(automationCommand, true)) { Type = "object" });
            }

            return moduleCommand;
        }

        /// <summary>
        /// Gets the commands for modules.
        /// </summary>
        /// <value>
        /// The commands in the module.
        /// </value>
        protected virtual List<ModuleCommand> CommandsList
        {
            get
            {
                if(!ReferenceEquals(commands, null))
                {
                    return commands;
                }

                commands = UtpModuleUtilities.GetModuleCommands(GetType());

                return commands;
            }
        }

        /// <summary>
        /// Gets the events for modules.
        /// </summary>
        /// <value>
        /// The events in the module.
        /// </value>
        protected virtual List<ModuleEvent> EventsList
        {
            get
            {
                if(!ReferenceEquals(events, null))
                {
                    return events;
                }

                events = UtpModuleUtilities.GetModuleEvents(GetType());
                return events;
            }
        }

        #endregion Private Methods
    }
}