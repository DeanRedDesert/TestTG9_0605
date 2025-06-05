//-----------------------------------------------------------------------
// <copyright file = "CompactSerializer.BaseTypes.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// While ReSharper and Visual Studio can infer the types, Mono cannot.
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace IGT.Game.Core.CompactSerialization
{
    #region Usings

    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;

    #endregion

    /// <summary>
    ///   This class provides functions for writing and reading a specific
    ///   primitive data type.
    ///   These functions can be utilized by other classes that implements
    ///   ICompactSerializable or other custom serialization.
    /// </summary>
    public partial class CompactSerializer
    {
        #region Conversion Delegates

        // Conversion delegates used to avoid delegate allocations when using BitConverter.
        private static readonly Func<bool, byte[]> GetBoolBytes = BitConverter.GetBytes;
        private static readonly Func<char, byte[]> GetCharBytes = BitConverter.GetBytes;
        private static readonly Func<int, byte[]> GetIntBytes = BitConverter.GetBytes;
        private static readonly Func<short, byte[]> GetShortBytes = BitConverter.GetBytes;
        private static readonly Func<ushort, byte[]> GetUShortBytes = BitConverter.GetBytes;
        private static readonly Func<uint, byte[]> GetUIntBytes = BitConverter.GetBytes;
        private static readonly Func<long, byte[]> GetLongBytes = BitConverter.GetBytes;
        private static readonly Func<ulong, byte[]> GetULongBytes = BitConverter.GetBytes;
        private static readonly Func<float, byte[]> GetSingleBytes = BitConverter.GetBytes;
        private static readonly Func<double, byte[]> GetDoubleBytes = BitConverter.GetBytes;

        private static readonly Func<byte[], int, bool> ToBoolean = BitConverter.ToBoolean;
        private static readonly Func<byte[], int, char> ToChar = BitConverter.ToChar;
        private static readonly Func<byte[], int, short> ToInt16 = BitConverter.ToInt16;
        private static readonly Func<byte[], int, int> ToInt32= BitConverter.ToInt32;
        private static readonly Func<byte[], int, ushort> ToUInt16 = BitConverter.ToUInt16;
        private static readonly Func<byte[], int, uint> ToUInt32 = BitConverter.ToUInt32;
        private static readonly Func<byte[], int, ulong> ToUInt64 = BitConverter.ToUInt64;
        private static readonly Func<byte[], int, long> ToInt64 = BitConverter.ToInt64;
        private static readonly Func<byte[], int, float> ToSingle = BitConverter.ToSingle;
        private static readonly Func<byte[], int, double> ToDouble = BitConverter.ToDouble;

        #endregion

        #region Utility Functions for Writing and Reading T

        /// <summary>
        ///   Utility function to convert a value of <typeparamref name = "T" />
        ///   to a byte array with the given converting function, then write
        ///   the byte array to the stream.
        /// </summary>
        /// <typeparam name = "T">The type of the data to write.</typeparam>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <param name = "byteConverter">The function to convert a <typeparamref name = "T" /> value to a byte array.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        private static void WriteRaw<T>(Stream stream, T data, Func<T, byte[]> byteConverter)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var bytes = byteConverter(data);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///   Utility function to read a byte array from the stream, then
        ///   convert the byte array to a value of <typeparamref name = "T" />
        ///   with the given converting function.
        /// </summary>
        /// <typeparam name = "T">The type of the data to read.</typeparam>
        /// <param name = "stream">The stream to read from.</param>
        /// <param name = "dataSize">The size of the data to read.</param>
        /// <param name = "typeConverter">The function to convert a byte array to a <typeparamref name = "T" /> value.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown when the number of bytes read from the stream is less than <paramref name="dataSize"/> bytes.
        /// </exception>
        private static T ReadRaw<T>(Stream stream, int dataSize, Func<byte[], int, T> typeConverter)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var bytes = GetThreadBuffer(dataSize);
            var bytesRead = stream.Read(bytes, 0, dataSize);
            if(bytesRead < dataSize)
            {
                var message = $"Attempted to read {dataSize} bytes but read {bytesRead} bytes.";
                throw new CompactSerializationException(message);
            }
            return typeConverter(bytes, 0);
        }

        #endregion

        #region Writing and Reading Bool

        /// <summary>
        ///   Write a Boolean value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, bool data)
        {
            WriteRaw(stream, data, GetBoolBytes);
        }

        /// <summary>
        ///   Read a Boolean value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Boolean value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static bool ReadBool(Stream stream)
        {
            return ReadRaw(stream, sizeof(bool), ToBoolean);
        }

        #endregion

        #region Writing and Reading Byte

        /// <summary>
        ///   Write a Byte value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, byte data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteByte(data);
        }

        /// <summary>
        ///   Read a Byte value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Byte value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static byte ReadByte(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return (byte) stream.ReadByte();
        }

        #endregion

        #region Writing and Reading Char

        /// <summary>
        ///   Write a Char value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, char data)
        {
            WriteRaw(stream, data, GetCharBytes);
        }

        /// <summary>
        ///   Read a Char value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Char value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static char ReadChar(Stream stream)
        {
            return ReadRaw(stream, sizeof(char), ToChar);
        }

        #endregion

        #region Writing and Reading Short

        /// <summary>
        ///   Write an Int16 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, short data)
        {
            WriteRaw(stream, data, GetShortBytes);
        }

        /// <summary>
        ///   Read an Int16 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Int16 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static short ReadShort(Stream stream)
        {
            return ReadRaw(stream, sizeof(short), ToInt16);
        }

        #endregion

        #region Writing and Reading Int

        /// <summary>
        ///   Write an Int32 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, int data)
        {
            WriteRaw(stream, data, GetIntBytes);
        }

        /// <summary>
        ///   Read an Int32 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Int32 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static int ReadInt(Stream stream)
        {
            return ReadRaw(stream, sizeof(int), ToInt32);
        }

        #endregion

        #region Writing and Reading Long

        /// <summary>
        ///   Write an Int64 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, long data)
        {
            WriteRaw(stream, data, GetLongBytes);
        }

        /// <summary>
        ///   Read an Int64 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Int64 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static long ReadLong(Stream stream)
        {
            return ReadRaw(stream, sizeof(long), ToInt64);
        }

        #endregion

        #region Writing and Reading Ushort

        /// <summary>
        ///   Write a UInt16 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, ushort data)
        {
            WriteRaw(stream, data, GetUShortBytes);
        }

        /// <summary>
        ///   Read a UInt16 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The UInt16 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static ushort ReadUshort(Stream stream)
        {
            return ReadRaw(stream, sizeof(ushort), ToUInt16);
        }

        #endregion

        #region Writing and Reading Uint

        /// <summary>
        ///   Write a UInt32 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, uint data)
        {
            WriteRaw(stream, data, GetUIntBytes);
        }

        /// <summary>
        ///   Read a UInt32 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The UInt32 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static uint ReadUint(Stream stream)
        {
            return ReadRaw(stream, sizeof(uint), ToUInt32);
        }

        #endregion

        #region Writing and Reading Ulong

        /// <summary>
        ///   Write a UInt64 value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, ulong data)
        {
            WriteRaw(stream, data, GetULongBytes);
        }

        /// <summary>
        ///   Read a UInt64 value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The UInt64 value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static ulong ReadUlong(Stream stream)
        {
            return ReadRaw(stream, sizeof(ulong), ToUInt64);
        }

        #endregion

        #region Writing and Reading Float

        /// <summary>
        ///   Write a Single value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, float data)
        {
            WriteRaw(stream, data, GetSingleBytes);
        }

        /// <summary>
        ///   Read a Single value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Single (float) value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static float ReadFloat(Stream stream)
        {
            return ReadRaw(stream, sizeof(float), ToSingle);
        }

        #endregion

        #region Writing and Reading Double

        /// <summary>
        ///   Write a Double value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, double data)
        {
            WriteRaw(stream, data, GetDoubleBytes);
        }

        /// <summary>
        ///   Read a Double value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The Double value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the stream does not contain enough data.
        /// </exception>
        public static double ReadDouble(Stream stream)
        {
            return ReadRaw(stream, sizeof(double), ToDouble);
        }

        #endregion

        #region Writing and Reading Decimal

        /// <summary>
        ///   Write a decimal value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, decimal data)
        {
            var bits = decimal.GetBits(data);
            foreach(var integer in bits)
            {
                Write(stream, integer);
            }
        }

        /// <summary>
        ///   Read a decimal value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The decimal value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <remarks>
        ///   For more information on converting decimal to bytes and vice versa:
        ///   http://social.technet.microsoft.com/wiki/contents/articles/19055.convert-system-decimal-to-and-from-byte-arrays-vb-c.aspx
        /// </remarks>
        public static decimal ReadDecimal(Stream stream)
        {
            const int numberOfIntegers = 4;
            var integers = new int[numberOfIntegers];
            for(var index = 0; index < numberOfIntegers; index++)
            {
                integers[index] = ReadInt(stream);
            }

            return new decimal(integers);
        }

        #endregion

        #region Writing and Reading Nullable Types

        /// <summary>
        ///   Write a nullable value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <paramref name = "data" /> is of an unsupported type.
        /// </exception>
        public static void Write<T>(Stream stream, T? data) where T : struct
        {
            if(!Supports(typeof(T?)))
            {
                throw new CompactSerializationException(
                    $"Type {typeof(T?)} is not supported by CompactSerializer.");
            }

            if(data != null)
            {
                WriteRaw(stream, true, GetBoolBytes);
                WriteBaseType(stream, data, GetTypeIdFromType(typeof(T)));
            }
            else
            {
                WriteRaw(stream, false, GetBoolBytes);
            }
        }

        /// <summary>
        ///   Read a nullable value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static T? ReadNullable<T>(Stream stream) where T : struct
        {
            var hasValue = ReadBool(stream);
            return hasValue ? (T?)ReadBaseType(stream, GetCompactType(typeof(T)).BaseType) : null;
        }

        #endregion

        #region Writing and Reading String

        /// <summary>
        ///   Write a String value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, string data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(data != null)
            {
                var bytes = Encoding.UTF8.GetBytes(data);

                Write(stream, bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                Write(stream, NullLength);
            }
        }

        /// <summary>
        ///   Read a String value from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The String value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the number of bytes read is less than the expected length of the string.
        /// </exception>
        public static string ReadString(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            string result = null;

            var length = ReadInt(stream);

            if(length != NullLength)
            {
                var bytes = GetThreadBuffer(length);
                var bytesRead = stream.Read(bytes, 0, length);
                if(bytesRead != length)
                {
                    throw new CompactSerializationException(
                        "The number of bytes read was less than the expected length of the string. Expected: " +
                        $"{length} Actual: {bytesRead} This usually means a previous read call on the stream read too many bytes.");
                }

                result = Encoding.UTF8.GetString(bytes, 0, length);
            }

            return result;
        }

        #endregion

        #region Writing and Reading Byte Array

        /// <summary>
        ///   Write a Byte array to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, byte[] data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(data != null)
            {
                Write(stream, data.Length);
                stream.Write(data, 0, data.Length);
            }
            else
            {
                Write(stream, NullLength);
            }
        }

        /// <summary>
        ///   Read a Byte array from a stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The byte array read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        /// Thrown when the returned length of the byte array is invalid. 
        /// </exception>
        public static byte[] ReadByteArray(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            byte[] result = null;

            var length = ReadInt(stream);

            if(length < NullLength)
            {
                throw new CompactSerializationException(
                    $"Invalid byte array length of {length} returned from the stream.");
            }
            else if(length != NullLength)
            {
                // Don't use the bytes buffer here since this byte array will be exposed to the user code directly.
                result = new byte[length];
                stream.Read(result, 0, length);
            }

            return result;
        }

        #endregion

        #region Writing and Reading Enums

        /// <summary>
        ///   Write an enumeration value to a stream.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        public static void Write(Stream stream, Enum data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var enumValue = Enum.GetName(data.GetType(), data);
            Write(stream, enumValue);
        }

        /// <summary>
        ///   Read an enumeration value from the stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <typeparam name = "TEnum">The enumeration data type being read.</typeparam>
        /// <returns>The enumeration value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "System.InvalidCastException">
        ///   Thrown when <typeparamref name = "TEnum" /> is not an enumeration.
        /// </exception>
        public static TEnum ReadEnum<TEnum>(Stream stream)
        {
            return (TEnum) ReadEnum(stream, typeof(TEnum));
        }


        /// <summary>
        ///   Read an enumeration value from the stream.
        /// </summary>
        /// <param name = "stream">The stream to read from.</param>
        /// <param name = "enumType">The enumeration data type being read.</param>
        /// <returns>The enumeration value read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "System.InvalidCastException">
        ///   Thrown when <paramref name = "enumType" /> is not an enumeration value.
        /// </exception>
        private static object ReadEnum(Stream stream, Type enumType)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(!enumType.IsEnum)
            {
                throw new InvalidCastException("The specified data type is not an enumeration.");
            }

            var enumValue = ReadString(stream);
            var result = Enum.Parse(enumType, enumValue);

            return result;
        }

        #endregion

        #region Writing and Reading ICompactSerializable

        /// <summary>
        ///   A unique identifier marking the end of the data for an object
        ///   that supports ICompactSerializable. This is used to ensure the correct
        ///   number of bytes were deserialized.
        /// </summary>
        private const ushort SerializableStopMarker = 0xA5A5;

        /// <summary>
        ///   Write to a stream the value of a data type that implements
        ///   ICompactSerializable.
        /// </summary>
        /// <param name = "stream">The stream to write to.</param>
        /// <param name = "data">The data to write.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        ///   Thrown if the Serialize function generates an exception.
        /// </exception>
        public static void Write(Stream stream, ICompactSerializable data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(data == null)
            {
                // The data is null.
                Write(stream, true);
            }
            else
            {
                // The data is not null.
                Write(stream, false);
                try
                {
                    data.Serialize(stream);
                }
                catch (Exception ex)
                {
                    throw new CompactSerializationException(
                        $"There was an unhandled exception while serializing type {data.GetType()}. " +
                        "Check the inner exception for more details.", ex);
                }
                Write(stream, SerializableStopMarker);
            }
        }

        /// <summary>
        ///   Read from a stream the value of a data type that implements
        ///   ICompactSerializable.
        /// </summary>
        /// <param name = "dataType">The type of the data to read.</param>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The value of type <paramref name = "dataType" /> read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <paramref name = "dataType" /> does not provide a public
        ///   parameter-less constructor, or not implement ICompactSerializable.
        ///   Thrown if there is an unhandled exception calling the Deserialize function
        ///   on the type.
        ///   Thrown if the wrong number of bytes were read by the Deserialize function.
        /// </exception>
        private static object ReadSerializable(Stream stream, Type dataType)
        {
            object result;
            try
            {
                result = Activator.CreateInstance(dataType);
            }
            catch(MissingMethodException ex)
            {
                var outerException = new CompactSerializationException(
                    "Types implementing ICompactSerializable must have a public parameter-less constructor", ex);

                throw outerException;
            }
            catch(TargetInvocationException ex)
            {
                var outerException = new CompactSerializationException(
                    "The ICompactSerializable type being created threw an exception in it's constructor.", ex);

                throw outerException;
            }
            catch(Exception ex)
            {
                var outerException = new CompactSerializationException("There was an exception while trying to create the ICompactSerializable type", ex);

                throw outerException;
            }

            // Make sure T implements ICompactSerializable.
            if(result is ICompactSerializable serializable)
            {
                Deserialize(ref serializable, stream, dataType);
                result = serializable;
            }
            else
            {
                throw new CompactSerializationException($"Type {dataType} does not implement ICompactSerializable.");
            }
            return result;
        }

        /// <summary>
        ///   Read from a stream the value of a data type that implements
        ///   ICompactSerializable.
        /// </summary>
        /// <typeparam name = "T">The type of the data to read.</typeparam>
        /// <param name = "stream">The stream to read from.</param>
        /// <returns>The value of type <typeparamref name = "T" /> read.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <typeparamref name = "T" /> does not provide a public
        ///   parameter-less constructor, or not implement ICompactSerializable.
        /// </exception>
        public static T ReadSerializable<T>(Stream stream)
        {
            return (T) ReadSerializable(stream, typeof(T));
        }

        #endregion

        #region Thread Local Bytes Buffer

        /// <summary>
        /// The thread local buffer of bytes used to reduce memory allocations when reading bytes from the stream.
        /// </summary>
        [ThreadStatic]
        private static byte[] threadBytesBuffer;

        /// <summary>
        /// The the thread local bytes buffer.
        /// </summary>
        /// <param name="dataSize">The requested size of the buffer.</param>
        /// <returns>The thread local bytes buffer with a length greater than or equal to the requested size.</returns>
        /// <remarks>
        /// For performance reasons, the buffer is not initialized with zeros. Therefore, care must be taken to either
        /// write to all elements or only read from the elements that were written to.
        /// </remarks>
        private static byte[] GetThreadBuffer(int dataSize)
        {
            if(threadBytesBuffer == null || dataSize > threadBytesBuffer.Length)
            {
                threadBytesBuffer = new byte[(dataSize / 8 + 1) * 8];
            }

            return threadBytesBuffer;
        }

        #endregion

        /// <summary>
        /// Deserialize the serializable object that implements ICompactSerializable.
        /// </summary>
        /// <param name="serializableObject">The object which implements ICompactSerializable.</param>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="dataType">The type of the serializableObject parameter.</param>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when data type of input object does not provide a public
        ///   parameter-less constructor, or not implement ICompactSerializable.
        ///   Thrown if there is an unhandled exception calling the Deserialize function
        ///   on the type.
        ///   Thrown if the wrong number of bytes were read by the Deserialize function.
        /// </exception>
        private static void Deserialize(ref ICompactSerializable serializableObject, Stream stream, Type dataType)
        {
            if(ReadBool(stream))
            {
                serializableObject = null;
                return;
            }

            try
            {
                serializableObject.Deserialize(stream);
            }
            catch(Exception ex)
            {
                throw new CompactSerializationException(
                    $"There was an unhandled exception while deserializing type {dataType}. Check the inner exception for more details.",
                    ex);
            }

            // Read and validate stop marker.
            var stopMarker = ReadUshort(stream);
            if(stopMarker != SerializableStopMarker)
            {
                throw new CompactSerializationException(
                    $"There was a problem deserializing type {dataType}. The deserialization was not of " +
                    "the proper length, too many or too few bytes were read.");
            }
        }
    }
}
