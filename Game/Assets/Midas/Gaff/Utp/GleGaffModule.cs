using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using Midas.Gle.Logic;
using Midas.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Gaff.Utp
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Module commands are discovered via reflection")]
	public sealed partial class GleGaffModule : AutomationModule
	{
		#region Fields

		private bool isGaffingEnabled;
		private bool isWaitingEnabled;

		// This fields should only be updated on the presentation thread. Use Communication.ToPresentationSender.Send to make the changes.
		private readonly UtpGaffStatus utpGaffStatus = new UtpGaffStatus();
		private readonly GaffController gaffController = new GaffController();

		#endregion

		#region AutomationModule Overrides

		public override string Name => "GleGaff";

		public override bool Initialize() => true;

		public override void Dispose() => Cleanup();

		#endregion

		private GleDialUpData gleGaffData;

		/// <summary>
		/// Gets last game state.
		/// </summary>
		[ModuleCommand("GetGameState", "string Inputs, string InitialStage, string State", "Gets the latest game result.")]
		public bool GetGameState(AutomationCommand command, IUtpCommunication sender)
		{
			if (!isGaffingEnabled)
				return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "False", "bool") }, sender);

			var waiting = FindObjectOfType<GleGaffGetGameStateHandler>();
			waiting.WaitForData(command, sender, this);
			return true;
		}

		/// <summary>
		/// Sends the doubles to the EGM and sets the gaff if necessary.
		/// </summary>
		[ModuleCommand("SendULongs", "bool Success", "The ulongs were sent successfully.", new[] { "TheULongs|string|The ulongs each separated by a space. Each group of ulongs is separated by a comma." })]
		public bool SendULongs(AutomationCommand command, IUtpCommunication sender)
		{
			if (!isGaffingEnabled)
				return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "False", "bool") }, sender);

			var listOfUlongs = new List<List<ulong>>();

			foreach (var ulongsAsString in command.Parameters[0].Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
			{
				var ulongs = new List<ulong>();

				foreach (var d in ulongsAsString.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (ulong.TryParse(d, out var value))
						ulongs.Add(value);
				}

				listOfUlongs.Add(ulongs);
			}

			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() => { utpGaffStatus.GaffResults = new GleDialUpResults(listOfUlongs); }));

			var param = new AutomationParameter("Success", "True", "bool");
			return SendCommand(command.Command, new List<AutomationParameter> { param }, sender);
		}

		/// <summary>
		/// Enable UTP gaffing.
		/// </summary>
		[ModuleCommand("GaffEnabled", "bool Success", "The command executed successfully.")]
		public bool GaffEnabled(AutomationCommand command, IUtpCommunication sender)
		{
			isGaffingEnabled = true;
			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() => { StatusDatabase.AddStatusBlock(utpGaffStatus); }));
			Communication.ToPresentationSender.Send(new DemoGaffHandleMessage(gaffController, true));
			return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "True", "bool") }, sender);
		}

		/// <summary>
		/// Disable UTP gaffing.
		/// </summary>
		[ModuleCommand("GaffDisabled", "bool Success", "The command executed successfully.")]
		public bool GaffDisabled(AutomationCommand command, IUtpCommunication sender)
		{
			Cleanup();
			return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "True", "bool") }, sender);
		}

		/// <summary>
		/// Gets gaff state.
		/// </summary>
		[ModuleCommand("GetGaffState", "bool Enabled", "Gets the gaff state.")]
		public bool GetGaffState(AutomationCommand command, IUtpCommunication sender) => SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("IsGaffing", isGaffingEnabled.ToString(), "bool") }, sender);

		/// <summary>
		/// If no pre loaded result is available make the EGM wait for the next result to come from the UTP.
		/// </summary>
		[ModuleCommand("WaitEnabled", "bool Success", "The command executed successfully.")]
		public bool WaitEnabled(AutomationCommand command, IUtpCommunication sender)
		{
			isWaitingEnabled = true;
			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() => { utpGaffStatus.IsWaiting = true; }));
			return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "True", "bool") }, sender);
		}

		/// <summary>
		/// If no pre loaded result is available let the EGM generate its own results.
		/// </summary>
		[ModuleCommand("WaitDisabled", "bool Success", "The command executed successfully.")]
		public bool WaitDisabled(AutomationCommand command, IUtpCommunication sender)
		{
			isWaitingEnabled = false;
			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() =>
			{
				utpGaffStatus.IsWaiting = false;
				utpGaffStatus.IsShowing = false;
			}));
			return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "True", "bool") }, sender);
		}

		/// <summary>
		/// Gets the wait state.
		/// </summary>
		[ModuleCommand("GetWaitState", "bool Enabled", "Gets the wait state.")]
		public bool GetWaitState(AutomationCommand command, IUtpCommunication sender) => SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("IsWaiting", isWaitingEnabled.ToString(), "bool") }, sender);

		private void Cleanup()
		{
			isGaffingEnabled = false;
			isWaitingEnabled = false;

			Communication.ToPresentationSender.Send(new DemoGaffHandleMessage(gaffController, false));
			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() =>
			{
				utpGaffStatus.IsShowing = false;
				utpGaffStatus.IsWaiting = false;
				utpGaffStatus.GaffResults = null;
				StatusDatabase.RemoveStatusBlock(utpGaffStatus);
			}));
		}
	}
}