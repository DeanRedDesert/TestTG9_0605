using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IGT.Game.Utp.Framework;
using IGT.Game.Utp.Framework.Communications;
using Logic.Core.Engine;
using Logic.Core.Utility;
using Midas.Core.Serialization;
using Midas.Gle.Logic;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;

namespace Midas.Gaff.Utp
{
	/// <summary>
	/// Required so the request to get the GLE gaff game data can be sent/received from a coroutine and not block the calling methods.
	/// </summary>
	public sealed class GleGaffGetGameStateHandler : MonoBehaviour
	{
		public void WaitForData(AutomationCommand command, IUtpCommunication sender, AutomationModule module) => StartCoroutine(WaitForGameState(command, sender, module));

		private static IEnumerator WaitForGameState(AutomationCommand command, IUtpCommunication sender, AutomationModule automationModule)
		{
			GleDialUpData gleGaffData = null;
			Communication.PresentationDispatcher.AddHandler<RequestGaffDataResponse>(OnGaffDataResponse);
			Communication.ToLogicSender.Send(new RequestGaffDataMessage());
			while (gleGaffData == null)
				yield return null;

			Communication.PresentationDispatcher.RemoveHandler<RequestGaffDataResponse>(OnGaffDataResponse);

			var paramsList = new List<AutomationParameter>
			{
				new AutomationParameter("Inputs", AsSerializedString(gleGaffData.Inputs), "string"),
				new AutomationParameter("InitialStage", gleGaffData.InitialStageName, "string"),
				new AutomationParameter("Config", AsSerializedString(gleGaffData.GameConfiguration), "string"),
				new AutomationParameter("Decisions", AsSerializedString(gleGaffData.CycleData), "string"),
				new AutomationParameter("InterGameData", AsSerializedString(gleGaffData.InterGameData), "string")
			};

			automationModule.SendCommand(command.Command, paramsList, sender);

			void OnGaffDataResponse(RequestGaffDataResponse obj) => gleGaffData = (GleDialUpData)obj.DialUpData;
		}

		private static string AsSerializedString(Inputs inputs) => string.Join("|!|", inputs.Select(i => StringConverter.TryToString(i, "SL", out var sl) ? sl : "NA"));
		private static string AsSerializedString(IReadOnlyDictionary<string, string> gameConfig) => string.Join("|!|", gameConfig.Select(kv => $"{kv.Key} {kv.Value}"));
		private static string AsSerializedString(IReadOnlyList<IReadOnlyList<ulong>> rng) => string.Join("|!|", rng.Select(cd => string.Join(" ", cd)));

		private static string AsSerializedString(IReadOnlyDictionary<string, object> interGameData)
		{
			var cs = new List<ICustomSerializer> { new GleCustomSerializers() };
			var bs = new BinarySerializer(cs, new List<Type>());

			var s = string.Join("|!|", interGameData.Select(igd =>
			{
				using var serializationStream = new MemoryStream();
				var bw = new BinaryWriter(serializationStream);
				bs.Serialize(bw, igd.Value);
				return igd.Key + "|#|" + Convert.ToBase64String(serializationStream.ToArray());
			}));

			var tms = string.Join("|#|", bs.TypeMap.Select(t => t.AssemblyQualifiedName));

			return $"{tms}|!|{s}";
		}
	}
}