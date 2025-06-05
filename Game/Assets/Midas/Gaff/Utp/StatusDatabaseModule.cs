using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using Midas.Presentation.Data;
using Midas.Logging;

namespace Midas.Gaff.Utp
{
	public sealed partial class StatusDatabaseModule : AutomationModuleBase
	{
		#region Public

		/// <summary>
		/// Gets all available status items from the StatusDatabase.
		/// </summary>
		/// <param name="command">The incoming command.</param>
		/// <param name="sender">The sender of the command.</param>
		[ModuleCommand(nameof(GetAvailableStatusItems), "string[] Items", "The names of all available status items from the StatusDatabase")]
		public bool GetAvailableStatusItems(AutomationCommand command, IUtpCommunication sender)
		{
			if (!VerifyStatusDatabaseIsInitialized(command, sender))
				return false;

			var names = new List<string>();
			foreach (var sb in StatusDatabase.StatusBlocksInstance.StatusBlocks)
				names.AddRange(sb.PropertyNames.Select(pn => sb.Name + "." + pn));

			var automationParameters = new List<AutomationParameter>
			{
				new AutomationParameter("Items", JsonConvert.SerializeObject(names), "json")
			};

			return SendCommand(command.Command, automationParameters, sender);
		}

		[ModuleCommand(nameof(GetStatusItemPropertyValue), "string Value", "The the value of the requested StatusItem property", new[] { "Path|String|The path of the status item property to query for." })]
		public bool GetStatusItemPropertyValue(AutomationCommand command, IUtpCommunication sender)
		{
			if (!VerifyIncomingParams(command, sender, "Path") || !VerifyStatusDatabaseIsInitialized(command, sender))
				return false;

			var path = NormalisePath(command.Parameters[0].Value);

			try
			{
				var item = FindStatusProperty(path);
				if (item == null)
					throw new Exception($"Could not find status: \"{command.Parameters[0].Value}\". "
						+ "A valid example would be: \"BankStatus.BankMeter\". "
						+ "Use UTP command \"GetAvailableStatusItems\" to get a full list.");

				var co = JsonConvert.SerializeObject(item, GetJsonSerializerSettings());
				var automationParameters = new List<AutomationParameter> { new AutomationParameter("Value", co, "json") };

				return SendCommand(command.Command, automationParameters, sender);
			}
			catch (Exception ex)
			{
				return SendErrorCommand(command.Command, ex.Message, sender);
			}
		}

		[ModuleCommand(nameof(DumpStatusDatabase), "string Dump", "Dumps the whole content of the StatusDatabase into a json string of Key/Values.")]
		public bool DumpStatusDatabase(AutomationCommand command, IUtpCommunication sender)
		{
			try
			{
				var dump = JsonConvert.SerializeObject(StatusDatabase.StatusBlocksInstance, GetJsonSerializerSettings());
				var automationParameters = new List<AutomationParameter> { new AutomationParameter("Dump", dump, "json") };

				return SendCommand(command.Command, automationParameters, sender);
			}
			catch (Exception ex)
			{
				return SendErrorCommand(command.Command, ex.Message, sender);
			}
		}

		[ModuleCommand(nameof(SetStatusItemValue), "bool Success", "Set the value of the requested StatusItem property",
			new[]
			{
				"Path|String|The path of the status item property to query for.",
				"Type|String|Type of Value (bool, int, float, double, string)",
				"Value|String|New Value for the status item"
			})]
		public bool SetStatusItemValue(AutomationCommand command, IUtpCommunication sender)
		{
			try
			{
				var path = NormalisePath(command.Parameters[0].Value);
				var type = command.Parameters[1].Value;
				switch (type.ToLower())
				{
					case "bool":
						bool.TryParse(command.Parameters[2].Value, out var bVal);
						SetItemValue(path, bVal);
						break;
					case "int":
						int.TryParse(command.Parameters[2].Value, out var iVal);
						SetItemValue(path, iVal);
						break;
					case "float":
						float.TryParse(command.Parameters[2].Value, out var fVal);
						SetItemValue(path, fVal);
						break;
					case "double":
						double.TryParse(command.Parameters[2].Value, out var dVal);
						SetItemValue(path, dVal);
						break;
					default:
						var sVal = command.Parameters[2].Value;
						SetItemValue(path, sVal);
						break;
				}

				return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", "True", "bool") }, sender);
			}
			catch (Exception e)
			{
				return SendErrorCommand(command.Command, e.Message);
			}
		}

		[ModuleCommand(nameof(DumpStatusDatabaseToLogFolder), "void", "Dump the StatusDatabase into the log directory.")]
		public bool DumpStatusDatabaseToLogFolder(AutomationCommand command, IUtpCommunication sender)
		{
			using (var sw = File.CreateText(Path.Combine(Factory.LogDirectory, "Utp.StatusDatabase.Dump.txt")))
			{
				var dump = JsonConvert.SerializeObject(StatusDatabase.StatusBlocksInstance, GetJsonSerializerSettings(true));
				sw.Write(dump);
			}

			return SendCommand(command.Command, new List<AutomationParameter>(), sender);
		}

		[ModuleCommand(nameof(TestDumpOnError), "void", "Logs an Fatal error which should result into a dump of StatusDatabase into the log directory.")]
		public bool TestDumpOnError(AutomationCommand command, IUtpCommunication sender)
		{
			Log.Instance.Fatal("Testing Status Database Error Dump");
			return SendCommand(command.Command, new List<AutomationParameter>(), sender);
		}

		#endregion

		#region Private

		private static IStatusProperty FindStatusProperty(string path)
		{
			IStatusProperty item = null;
			foreach (var sb in StatusDatabase.StatusBlocksInstance.StatusBlocks)
			{
				foreach (var prop in sb.Properties)
				{
					if ($"{sb.Name}.{prop.Name}" != path)
						continue;

					item = prop;
					break;
				}
			}

			return item;
		}

		private static void SetItemValue<T>(string path, T value)
		{
			var item = FindStatusProperty(path);
			if (item is IStatusProperty<T> p)
				p.Value = value;
		}

		private bool VerifyStatusDatabaseIsInitialized(AutomationCommand command, IUtpCommunication sender)
		{
			if (StatusDatabase.IsInitialised)
				return true;

			SendErrorCommand(command.Command, "StatusDatabase is not initialized.", sender);
			return false;
		}

		private static string NormalisePath(string path)
		{
			const string statusDataBase = "StatusDatabase";
			if (path.StartsWith(statusDataBase, StringComparison.OrdinalIgnoreCase))
				path = path.Substring(statusDataBase.Length);

			if (path.StartsWith(".", StringComparison.OrdinalIgnoreCase))
				path = path.Substring(1);
			return path;
		}

		private static JsonSerializerSettings GetJsonSerializerSettings(bool isPretty = false)
		{
			var settings = new JsonSerializerSettings
			{
				Converters = new List<JsonConverter>
				{
					new StatusBlockConverter(),
					new StatusItemValueConverter()
				},
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = isPretty ? Formatting.Indented : Formatting.None
			};
			return settings;
		}

		#endregion
	}
}