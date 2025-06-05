using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Midas.Core.General
{
	public sealed class ListSegment<T> : IReadOnlyList<T>
	{
		private readonly IReadOnlyList<T> list;
		private readonly int offset;

		public ListSegment(IReadOnlyList<T> list, int offset, int count)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), "Argument must be non-negative");
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Argument must be non-negative");
			if (list.Count - offset < count)
				throw new ArgumentException("Offset and length go out of range of the list");

			this.list = list;
			this.offset = offset;
			Count = count;
		}

		public IEnumerator<T> GetEnumerator() => list.Skip(offset).Take(Count).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count { get; }

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));
				return list[offset + index];
			}
		}
	}
}