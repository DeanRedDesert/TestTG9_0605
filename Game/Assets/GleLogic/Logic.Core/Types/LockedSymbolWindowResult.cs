using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Container class that provides symbol window and a lock mask to standardise hold and spin games.
	/// </summary>
	public sealed class LockedSymbolWindowResult : IToString
	{
		/// <summary>
		/// The symbol window result for this locked result.
		/// This result must have all the reel information that is required to correctly present populations to the player.
		/// </summary>
		public SymbolWindowResult SymbolWindowResult { get; }

		/// <summary>
		/// The lock mask for this result.
		/// Each bit represents the population that is locked as mapped by <see cref="SymbolWindowResult.SymbolWindowStructure"/>.
		/// </summary>
		public ReadOnlyMask LockMask { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public LockedSymbolWindowResult(SymbolWindowResult symbolWindowResult, ReadOnlyMask lockMask)
		{
			SymbolWindowResult = symbolWindowResult;
			LockMask = lockMask;
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				var r = StringConverter.ToString(SymbolWindowResult, "ML");

				if (r is StringSuccess s)
				{
					var list = new List<string> { "Symbol Window:" };
					list.AddRange(s.Value.ToLines().Indent(2, ' '));
					list.Add($"Lock Mask: {LockMask.ToStringOrThrow("SL")}");
					return list.Join().ToSuccess();
				}

				return r;
			}

			return new NotSupported();
		}
	}
}