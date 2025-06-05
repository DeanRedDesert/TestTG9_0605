namespace Logic.Types
{
	public sealed class RespinPrize
	{
		public string SymbolName { get; }
		public int SymbolIndex { get; }
		public bool IsBonus { get; }
		public bool IsProgressive { get; }
		public string PrizeName { get; }
		public ulong Value { get; }

		public RespinPrize(string symbolName, int symbolIndex, bool isBonus, bool isProgressive, string prizeName, ulong value)
		{
			SymbolName = symbolName;
			SymbolIndex = symbolIndex;
			IsBonus = isBonus;
			IsProgressive = isProgressive;
			PrizeName = prizeName;
			Value = value;
		}
	}
}