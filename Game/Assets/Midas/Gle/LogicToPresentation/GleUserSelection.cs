using System;
using System.Collections.Generic;
using System.IO;
using Midas.Core.Serialization;

namespace Midas.Gle.LogicToPresentation
{
	public enum GleDecisionPersistence
	{
		Permanent,
		Game,
		Cycle
	}

	public sealed class GleUserSelection
	{
		public string Name { get; }
		public IReadOnlyList<int> Selections { get; }
		public GleDecisionPersistence Persistence { get; }

		public GleUserSelection(string name, IReadOnlyList<int> selections, GleDecisionPersistence persistence)
		{
			Name = name;
			Selections = selections;
			Persistence = persistence;
		}

		#region Serialization

		private sealed class CustomSerializer : ICustomSerializer
		{
			public bool SupportsType(Type t) => t == typeof(GleUserSelection);

			public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
			{
				switch (o)
				{
					case GleUserSelection gd:
						writer.Write(gd.Name);
						serializeComplex(writer, gd.Selections);
						serializeComplex(writer, gd.Persistence);
						break;

					default:
						throw new InvalidOperationException($"Unable to serialize object of type {o.GetType()}");
				}
			}

			public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
			{
				if (t == typeof(GleUserSelection))
				{
					var name = reader.ReadString();
					var decisions = (IReadOnlyList<int>)deserializeComplex(reader);
					var persistence = (GleDecisionPersistence)deserializeComplex(reader);
					return new GleUserSelection(name, decisions, persistence);
				}

				throw new InvalidOperationException($"Unable to deserialize object of type {t}");
			}
		}

		static GleUserSelection()
		{
			NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
		}

		#endregion
	}
}