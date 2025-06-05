using Logic.Core.Utility;

namespace Logic.Types
{
	public sealed class GeneralData : IToString
	{
		public string TargetSymbolForBaseAndFree { get; }
		public int TargetSymbolIndexForBaseAndFree { get; }
		public string TargetSymbolForRespin { get; }
		public int TargetSymbolIndexForRespin { get; }
		public int RespinSpins { get; }
		public int RespinTriggerCount { get; }

		public GeneralData(string targetSymbolForBaseAndFree, int targetSymbolIndexForBaseAndFree, string targetSymbolForRespin, int targetSymbolIndexForRespin, int respinSpins, int respinTriggerCount)
		{
			TargetSymbolForBaseAndFree = targetSymbolForBaseAndFree;
			TargetSymbolIndexForBaseAndFree = targetSymbolIndexForBaseAndFree;
			TargetSymbolForRespin = targetSymbolForRespin;
			TargetSymbolIndexForRespin = targetSymbolIndexForRespin;
			RespinSpins = respinSpins;
			RespinTriggerCount = respinTriggerCount;
		}

		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				return new[]
				{
					$"Target Symbol: {TargetSymbolForBaseAndFree} ({TargetSymbolIndexForBaseAndFree})",
					$"Target Respin Symbol: {TargetSymbolForRespin} ({TargetSymbolIndexForRespin})",
					$"Respins: {RespinSpins}",
					$"Respin Trigger Count: {RespinTriggerCount}"
				}.Join().ToSuccess();
			}

			return new NotSupported();
		}
	}
}