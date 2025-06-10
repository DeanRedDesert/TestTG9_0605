using System.Collections.Generic;
using System.Linq;
using Logic.Types;
using Midas.Gle.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Gle.Logic;
using Midas.Core.General;
using SelectorItems = Logic.Core.Types.SelectorItems;
using Midas.Presentation.Data.StatusBlocks;
using System.Diagnostics;

public sealed class CashChingStatus : StatusBlock
{
	private static IReadOnlyDictionary<Money, IReadOnlyList<RespinPrize>> respinPrizes;
	private StatusProperty<IReadOnlyDictionary<string, long>> respinPrizeValues;

	public IReadOnlyDictionary<string, long> RespinPrizeValues => respinPrizeValues.Value;

	public CashChingStatus() : base(nameof(CashChingStatus))
	{

	}

	public override void Init()
	{
		base.Init();

		if (respinPrizes == null)
			respinPrizes = ExtractRespinPrizes();
	}

	protected override void DoResetProperties()
	{
		base.DoResetProperties();
		respinPrizeValues = AddProperty(nameof(RespinPrizeValues), default(IReadOnlyDictionary<string, long>));
	}

	protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
	{
		base.RegisterForEvents(unregisterHelper);

		unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
	}

	private void OnDenomConfigChanged(StatusBlock sender, string propertyname)
	{
		var newRespinPrizes = new Dictionary<string, long>();
		var prizes = respinPrizes[StatusDatabase.ConfigurationStatus.DenomConfig.CurrentDenomination];
		foreach (var prize in prizes)
		{
			if (!prize.IsBonus && !prize.IsProgressive)
				newRespinPrizes.Add(prize.SymbolName, (long)prize.Value);
		}

		respinPrizeValues.Value = newRespinPrizes;
		UnityEngine.Debug.Log("DGS Denom Config Changed");
	}

	private static IReadOnlyDictionary<Money, IReadOnlyList<RespinPrize>> ExtractRespinPrizes()
	{
		// The respin prizes can be found in the base stage.

		var sd = GleGameData.EntryStage.StageData;
		var respinPrizeData = (SelectorItems)sd.Single(d => d.name == "data_ExcelRespinPrizes").o;

		var result = new Dictionary<Money, IReadOnlyList<RespinPrize>>();

		// The respin prizes are selected using a single selector requirement "Denom".

		for (var i = 0; i < respinPrizeData.Count; i++)
		{
			var item = respinPrizeData[i];
			var denoms = item.Requirements.Single();
			foreach (var denom in denoms.Values)
			{
				var denomMoney = (Logic.Core.Types.Money)denom;

				// Convert the denom to the internal Money type and add the prizes to the dictionary.
				result.Add(denomMoney.ToMidasMoney(), (IReadOnlyList<RespinPrize>)item.Data);
			}
		}

		return result;
	}
}
