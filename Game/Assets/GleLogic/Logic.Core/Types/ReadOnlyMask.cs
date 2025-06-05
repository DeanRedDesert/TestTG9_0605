using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Logic.Core.Utility;

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace Logic.Core.Types
{
	public sealed class ReadOnlyMask : IEquatable<ReadOnlyMask>, IToString
	{
		#region Fields

		private const int Threshold3 = 192;
		private const int Threshold1 = 64;
		private const int ULongBits = 64;
		private const ulong AllFalse = 0UL;
		private const ulong AllTrue = ~0UL;

		private const uint Vector128ULongCount = 2;
		private const uint Vector256ULongCount = 4;

		private readonly int bitLength;
		private readonly ulong mask; // Mask to use if bitLength is <= Threshold.
		private readonly ulong[] array; // Array to use if bitLength is > Threshold.

		private readonly bool isAllFalse;
		private readonly bool isAllTrue;

		private int trueCount = -1;

		#endregion

		#region Properties

		public int BitLength => bitLength;

		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public bool IsEmpty => isAllFalse;

		public int TrueCount
		{
			get
			{
				if (isAllTrue)
					return bitLength;

				if (isAllFalse)
					return 0;

				if (trueCount == -1)
				{
					trueCount = bitLength <= Threshold1
						? PopulationCount(mask)
						: DoTrueCount();
				}

				return trueCount;
			}
		}

		#endregion

		#region Construction

		private ReadOnlyMask(int bitLength, bool defaultValue)
		{
			this.bitLength = bitLength;
			isAllFalse = !defaultValue;
			isAllTrue = defaultValue;
		}

		private ReadOnlyMask(int bitLength, ulong mask, bool? isAllFalse = null, bool? isAllTrue = null)
		{
			if (bitLength > Threshold1)
				throw new ArgumentException($"bitLength must be less than or equal to {Threshold1}", nameof(bitLength));

			this.bitLength = bitLength;
			this.mask = mask;

			// Clear the most significant bits of the mask if the bit length is not a multiple of Threshold.
			var remainderBits = bitLength & (ULongBits - 1);
			var remainderMask = remainderBits > 0 ? (1UL << remainderBits) - 1UL : AllTrue;

			this.mask &= remainderMask;
			this.isAllFalse = isAllFalse ?? this.mask == AllFalse;
			this.isAllTrue = isAllTrue ?? this.mask == (AllTrue & remainderMask);
		}

		private ReadOnlyMask(int bitLength, ulong[] array)
		{
			this.bitLength = bitLength;
			this.array = array;
			var arrayLength = array.Length;

			// Clear the most significant bits of the mask if the bit length is not a multiple of Bits.
			var remainderBits = bitLength & (ULongBits - 1);
			var remainderMask = remainderBits > 0 ? (1UL << remainderBits) - 1UL : AllTrue;
			array[arrayLength - 1] &= remainderMask;

			isAllFalse = true;
			isAllTrue = true;

			for (var i = 0; i < arrayLength; i++)
			{
				var ai = array[i];

				if (ai != AllFalse)
					isAllFalse = false;

				if (ai != (i < arrayLength - 1 ? AllTrue : AllTrue & remainderMask))
					isAllTrue = false;

				if (!isAllTrue && !isAllFalse)
					break;
			}
		}

		#endregion

		#region Create Methods

		/// <summary>
		/// Create a bit mask of length <see cref="bitLength"/> and initialise all the bits to false.
		/// </summary>
		public static ReadOnlyMask CreateAllFalse(int bitLength) => new ReadOnlyMask(bitLength, false);

		/// <summary>
		/// Create a bit mask of length <see cref="bitLength"/> and initialise all the bits to true.
		/// </summary>
		public static ReadOnlyMask CreateAllTrue(int bitLength) => new ReadOnlyMask(bitLength, true);

		/// <summary>
		/// Create a bit mask of length <see cref="bitLength"/> and initialise all the bits using the <param name="array"/>.
		/// </summary>
		public static ReadOnlyMask CreateFrom(int bitLength, ulong[] array)
		{
			return bitLength > Threshold1
				? new ReadOnlyMask(bitLength, array)
				: new ReadOnlyMask(bitLength, array[0]);
		}

		/// <summary>
		/// Create a mask from a string of '0's and '1's <param name="bitString"/>.
		/// </summary>
		public static ReadOnlyMask CreateFromBitString(string bitString)
		{
			var allFalse = true;
			var allTrue = true;
			var mask = new Mask(bitString.Length);
			var i = 0;

			foreach (var b in bitString)
			{
				if (b == '1')
				{
					mask[i] = true;
					allFalse = false;
				}
				else
					allTrue = false;

				i++;
			}

			return allFalse
				? CreateAllFalse(bitString.Length)
				: allTrue
					? CreateAllTrue(bitString.Length)
					: mask.Lock();
		}

		/// <summary>
		/// Create a mask of length <see cref="bitLength"/> and set all bits specified in <param name="indexes"/> to true.
		/// </summary>
		public static ReadOnlyMask CreateFromIndexes(int bitLength, IReadOnlyList<int> indexes)
		{
			if (indexes.Count == 0)
				return CreateAllFalse(bitLength);

			var distinct = indexes.Distinct().ToArray();

			if (distinct.Length == bitLength)
				return CreateAllTrue(bitLength);

			var mask = new Mask(bitLength);

			foreach (var i in distinct)
				mask[i] = true;

			return mask.Lock();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Get the bit at the given index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">if index less than 0 or index greater than or equal to <see cref="BitLength"/>.</exception>
		public bool this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (index < 0 || index >= bitLength)
					throw new ArgumentOutOfRangeException(nameof(index), index, null);

				if (isAllFalse)
					return false;

				if (isAllTrue)
					return true;

				if (bitLength > Threshold1)
					return (array[index >> 6] & (1UL << (index % ULongBits))) != 0;

				return (mask & (1UL << index)) != 0;
			}
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the result.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/>is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public ReadOnlyMask And(ReadOnlyMask other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (bitLength != other.bitLength)
				throw new ArgumentException("Array lengths differ");

			if (isAllFalse)
				return this;

			if (isAllTrue || trueCount == bitLength)
				return other;

			if (other.isAllFalse)
				return other;

			if (other.isAllTrue || other.trueCount == other.bitLength)
				return this;

			if (bitLength <= Threshold1)
				return new ReadOnlyMask(bitLength, mask & other.mask);

			if (bitLength <= Threshold3)
				return DoAnd3(other);

			return DoAnd(other);
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the number of true bits in the result.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public int AndTrueCount(ReadOnlyMask other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (bitLength != other.bitLength)
				throw new ArgumentException("Array lengths differ");

			if (isAllFalse || other.isAllFalse)
				return 0;

			if (isAllTrue || trueCount == bitLength)
				return other.TrueCount;

			if (other.isAllTrue || other.trueCount == other.bitLength)
				return TrueCount;

			if (bitLength <= Threshold1)
				return PopulationCount(mask & other.mask);

			if (bitLength <= Threshold3)
				return DoAndTrueCount3(other);

			return DoAndTrueCount(other);
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the number of true bits in the result.
		/// Also returns the result in the <see cref="result"/> parameter.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public int AndTrueCount(ReadOnlyMask other, out ReadOnlyMask result)
		{
			result = And(other);
			return result.TrueCount;
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the true if the result has no true bits.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public bool AndIsEmpty(ReadOnlyMask other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (bitLength != other.bitLength)
				throw new ArgumentException("Array lengths differ");

			if (isAllFalse)
				return true;

			if (isAllTrue || trueCount == bitLength)
				return other.isAllFalse;

			if (other.isAllFalse)
				return true;

			if (other.isAllTrue || other.trueCount == other.bitLength)
				return isAllFalse;

			if (bitLength <= Threshold1)
				return (mask & other.mask) == AllFalse;

			if (bitLength <= Threshold3)
				return DoAndIsEmpty3(other);

			return DoAndIsEmpty(other);
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the true if the result has no true bits.
		/// Also returns the result in the <see cref="result"/> parameter.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public bool AndIsEmpty(ReadOnlyMask other, out ReadOnlyMask result)
		{
			result = And(other);
			return result.IsEmpty;
		}

		/// <summary>
		/// AND this mask with <see cref="other"/> and return the true if the result has at lease one true bits.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public bool AndNotEmpty(ReadOnlyMask other) => !AndIsEmpty(other);

		/// <summary>
		/// OR this mask with <see cref="other"/> and return the result.
		/// </summary>
		/// <exception cref="ArgumentNullException">If <see cref="other"/> is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public ReadOnlyMask Or(ReadOnlyMask other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (bitLength != other.bitLength)
				throw new ArgumentException("Array lengths differ");

			if (isAllFalse)
				return other;

			if (isAllTrue || trueCount == bitLength)
				return this;

			if (other.isAllFalse)
				return this;

			if (other.isAllTrue || other.trueCount == other.bitLength)
				return other;

			if (bitLength <= Threshold1)
				return new ReadOnlyMask(bitLength, mask | other.mask);

			if (bitLength <= Threshold3)
				return DoOr3(other);

			return DoOr(other);
		}

		/// <summary>
		/// NOT this mask and return the result.
		/// </summary>
		public ReadOnlyMask Not()
		{
			if (isAllFalse)
				return new ReadOnlyMask(bitLength, true);

			if (isAllTrue || trueCount == bitLength)
				return new ReadOnlyMask(bitLength, false);

			if (bitLength <= Threshold1)
				return new ReadOnlyMask(bitLength, ~mask);

			if (bitLength <= Threshold3)
				return DoNot3();

			return DoNot();
		}

		/// <summary>
		/// Enumerates all the indexes of all the true bits in the mask.
		/// </summary>
		public IEnumerable<int> EnumerateIndexes()
		{
			if (isAllFalse)
				yield break;

			if (isAllTrue)
			{
				for (var i = 0; i < bitLength; i++)
					yield return i;

				yield break;
			}

			var test = bitLength > Threshold1
				? (Func<int, bool>)(i => (array[i >> 6] & (1UL << (i % ULongBits))) != 0)
				: i => (mask & (1UL << i)) != 0;

			for (var i = 0; i < bitLength; i++)
			{
				if (test(i))
					yield return i;
			}
		}

		/// <summary>
		/// Is this mask a subset of <param name="other"/>.
		/// </summary>
		public bool IsSubsetOf(ReadOnlyMask other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (bitLength != other.bitLength)
				throw new ArgumentException("Array lengths differ");

			if (isAllFalse || other.isAllTrue)
				return true;

			if (isAllTrue || other.isAllFalse)
				return false;

			if (bitLength <= Threshold1)
				return (mask & other.mask) == mask;

			var arrayLength = array.Length;

			for (var i = 0; i < arrayLength; i++)
			{
				var src = array[i];
				var oth = other.array[i];

				if ((src & oth) != src)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Test if this mask is the same as <see cref="other"/>.
		/// </summary>
		public bool Equals(ReadOnlyMask other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (bitLength != other.bitLength)
				return false;

			if (isAllFalse && other.isAllFalse)
				return true;

			if (isAllTrue && other.isAllTrue)
				return true;

			return bitLength > Threshold1
				? array.SequenceEqual(other.array)
				: mask == other.mask;
		}

		/// <summary>
		/// Convert the mask to a string representation.
		///
		/// Note: the LSB of the mask is on the left
		/// E.g. 0xC = 12 = '0011'
		/// </summary>
		public IResult ToString(string format) => ToString().ToSuccess();

		/// <summary>
		/// Convert the mask to a string representation.
		///
		/// Note: the LSB of the mask is on the left
		/// E.g. 0xC = 12 = '0011'
		/// </summary>
		public override string ToString()
		{
			// Possible error here, as the maximum bit length is greater than the largest int.
			var sb = new StringBuilder(bitLength);

			for (var i = 0; i < bitLength; i++)
				sb.Append(this[i] ? "1" : "0");

			return sb.ToString();
		}

		#endregion

		#region And, Or and Not implementations optimised for bit length

		private ReadOnlyMask DoAnd3(ReadOnlyMask other)
		{
			var arrayLength = array.Length;
			var cloneArray = new ulong[arrayLength];
			var thisArray = array;
			var otherArray = other.array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: cloneArray[2] = thisArray[2] & otherArray[2]; goto case 2;
				case 2: cloneArray[1] = thisArray[1] & otherArray[1]; goto case 1;
				case 1: cloneArray[0] = thisArray[0] & otherArray[0]; return new ReadOnlyMask(bitLength, cloneArray);
			}

			throw new InvalidOperationException();
		}

		private ReadOnlyMask DoAnd(ReadOnlyMask other)
		{
			var arrayLength = array.Length;
			var resultArray = new ulong[arrayLength];
			var thisArray = array;
			var otherArray = other.array;

			var i = 0U;

#if NET8_0_OR_GREATER
			if (Vector256.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var right = ref MemoryMarshal.GetArrayDataReference(otherArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector256ULongCount - 1U); i += Vector256ULongCount)
				{
					var r = Vector256.LoadUnsafe(ref left, i) & Vector256.LoadUnsafe(ref right, i);
					r.StoreUnsafe(ref result, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var right = ref MemoryMarshal.GetArrayDataReference(otherArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector128ULongCount - 1U); i += Vector128ULongCount)
				{
					var r = Vector128.LoadUnsafe(ref left, i) & Vector128.LoadUnsafe(ref right, i);
					r.StoreUnsafe(ref result, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				resultArray[i] = thisArray[i] & otherArray[i];

			return new ReadOnlyMask(bitLength, resultArray);
		}

		private int DoAndTrueCount3(ReadOnlyMask other)
		{
			var count = 0;
			var thisArray = array;
			var otherArray = other.array;
			var arrayLength = array.Length;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: count += PopulationCount(thisArray[2] & otherArray[2]); goto case 2;
				case 2: count += PopulationCount(thisArray[1] & otherArray[1]); goto case 1;
				case 1: count += PopulationCount(thisArray[0] & otherArray[0]); return count;
			}

			throw new InvalidOperationException();
		}

		private int DoAndTrueCount(ReadOnlyMask other)
		{
			var count = 0;
			var thisArray = array;
			var otherArray = other.array;
			var arrayLength = array.Length;

			for (var i = 0; i < arrayLength; i++)
				count += PopulationCount(thisArray[i] & otherArray[i]);

			return count;
		}

		private bool DoAndIsEmpty3(ReadOnlyMask other)
		{
			var thisArray = array;
			var otherArray = other.array;
			var arrayLength = array.Length;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: if ((thisArray[2] & otherArray[2]) == 0) goto case 2; return false;
				case 2: if ((thisArray[1] & otherArray[1]) == 0) goto case 1; return false;
				case 1: return (thisArray[0] & otherArray[0]) == 0;
			}

			throw new InvalidOperationException();
		}

		private bool DoAndIsEmpty(ReadOnlyMask other)
		{
			var thisArray = array;
			var otherArray = other.array;
			var arrayLength = array.Length;

			for (var i = 0U; i < (uint)arrayLength; i++)
			{
				if ((thisArray[i] & otherArray[i]) != 0)
					return false;
			}

			return true;
		}

		private ReadOnlyMask DoOr3(ReadOnlyMask other)
		{
			var arrayLength = array.Length;
			var cloneArray = new ulong[arrayLength];
			var thisArray = array;
			var otherArray = other.array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: cloneArray[2] = thisArray[2] | otherArray[2]; goto case 2;
				case 2: cloneArray[1] = thisArray[1] | otherArray[1]; goto case 1;
				case 1: cloneArray[0] = thisArray[0] | otherArray[0]; return new ReadOnlyMask(bitLength, cloneArray);
			}

			throw new InvalidOperationException();
		}

		private ReadOnlyMask DoOr(ReadOnlyMask other)
		{
			var arrayLength = array.Length;
			var resultArray = new ulong[arrayLength];
			var thisArray = array;
			var otherArray = other.array;

			var i = 0U;

#if NET8_0_OR_GREATER
			if (Vector256.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var right = ref MemoryMarshal.GetArrayDataReference(otherArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector256ULongCount - 1U); i += Vector256ULongCount)
				{
					var r = Vector256.LoadUnsafe(ref left, i) | Vector256.LoadUnsafe(ref right, i);
					r.StoreUnsafe(ref result, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var right = ref MemoryMarshal.GetArrayDataReference(otherArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector128ULongCount - 1U); i += Vector128ULongCount)
				{
					var r = Vector128.LoadUnsafe(ref left, i) | Vector128.LoadUnsafe(ref right, i);
					r.StoreUnsafe(ref result, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				resultArray[i] = thisArray[i] | otherArray[i];

			return new ReadOnlyMask(bitLength, resultArray);
		}

		private ReadOnlyMask DoNot3()
		{
			var arrayLength = array.Length;
			var cloneArray = new ulong[arrayLength];
			var thisArray = array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: cloneArray[2] = ~thisArray[2]; goto case 2;
				case 2: cloneArray[1] = ~thisArray[1]; goto case 1;
				case 1: cloneArray[0] = ~thisArray[0]; return new ReadOnlyMask(bitLength, cloneArray);
			}

			throw new InvalidOperationException();
		}

		private ReadOnlyMask DoNot()
		{
			var arrayLength = array.Length;
			var resultArray = new ulong[arrayLength];
			var thisArray = array;

			var i = 0U;

#if NET8_0_OR_GREATER
			if (Vector256.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector256ULongCount - 1U); i += Vector256ULongCount)
				{
					var r = ~Vector256.LoadUnsafe(ref left, i);
					r.StoreUnsafe(ref result, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
				ref var result = ref MemoryMarshal.GetArrayDataReference(resultArray);

				for (; i < (uint)arrayLength - (Vector128ULongCount - 1U); i += Vector128ULongCount)
				{
					var r = ~Vector128.LoadUnsafe(ref left, i);
					r.StoreUnsafe(ref result, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				resultArray[i] = ~thisArray[i];

			return new ReadOnlyMask(bitLength, resultArray);
		}

		private int DoTrueCount()
		{
			var arrayLength = array.Length;
			var count = 0;

			// No need for special 'last int' logic.
			for (var i = 0; i < arrayLength; i++)
				count += PopulationCount(array[i]);

			return count;
		}

		#endregion

		#region Optimised TrueCount Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int PopulationCount(ulong value)
		{
#if NET8_0_OR_GREATER
			if (Popcnt.IsSupported)
			{
				var low = (uint)(value & uint.MaxValue);
				var high = (uint)(value >> 32);
				return (int)(Popcnt.PopCount(low) + Popcnt.PopCount(high));
			}

			if (AdvSimd.Arm64.IsSupported)
			{
				// PopCount works on vector so convert input value to vector first.
				var input = Vector64.CreateScalar(value);
				var aggregated = AdvSimd.Arm64.AddAcross(AdvSimd.PopCount(input.AsByte()));
				return aggregated.ToScalar();
			}
#endif

			const ulong c1 = 0x5555555555555555UL;
			const ulong c2 = 0x3333333333333333UL;
			const ulong c3 = 0x0F0F0F0F0F0F0F0FUL;
			const ulong c4 = 0x0101010101010101UL;

			unchecked
			{
				value -= value >> 1 & c1;
				value = (value & c2) + (value >> 2 & c2);
				return (int)((value + (value >> 4) & c3) * c4 >> 56);
			}
		}

		#endregion
	}

	public static class ReadOnlyMaskExtensions
	{
		/// <summary>
		/// Create a mask with all bits set to true.
		/// After this call you will no longer be able to modify the masks.
		/// </summary>
		public static IReadOnlyList<ReadOnlyMask> ToReadOnlyMasks(this IReadOnlyList<Mask> masks, int bitLength)
		{
			ReadOnlyMask empty = null;
			var count = masks.Count;
			var readOnlyMasks = new ReadOnlyMask[count];

			for (var i = 0; i < count; i++)
			{
				var mask = masks[i];

				if (mask == null)
				{
					if (empty == null)
						empty = ReadOnlyMask.CreateAllFalse(bitLength);
					readOnlyMasks[i] = empty;
				}
				else
					readOnlyMasks[i] = mask.Lock();
			}

			return readOnlyMasks;
		}
	}
}