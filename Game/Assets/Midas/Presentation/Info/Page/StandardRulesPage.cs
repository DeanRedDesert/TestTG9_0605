using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Data;
using UnityEngine;

namespace Midas.Presentation.Info.Page
{
	/// <summary>
	/// Uses the Game Rules Naming Convention to detect if the page should be enabled.
	/// </summary>
	public sealed class StandardRulesPage : RulesPage
	{
		private bool init;
		private readonly List<Money> denominations = new List<Money>();
		private readonly List<string> jurisdictions = new List<string>();
		private readonly List<long> minimumLines = new List<long>();
		private readonly List<long> maximumLines = new List<long>();
		private readonly List<Credit> maximumBet = new List<Credit>();
		private readonly List<long> minimumWays = new List<long>();
		private readonly List<long> maximumWays = new List<long>();

		[SerializeField]
		public SpriteRenderer spriteRenderer;

		public override bool CanEnable()
		{
			Initialise();

			var canEnable = true;
			if (denominations.Count > 0)
				canEnable &= denominations.Contains(StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination);

			if (jurisdictions.Count > 0)
				canEnable &= jurisdictions.Contains(StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction);

			Check(minimumLines, Stake.LinesBet, true);
			Check(maximumLines, Stake.LinesBet, false);
			Check(minimumWays, Stake.Multiway, true);
			Check(maximumWays, Stake.Multiway, false);

			if (maximumBet.Count > 0)
				canEnable &= maximumBet.Contains(StatusDatabase.GameStatus.StakeCombinations.Max(sc => sc.TotalBet));

			return canEnable;

			void Check(IReadOnlyCollection<long> states, Stake option, bool minimum)
			{
				if (states.Count == 0)
					return;
				var d = StatusDatabase.GameStatus.StakeCombinations.Select(sc =>
				{
					sc.Values.TryGetValue(option, out var wb);
					return wb;
				});

				var value = minimum ? d.Min() : d.Max();
				canEnable &= states.Contains(value);
			}
		}

		private void Initialise()
		{
			if (init)
				return;

			PageType = spriteRenderer.sprite.name.Contains("PAYTABLE") ? RulesPageType.Paytable : RulesPageType.Rules;
			var s = spriteRenderer.sprite.name.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
			if (s.Length <= 1)
				return;
			var items = s[1].Split(new[] { "!" }, StringSplitOptions.RemoveEmptyEntries);
			var parameters = items.SelectMany(i => i.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries));

			foreach (var param in parameters)
			{
				var item = param.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
				switch (item[0])
				{
					case "CD":
						denominations.Add(Money.FromMinorCurrency(int.Parse(item[1])));
						break;
					case "JD":
						jurisdictions.Add(item[1]);
						break;
					case "mL":
						minimumLines.Add(int.Parse(item[1]));
						break;
					case "ML":
						maximumLines.Add(int.Parse(item[1]));
						break;
					case "MB":
						maximumBet.Add(Credit.FromLong(int.Parse(item[1])));
						break;
					case "mW":
						minimumWays.Add(int.Parse(item[1]));
						break;
					case "MW":
						maximumWays.Add(int.Parse(item[1]));
						break;
				}
			}

			init = true;
		}
	}
}