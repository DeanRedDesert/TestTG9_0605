using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using Midas.Presentation.ButtonHandling;

namespace Midas.Gaff.Utp
{
	public sealed class ButtonsModule : AutomationModuleBase
	{
		#region Fields

		private static readonly char[] buttonSeparator = { '|' };

		#endregion

		#region Module Commands

		[ModuleCommand(nameof(GetButtonMapping), "ButtonMapping", "Gets the current Id - Name mapping of all available buttons.")]
		public bool GetButtonMapping(AutomationCommand command, IUtpCommunication sender)
		{
			var mapping = ButtonHelpers.AllButtonFunctions;
			var jsonMapping = JsonConvert.SerializeObject(mapping.Select(x => x.Function), Formatting.Indented);
			var automationParameters = new List<AutomationParameter> { new AutomationParameter("ButtonMapping", jsonMapping, "json") };

			return SendCommand(command.Command, automationParameters, sender);
		}

		[ModuleCommand(nameof(EmulateButtonPressById), "void", "Emulates a button press for a button with given id.", new[] { "ButtonId|int|The id of the button to press." })]
		public bool EmulateButtonPressById(AutomationCommand command, IUtpCommunication sender)
		{
			if (!VerifyIncomingParams(command, sender, "ButtonId"))
				return false;

			var buttonIdText = command.Parameters[0].Value;
			if (!int.TryParse(buttonIdText, out var buttonId))
			{
				SendErrorCommand(command.Command, $"Invalid ButtonId: '{buttonIdText}'.", sender);
				return false;
			}

			var idx = ButtonHelpers.GetButtonFunctionIndexOfId(buttonId);
			if (idx != -1)
				return EmulateButtonPress(command, sender, idx);

			SendErrorCommand(command.Command, $"ButtonId '{buttonIdText}' not found.", sender);
			return false;
		}

		[ModuleCommand(nameof(EmulateButtonPressByName), "void", "Emulates a button press for a button with given name.", new[] { "ButtonName|string|The name of the button to press." })]
		public bool EmulateButtonPressByName(AutomationCommand command, IUtpCommunication sender)
		{
			if (!VerifyIncomingParams(command, sender, "ButtonName"))
				return false;

			var buttonName = command.Parameters[0].Value;
			if (string.IsNullOrWhiteSpace(buttonName))
			{
				SendErrorCommand(command.Command, "Empty ButtonName", sender);
				return false;
			}

			var idx = ButtonHelpers.GetButtonFunctionIndexOfName(buttonName);
			if (idx != -1)
				return EmulateButtonPress(command, sender, idx);

			SendErrorCommand(command.Command, $"ButtonName '{buttonName}' not found.", sender);
			return false;
		}

		[ModuleCommand(nameof(EmulateMultipleButtonPressByName), "void", "Emulates button presses for multiple buttons successively without delay.", new[] { "Buttons|string|A pipe separated string defining which buttons to press." })]
		public bool EmulateMultipleButtonPressByName(AutomationCommand command, IUtpCommunication sender)
		{
			if (!VerifyIncomingParams(command, sender, "Buttons"))
				return false;

			var buttonsArgument = command.Parameters[0].Value;
			if (string.IsNullOrWhiteSpace(buttonsArgument))
			{
				SendErrorCommand(command.Command, "Empty Buttons argument", sender);
				return false;
			}

			var automationParameters = new List<AutomationParameter>();
			foreach (var buttonName in buttonsArgument.Split(buttonSeparator, StringSplitOptions.RemoveEmptyEntries))
			{
				try
				{
					var idx = ButtonHelpers.GetButtonFunctionIndexOfName(buttonName);
					var buttonFunction = ButtonHelpers.AllButtonFunctions[idx];

					ButtonManager.EmulateButtonPress(buttonFunction.Function);
					var param = new AutomationParameter("Button", $"Id: {buttonFunction.Function.Id}; Name: {buttonFunction.Name}");
					automationParameters.Add(param);
				}
				catch (ArgumentOutOfRangeException)
				{
					return SendErrorCommand(command.Command, $"Tried to invoke unknown button '{buttonName}'.", sender);
				}
			}

			return SendCommand(command.Command, automationParameters, sender);
		}

		#endregion

		#region Private Methods

		private bool EmulateButtonPress(AutomationCommand command, IUtpCommunication sender, int idx)
		{
			var buttonFunction = ButtonHelpers.AllButtonFunctions[idx];
			ButtonManager.EmulateButtonPress(buttonFunction.Function);

			var automationParameters = new List<AutomationParameter> { new AutomationParameter("Button", $"Id: {buttonFunction.Function.Id}; Name: {buttonFunction.Name}") };
			return SendCommand(command.Command, automationParameters, sender);
		}

		#endregion
	}
}