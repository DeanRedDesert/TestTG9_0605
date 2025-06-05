using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using UnityEditor;
using System.Linq;

namespace Midas.Gaff.Utp
{
	public abstract class AutomationModuleBase : AutomationModule
	{
		#region Fields

		private bool eventHandlersRegistered;

		#endregion

		#region Public

		public override void Dispose() => InternalUnregisterEventHandlers();
		public override string Name => GetType().Name.Replace("Module", "");

		public override bool Initialize()
		{
			InternalUnregisterEventHandlers();
			InternalRegisterEventHandlers();

#if UNITY_EDITOR
			// There is no need to deregister following event as on exiting play mode Unity will reset the whole scene to a state before entering play mode
			// and therefore this registration never happened then ...
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
			return true;
		}

		#endregion

		#region Protected

		protected bool VerifyIncomingParams(AutomationCommand command, IUtpCommunication sender, params string[] expectedParameterNames)
		{
			var errorMessage = AutomationParameter.GetIncomingParameterValidationErrors(command.Parameters, expectedParameterNames.ToList());

			if (string.IsNullOrEmpty(errorMessage))
				return true;
			SendErrorCommand(command.Command, errorMessage, sender);
			return false;
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global - Present for expandability
		protected virtual void RegisterEventHandlers() { }

		// ReSharper disable once VirtualMemberNeverOverridden.Global - Present for expandability
		protected virtual void UnregisterEventHandlers() { }

		#endregion

		#region Private

		private void InternalRegisterEventHandlers()
		{
			if (eventHandlersRegistered)
				return;

			RegisterEventHandlers();
			eventHandlersRegistered = true;
		}

		private void InternalUnregisterEventHandlers()
		{
			if (!eventHandlersRegistered)
				return;

			UnregisterEventHandlers();
			eventHandlersRegistered = false;
		}

#if UNITY_EDITOR

		private void OnPlayModeChanged(PlayModeStateChange state)
		{
			switch (state)
			{
				case PlayModeStateChange.EnteredPlayMode:
					InternalRegisterEventHandlers();
					break;

				case PlayModeStateChange.ExitingPlayMode:
					InternalUnregisterEventHandlers();
					break;
			}
		}

#endif

		#endregion
	}
}