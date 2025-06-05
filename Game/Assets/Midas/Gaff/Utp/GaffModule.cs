using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using Logic.Core.Utility;
using Midas.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Gaff.Utp
{
	/// <summary>
	/// UTP module to allow access to the gaffing features.
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Module commands are discovered via reflection")]
	public sealed class GaffModule : AutomationModule
	{
		#region AutomationModule Overrides

		public override string Name => "Gaff";

		public override bool Initialize() => true;

		#endregion

		/// <summary>
		/// Gets last game state.
		/// </summary>
		[ModuleCommand("SetAutoPlayState", "void", "Set the state of automatic play.", new[] { "Enabled|bool|Enable or disable autoplay.", "AddCredits|bool|Enable or disable adding credits." })]
		[SuppressMessage("ReSharper", "UnusedParameter.Global")]
		public bool SetAutoPlayState(AutomationCommand command, IUtpCommunication sender)
		{
			var enabled = false;
			var autoAddCredits = false;

			if (bool.TryParse(command.Parameters[0].Value, out var e))
				enabled = e;

			if (bool.TryParse(command.Parameters[1].Value, out var a))
				autoAddCredits = a;

			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() =>
			{
				var gs = StatusDatabase.GaffStatus;
				gs.IsSelfPlayActive = enabled;
				gs.IsSelfPlayAddCreditsActive = autoAddCredits;
			}));

			return true;
		}

		[ModuleCommand("GetGaffList", "string []Gaffs", "Get the collection of available gaffs.")]
		public bool GetGaffList(AutomationCommand command, IUtpCommunication sender)
		{
			var automationParameters = new List<AutomationParameter> { new AutomationParameter("Gaffs", string.Join(",", StatusDatabase.GaffStatus.GaffSequences), "string") };
			return SendCommand(command.Command, automationParameters, sender);
		}

		[ModuleCommand("SetActiveGaff", "void", "Set the active gaff.", new[] { "Gaff Name|string|The name of the gaff to make active.", "Repeat Gaff|bool|Should the gaff repeat." })]
		public bool SetActiveGaff(AutomationCommand command, IUtpCommunication sender)
		{
			var n = command.Parameters[0].Value;
			var repeat = false;

			if (bool.TryParse(command.Parameters[1].Value, out var r))
				repeat = r;

			var index = StatusDatabase.GaffStatus.GaffSequences.IndexOf(s => s.Name == n);
			if (index == -1)
			{
				SendErrorCommand(command.Command, $"Invalid gaff sequence: {n}", sender);
				return false;
			}

			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() =>
			{
				var gs = StatusDatabase.GaffStatus;
				gs.SelectedGaffIndex = index;
				gs.RepeatSelectedGaff = repeat;
			}));

			return true;
		}

		[ModuleCommand("ClearActiveGaff", "void", "Clear the active gaff and repeat gaff flag.")]
		[SuppressMessage("ReSharper", "UnusedParameter.Global")]
		public bool ClearActiveGaff(AutomationCommand command, IUtpCommunication sender)
		{
			Communication.ToPresentationSender.Send(new DemoGaffActionMessage(() =>
			{
				var gs = StatusDatabase.GaffStatus;
				gs.SelectedGaffIndex = null;
				gs.RepeatSelectedGaff = false;
			}));
			return true;
		}
	}
}