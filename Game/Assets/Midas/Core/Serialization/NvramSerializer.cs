using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Midas.Core.General;

namespace Midas.Core.Serialization
{
	/// <summary>
	/// Serializes game data without bloating it with type information.
	/// </summary>
	public static class NvramSerializer
	{
		#region Fields

		private static IReadOnlyList<Type> typeMap;
		private static bool typeMapChanged;
		private static readonly List<ICustomSerializer> customSerializers = new List<ICustomSerializer>();

		#endregion

		#region Constants

		private const string NvramSerializationData = "NvramSerializationData";

		#endregion

		#region Initialisation

		static NvramSerializer() => RegisterCustomSerializer(new GeneralCustomSerializer());

		/// <summary>
		/// Installs a custom serializer to support types that are not natively supported by the serializer.
		/// </summary>
		/// <param name="serializer">The custom serializer to register.</param>
		public static void RegisterCustomSerializer(ICustomSerializer serializer)
		{
			if (!customSerializers.Contains(serializer))
				customSerializers.Add(serializer);
		}

		public static void Init(IFoundationShim foundation)
		{
			if (typeMap != null)
				return;

			var scope = foundation.GameMode == FoundationGameMode.History ? NvramScope.History : NvramScope.Theme;
			if (foundation.TryReadNvram(scope, NvramSerializationData, out var data))
			{
				using var br = new BinaryReader(SerializedDataPadder.Unpad(new MemoryStream(data)));
				var count = br.ReadInt32();
				var tm = new List<Type>(count);

				for (var i = 0; i < count; i++)
				{
					var t = Type.GetType(br.ReadString());
					tm.Add(t);
					if (t != null)
						RuntimeHelpers.RunClassConstructor(t.TypeHandle);
				}

				typeMap = tm;
			}
			else
			{
				typeMap = new List<Type>();
			}
		}

		public static void DeInit() => typeMap = null;

		private static void SaveTypes(IFoundationShim foundation, NvramScope scope = NvramScope.Theme)
		{
			if (typeMapChanged || scope == NvramScope.History)
			{
				typeMapChanged = false;
				using var ms = new MemoryStream();
				using var bw = new BinaryWriter(ms, Encoding.Default, true);

				bw.Write(typeMap.Count);
				foreach (var type in typeMap)
					bw.Write(type.AssemblyQualifiedName);

				using var finalMs = new MemoryStream();
				SerializedDataPadder.Pad(ms.ToArray(), finalMs, true, scope == NvramScope.History ? 1 : 5000);
				foundation.WriteNvram(scope, NvramSerializationData, finalMs.ToArray());
			}
		}

		#endregion

		#region Public Methods

		public static void WriteNvram(this IFoundationShim foundation, NvramScope scope, string path, object data, int padThreshold = 0)
		{
			using var s = new MemoryStream();

			if (padThreshold == 0)
			{
				s.WriteByte(0);
				Serialize(s, data);
				foundation.WriteNvram(scope, path, s.ToArray());
			}
			else
			{
				Serialize(s, data);
				var serializedData = s.ToArray();
				using var finalMs = new MemoryStream();

				finalMs.WriteByte(1);
				SerializedDataPadder.Pad(serializedData, finalMs, serializedData.Length > padThreshold, padThreshold);
				foundation.WriteNvram(scope, path, finalMs.ToArray());
			}

			SaveTypes(foundation);
			if (scope == NvramScope.History)
				SaveTypes(foundation, scope);
		}

		public static bool TryReadNvram<T>(this IFoundationShim foundation, NvramScope scope, string path, out T data)
		{
			if (foundation.TryReadNvram(scope, path, out var byteData))
			{
				Stream stream = null;
				try
				{
					stream = new MemoryStream(byteData);
					var padded = stream.ReadByte() == 1;

					if (padded)
						stream = SerializedDataPadder.Unpad(stream);

					var bs = new BinarySerializer(customSerializers, typeMap);
					data = (T)bs.Deserialize(stream);

					return true;
				}
				finally
				{
					stream?.Dispose();
				}
			}

			data = default;
			return false;
		}

		/// <summary>
		/// Serialize an object into a stream.
		/// </summary>
		/// <param name="stream">The stream to serialize into.</param>
		/// <param name="o">The object to serialize.</param>
		private static void Serialize(Stream stream, object o)
		{
			using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
			var bs = new BinarySerializer(customSerializers, typeMap);
			bs.Serialize(writer, o);
			if (bs.TypeMap.Count != typeMap.Count)
			{
				typeMap = bs.TypeMap;
				typeMapChanged = true;
			}
		}

		#endregion
	}
}