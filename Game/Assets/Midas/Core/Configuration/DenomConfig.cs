using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.Configuration
{
	public sealed class DenomConfig
	{
		public Money CurrentDenomination { get; }
		public IReadOnlyList<Money> AvailableDenominations { get; }
		public Money TokenValue { get; }
		public bool IsConfirmationRequired { get; }

		public DenomConfig(Money currentDenomination, IReadOnlyList<Money> availableDenominations, Money tokenValue, bool isConfirmationRequired)
		{
			CurrentDenomination = currentDenomination;
			AvailableDenominations = availableDenominations.ToArray();
			TokenValue = tokenValue;
			IsConfirmationRequired = isConfirmationRequired;
		}

		public override string ToString()
		{
			return $"{CurrentDenomination}, '{string.Join(", ", AvailableDenominations)}', Confirm:{IsConfirmationRequired}";
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex)
		{
			serializeComplex(writer, CurrentDenomination);
			serializeComplex(writer, AvailableDenominations);
			serializeComplex(writer, TokenValue);
			serializeComplex(writer, IsConfirmationRequired);
		}

		public static DenomConfig Deserialize(BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var currentDenomination = (Money)deserializeComplex(reader);
			var availableDenominations = (IReadOnlyList<Money>)deserializeComplex(reader);
			var tokenValue = (Money)deserializeComplex(reader);
			var isConfirmationRequired = (bool)deserializeComplex(reader);

			return new DenomConfig(currentDenomination, availableDenominations, tokenValue, isConfirmationRequired);
		}
	}
}