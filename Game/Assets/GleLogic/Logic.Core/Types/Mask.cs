using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace Logic.Core.Types
{
	public sealed class Mask : IEquatable<Mask>
	{
		private const int ULongBits = 64;
		private const ulong AllFalse = 0UL;
		private const ulong AllTrue = ~0UL;

		private const uint Vector128ULongCount = 2;
		private const uint Vector256ULongCount = 4;

		private readonly int bitLength;
		private readonly ulong[] array;
		private readonly int arrayLength;
		private readonly ulong remainderMask;

		private bool isLocked;

		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public int BitLength => bitLength;

		public Mask(int bitLength, bool defaultValue = false)
		{
			if (bitLength < 0)
				throw new ArgumentOutOfRangeException(nameof(bitLength));

			this.bitLength = bitLength;

			// Clear the most significant bits of the mask if the bit length is not a multiple of Threshold.
			var remainderBits = bitLength & (ULongBits - 1);
			remainderMask = remainderBits > 0 ? (1UL << remainderBits) - 1UL : AllTrue;
			arrayLength = ((bitLength - 1) >> 6) + 1;
			array = new ulong[arrayLength];

			if (defaultValue)
				SetAll(true);
		}

		/// <summary>
		/// Creates an unlocked clone of the mask.
		/// </summary>
		/// <exception cref="ArgumentNullException">If mask is null.</exception>
		public Mask(Mask mask)
		{
			if (mask == null)
				throw new ArgumentNullException(nameof(mask));

			// Copy the state from the mask.
			bitLength = mask.bitLength;
			arrayLength = mask.arrayLength;
			remainderMask = mask.remainderMask;
			isLocked = false;
			array = new ulong[arrayLength];
			Array.Copy(mask.array, array, arrayLength);
		}

		public ReadOnlyMask Lock()
		{
			isLocked = true;
			return ReadOnlyMask.CreateFrom(bitLength, array);
		}

		/// <summary>
		/// Get or Set the bit at the given index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">if index less than 0 or index greater than or equal to <see cref="BitLength"/>.</exception>
		public bool this[int index]
		{
			get => Get(index);
			set => Set(index, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Get(int index)
		{
			if ((uint)index >= (uint)bitLength)
				throw new ArgumentOutOfRangeException(nameof(index), index, null);

			return (array[index >> 6] & (1UL << (index % ULongBits))) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set(int index, bool value)
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

			if ((uint)index >= (uint)bitLength)
				throw new ArgumentOutOfRangeException(nameof(index), index, null);

			ref var segment = ref array[index >> 6];

			if (value)
				segment |= 1UL << (index % ULongBits);
			else
				segment &= ~(1UL << (index % ULongBits));
		}

		/// <summary>
		/// Sets all the bits to <see cref="value"/>.
		/// </summary>
		public void SetAll(bool value)
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

#if NET8_0_OR_GREATER
			if (value)
			{
				var span = array.AsSpan();
				span.Fill(AllTrue);
				// Clear high bit values in the last int
				span[arrayLength - 1] = remainderMask;
			}
			else
				array.AsSpan().Clear();
#else
			if (value)
			{
				for (var i = 0; i < arrayLength; i++)
					array[i] = AllTrue;

				// Clear high bit values in the last int
				array[arrayLength - 1] = remainderMask;
			}
			else
			{
				for (var i = 0; i < arrayLength; i++)
					array[i] = AllFalse;
			}
#endif
		}

		/// <summary>
		/// And the value onto the current instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
		/// <exception cref="ArgumentNullException">If the value is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public void And(Mask value)
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (bitLength != value.bitLength)
				throw new ArgumentException("Array lengths differ");

			var thisArray = array;
			var valueArray = value.array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: thisArray[2] &= valueArray[2]; goto case 2;
				case 2: thisArray[1] &= valueArray[1]; goto case 1;
				case 1: thisArray[0] &= valueArray[0]; return;
				case 0: return;
			}

			var i = 0U;

#if NET8_0_OR_GREATER
			ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
			ref var right = ref MemoryMarshal.GetArrayDataReference(valueArray);

			if (Vector256.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector256ULongCount - 1u); i += Vector256ULongCount)
				{
					var result = Vector256.LoadUnsafe(ref left, i) & Vector256.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector128ULongCount - 1u); i += Vector128ULongCount)
				{
					var result = Vector128.LoadUnsafe(ref left, i) & Vector128.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				thisArray[i] &= valueArray[i];
		}

		/// <summary>
		/// Or the value onto the current instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
		/// <exception cref="ArgumentNullException">If the value is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public void Or(Mask value)
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (bitLength != value.bitLength)
				throw new ArgumentException("Array lengths differ");

			var thisArray = array;
			var valueArray = value.array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: thisArray[2] |= valueArray[2]; goto case 2;
				case 2: thisArray[1] |= valueArray[1]; goto case 1;
				case 1: thisArray[0] |= valueArray[0]; return;
				case 0: return;
			}

			var i = 0U;

#if NET8_0_OR_GREATER
			ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
			ref var right = ref MemoryMarshal.GetArrayDataReference(valueArray);

			if (Vector256.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector256ULongCount - 1u); i += Vector256ULongCount)
				{
					var result = Vector256.LoadUnsafe(ref left, i) | Vector256.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector128ULongCount - 1u); i += Vector128ULongCount)
				{
					var result = Vector128.LoadUnsafe(ref left, i) | Vector128.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				thisArray[i] |= valueArray[i];
		}

		/// <summary>
		/// Xor the value onto the current instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
		/// <exception cref="ArgumentNullException">If the value is null</exception>
		/// <exception cref="ArgumentException">If the masks have different BitLengths</exception>
		public void Xor(Mask value)
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (bitLength != value.bitLength)
				throw new ArgumentException("Array lengths differ");

			var thisArray = array;
			var valueArray = value.array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: thisArray[2] ^= valueArray[2]; goto case 2;
				case 2: thisArray[1] ^= valueArray[1]; goto case 1;
				case 1: thisArray[0] ^= valueArray[0]; return;
				case 0: return;
			}

			var i = 0U;

#if NET8_0_OR_GREATER
			ref var left = ref MemoryMarshal.GetArrayDataReference(thisArray);
			ref var right = ref MemoryMarshal.GetArrayDataReference(valueArray);

			if (Vector256.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector256ULongCount - 1u); i += Vector256ULongCount)
				{
					var result = Vector256.LoadUnsafe(ref left, i) ^ Vector256.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector128ULongCount - 1u); i += Vector128ULongCount)
				{
					var result = Vector128.LoadUnsafe(ref left, i) ^ Vector128.LoadUnsafe(ref right, i);
					result.StoreUnsafe(ref left, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				thisArray[i] ^= valueArray[i];
		}

		/// <summary>
		/// Not the current instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
		public void Not()
		{
			if (isLocked)
				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");

			var thisArray = array;

			// Unroll loop for count less than Vector256 size.
			switch (arrayLength)
			{
				case 3: thisArray[2] = ~thisArray[2]; goto case 2;
				case 2: thisArray[1] = ~thisArray[1]; goto case 1;
				case 1: thisArray[0] = ~thisArray[0]; return;
				case 0: return;
			}

			var i = 0U;

#if NET8_0_OR_GREATER
			ref var value = ref MemoryMarshal.GetArrayDataReference(thisArray);

			if (Vector256.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector256ULongCount - 1u); i += Vector256ULongCount)
				{
					var result = ~Vector256.LoadUnsafe(ref value, i);
					result.StoreUnsafe(ref value, i);
				}
			}
			else if (Vector128.IsHardwareAccelerated)
			{
				for (; i < (uint)arrayLength - (Vector128ULongCount - 1u); i += Vector128ULongCount)
				{
					var result = ~Vector128.LoadUnsafe(ref value, i);
					result.StoreUnsafe(ref value, i);
				}
			}
#endif

			for (; i < (uint)arrayLength; i++)
				thisArray[i] = ~thisArray[i];
		}

// 		/// <summary>
// 		/// RightShift the current instance by the count bits.
// 		/// </summary>
// 		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
// 		/// <exception cref="ArgumentOutOfRangeException">If the count is less than zero</exception>
// 		public void RightShift(int count)
// 		{
// 			if (isLocked)
// 				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");
//
// 			if (count < 0)
// 				throw new ArgumentOutOfRangeException(nameof(count));
//
// 			if (count == 0)
// 				return;
//
// 			var toIndex = 0;
//
// 			if (count < bitLength)
// 			{
// 				// We can not use Math.DivRem without taking a dependency on System.Runtime.Extensions
// 				var fromIndex = (int)((uint)count / 32);
// 				var shiftCount = count & (32 - 1);
// 				var remainderBits = bitLength & (32 - 1);
//
// 				if (shiftCount == 0)
// 				{
// 					unchecked
// 					{
// 						// Cannot use `(1u << extraBits) - 1u` as the mask
// 						// because for extraBits == 0, we need the mask to be 111...111, not 0.
// 						// In that case, we are shifting a uint by 32, which could be considered undefined.
// 						// The result of a shift operation is undefined ... if the right operand
// 						// is greater than or equal to the width in bits of the promoted left operand,
// 						// https://docs.microsoft.com/en-us/cpp/c-language/bitwise-shift-operators?view=vs-2017
// 						// However, the compiler protects us from undefined behaviour by constraining the
// 						// right operand to between 0 and width - 1 (inclusive), i.e. right_operand = (right_operand % width).
// 						var mask = uint.MaxValue >> (BitsPerInt32 - remainderBits);
// 						array[arrayLength - 1] &= (int)mask;
// 					}
//
// 					Array.Copy(array, fromIndex, array, 0, arrayLength - fromIndex);
// 					toIndex = arrayLength - fromIndex;
// 				}
// 				else
// 				{
// 					var lastIndex = arrayLength - 1;
// 					unchecked
// 					{
// 						while (fromIndex < lastIndex)
// 						{
// 							var right = (uint)array[fromIndex] >> shiftCount;
// 							var left = array[++fromIndex] << (BitsPerInt32 - shiftCount);
// 							array[toIndex++] = left | (int)right;
// 						}
//
// 						var mask = uint.MaxValue >> (BitsPerInt32 - remainderBits);
// 						mask &= (uint)array[fromIndex];
// 						array[toIndex++] = (int)(mask >> shiftCount);
// 					}
// 				}
// 			}
//
// #if NET8_0_OR_GREATER
// 			array.AsSpan(toIndex, arrayLength - toIndex).Clear();
// #else
// 			for (var i = toIndex; i < arrayLength; i++)
// 				array[i] = 0;
// #endif
// 		}
//
// 		/// <summary>
// 		/// LeftShift the current instance by the count bits.
// 		/// </summary>
// 		/// <exception cref="InvalidOperationException">If the mask is locked for editing</exception>
// 		/// <exception cref="ArgumentOutOfRangeException">If the count is less than zero</exception>
// 		public void LeftShift(int count)
// 		{
// 			if (isLocked)
// 				throw new InvalidOperationException("Cannot change the mask while the mask is locked.");
//
// 			if (count < 0)
// 				throw new ArgumentOutOfRangeException(nameof(count));
//
// 			if (count == 0)
// 				return;
//
// 			int lengthToClear;
// 			if (count < bitLength)
// 			{
// 				var lastIndex = (bitLength - 1) >> BitShiftPerInt32; // Divide by 32.
//
// 				// We can not use Math.DivRem without taking a dependency on System.Runtime.Extensions
// 				lengthToClear = (int)((uint)count / 32);
// 				var shiftCount = count & (32 - 1);
//
// 				if (shiftCount == 0)
// 				{
// 					Array.Copy(array, 0, array, lengthToClear, lastIndex + 1 - lengthToClear);
// 				}
// 				else
// 				{
// 					var fromIndex = lastIndex - lengthToClear;
// 					unchecked
// 					{
// 						while (fromIndex > 0)
// 						{
// 							var left = array[fromIndex] << shiftCount;
// 							var right = (uint)array[--fromIndex] >> (BitsPerInt32 - shiftCount);
// 							array[lastIndex] = left | (int)right;
// 							lastIndex--;
// 						}
//
// 						array[lastIndex] = array[fromIndex] << shiftCount;
// 					}
// 				}
// 			}
// 			else
// 			{
// 				lengthToClear = arrayLength;
// 			}
//
// #if NET8_0_OR_GREATER
// 			array.AsSpan(0, lengthToClear).Clear();
// #else
// 			for (var i = 0; i < lengthToClear; i++)
// 				array[i] = 0;
// #endif
// 		}

		/// <summary>
		/// Determines whether all bits in the <see cref="Mask"/> are set to true.
		/// </summary>
		/// <returns>true if every bit in the <see cref="Mask"/> is set to true, or if <see cref="Mask"/> is empty; otherwise, false.</returns>
		public bool HasAllSet()
		{
#if NET8_0_OR_GREATER
			if (remainderMask == AllTrue)
				return !array.AsSpan(0, arrayLength).ContainsAnyExcept(AllTrue);

			var last = arrayLength - 1;

			if (array.AsSpan(0, last).ContainsAnyExcept(AllTrue))
				return false;
#else
			if (remainderMask == AllTrue)
			{
				for (var i = 0; i < arrayLength; i++)
				{
					if (array[i] != AllTrue)
						return false;
				}

				return true;
			}

			var last = arrayLength - 1;

			for (var i = 0; i < last; i++)
			{
				if (array[i] != AllTrue)
					return false;
			}
#endif

			return (array[last] & remainderMask) == remainderMask;
		}

		/// <summary>
		/// Determines whether any bit in the <see cref="Mask"/> is set to true.
		/// </summary>
		/// <returns>true if the <see cref="Mask"/> is not empty and any bit is set to true; otherwise, false.</returns>
		public bool HasAnySet()
		{
#if NET8_0_OR_GREATER
			if (remainderMask == AllTrue)
				return array.AsSpan(0, arrayLength).ContainsAnyExcept(AllFalse);

			var last = arrayLength - 1;

			if (array.AsSpan(0, last).ContainsAnyExcept(AllFalse))
				return true;
#else
			if (remainderMask == AllTrue)
			{
				for (var i = 0; i < arrayLength; i++)
				{
					if (array[i] != AllFalse)
						return true;
				}

				return false;
			}

			var last = arrayLength - 1;

			for (var i = 0; i < last; i++)
			{
				if (array[i] != AllFalse)
					return true;
			}
#endif

			return (array[last] & remainderMask) != AllFalse;
		}

		/// <summary>
		/// Counts the number of true bits in the <see cref="Mask"/>.
		/// </summary>
		/// <returns>The count of true bits in the <see cref="Mask"/>.</returns>
		public int TrueCount()
		{
			var count = 0;

			if (remainderMask == AllTrue)
			{
				// No need for special 'last int' logic.
				for (var i = 0; i < arrayLength; i++)
					count += PopulationCount(array[i]);
			}
			else
			{
				var last = arrayLength - 1;

				for (var i = 0; i < last; i++)
					count += PopulationCount(array[i]);

				count += PopulationCount(array[last] & remainderMask);
			}

			return count;
		}

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

		/// <summary>
		/// Checks if this mask is equal to the other mask.
		/// </summary>
		bool IEquatable<Mask>.Equals(Mask other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (bitLength != other.bitLength)
				return false;

			if (remainderMask == AllTrue)
				return array.SequenceEqual(other.array);

			var last = arrayLength - 1;

			for (var i = 0; i < last; i++)
			{
				if (array[i] != other.array[i])
					return false;
			}

			return (array[last] & remainderMask) == (other.array[last] & remainderMask);
		}
	}
}