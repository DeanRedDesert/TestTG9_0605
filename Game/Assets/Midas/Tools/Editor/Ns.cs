// Copyright (C) 2020, IGT Australia Pty. Ltd.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

// ReSharper disable UnusedMember.Global - Helper methods and fields for future use.
namespace Midas.Tools.Editor
{
	/// <summary>
	/// Helper class for commonly used xml namespaces.
	/// </summary>
	/// <remarks>
	/// Please excuse the brevity of the class name. It makes using it much cleaner.
	/// </remarks>
	public static class Ns
	{
		public static readonly XNamespace R1 = "F2XRegistryVer1.xsd";
		public static readonly XNamespace Rt2 = "F2LRegistryTypeVer2.xsd";
		public static readonly XNamespace Pr3 = "F2LPayvarRegistryVer3.xsd";
		public static readonly XNamespace Tr5 = "F2LThemeRegistryVer5.xsd";
		public static readonly XNamespace Rt1 = "F2XRegistryTypesVer1.xsd";
		public static readonly XNamespace Sgr1 = "F2XSpcGroupRegistryVer1.xsd";
		public static readonly XNamespace Ngr1 = "F2XNetProgressiveControllerGroupRegistryVer1.xsd";
		public static readonly XNamespace Plr1 = "F2XProgressiveLinkRegistryVer1.xsd";

		/// <summary>
		/// Get the descendant named <paramref name="descendantName"/> at the <paramref name="index"/> of all descendants of the same name.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// Throws an exception on not found.
		/// </summary>
		public static XElement Descendant(this XElement e, string descendantName, int index = 0)
		{
			return e.Descendants(e.Name.Namespace + descendantName).ElementAt(index);
		}

		/// <summary>
		/// Get the descendant named <paramref name="descendantName"/> which matches the <paramref name="predicate"/>.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// Throws an exception on not found.
		/// </summary>
		public static XElement Descendant(this XElement e, string descendantName, Func<XElement, bool> predicate)
		{
			return e.Descendants(e.Name.Namespace + descendantName).Single(predicate);
		}

		/// <summary>
		/// Get the descendant named <paramref name="descendantName"/> which matches the <paramref name="predicate"/>.
		/// Throws an exception on not found.
		/// </summary>
		public static XElement Descendant(this XElement e, XName descendantName, Func<XElement, bool> predicate)
		{
			return e.Descendants(descendantName).Single(predicate);
		}

		/// <summary>
		/// Get the string value of the descendant named <paramref name="descendantName"/>.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// Throws an exception if there is not exactly one result.
		/// </summary>
		public static string DescendantValue(this XElement e, string descendantName)
		{
			return e.Descendants(e.Name.Namespace + descendantName).Single().Value;
		}

		/// <summary>
		/// Get the string value of the descendant named <paramref name="descendantName"/>.
		/// Throws an exception if there is not exactly one result.
		/// </summary>
		public static string DescendantValue(this XElement e, XName descendantName)
		{
			return e.Descendants(descendantName).Single().Value;
		}

		/// <summary>
		/// Get the string values of all descendants named <paramref name="descendantName"/>.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// </summary>
		public static IEnumerable<string> DescendantValues(this XElement e, string descendantName)
		{
			return e.Descendants(e.Name.Namespace + descendantName).Select(v => v.Value);
		}

		/// <summary>
		/// Get the string value of the child element named <paramref name="childName"/>.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// Throws an exception if there is not exactly one result.
		/// </summary>
		public static string ElementValue(this XElement e, string childName)
		{
			return e.Elements(e.Name.Namespace + childName).Single().Value;
		}

		/// <summary>
		/// Get the string value of the child element named <paramref name="childName"/>.
		/// Throws an exception if there is not exactly one result.
		/// </summary>
		public static string ElementValue(this XElement e, XName childName)
		{
			return e.Elements(childName).Single().Value;
		}

		/// <summary>
		/// Get the string value of the child attribute named <paramref name="attributeName"/>.
		/// XNamespace is assumed to be the same as <paramref name="e" />.
		/// Throws an exception if there is not exactly one result.
		/// </summary>
		public static string AttributeValue(this XElement e, string attributeName)
		{
			return e.Attributes(e.Name.Namespace + attributeName).Single().Value;
		}
	}
}