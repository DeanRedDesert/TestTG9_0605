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
using Midas.Presentation.Denom;

public sealed class CashChingStatus : StatusBlock
{
	private static IReadOnlyDictionary<Money, IReadOnlyList<RespinPrize>> respinPrizes;
	private StatusProperty<IReadOnlyDictionary<string, long>> respinPrizeValues;
	private StatusProperty<Money> miniValue;
	private StatusProperty<Money> minorValue;

	public IReadOnlyDictionary<string, long> RespinPrizeValues => respinPrizeValues.Value;
	public Money MiniValue => miniValue.Value;
	public Money MinorValue => minorValue.Value;

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
		miniValue = AddProperty(nameof(MiniValue), Money.Zero);
		minorValue = AddProperty(nameof(MinorValue), Money.Zero);
	}

	protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
	{
		base.RegisterForEvents(unregisterHelper);

		unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
		unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.DenomStatus, nameof(DenomStatus.SelectedDenom), OnSelectedDenomChanged);
	}

	private void OnSelectedDenomChanged(StatusBlock sender, string propertyname)
	{
		var prizes = respinPrizes[StatusDatabase.DenomStatus.SelectedDenom];

		foreach (var prize in prizes)
		{
			if (prize.IsBonus && !prize.IsProgressive)
			{
				Money value = StatusDatabase.DenomStatus.SelectedDenom * new RationalNumber((long)prize.Value, 1);
				if (prize.SymbolName == "MINI")
				{
					miniValue.Value = value;
				}
				else if (prize.SymbolName == "MINOR")
				{
					minorValue.Value = value;
				}
			}
		}
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
