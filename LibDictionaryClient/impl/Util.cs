using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient.impl
{

	public class Util
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		public static readonly string CRLF = "" + ((char)13) + ((char)10);

		public static readonly NumberFormatInfo NFI_DOUBLE;
		public static readonly NumberFormatInfo NFI_FLOAT;

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		private static string appDirPathCache;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		static Util()
		{
			NFI_DOUBLE = new CultureInfo("en-US", false).NumberFormat;
			NFI_DOUBLE.NumberDecimalDigits = 15;
			NFI_FLOAT = new CultureInfo("en-US", false).NumberFormat;
			NFI_FLOAT.NumberDecimalDigits = 7;
		}

		private Util()
		{
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public static string AppDirPath
		{
			get {
				if (appDirPathCache != null) return appDirPathCache;
				System.Reflection.Module[] modules = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
				string aPath = System.IO.Path.GetDirectoryName(modules[0].FullyQualifiedName);
				if ((aPath != "") && (aPath[aPath.Length - 1] != Path.DirectorySeparatorChar)) aPath += Path.DirectorySeparatorChar;
				appDirPathCache = aPath;
				return appDirPathCache;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public static bool Equals(string s1, string s2)
		{
			if (s1 == null) {
				if (s2 == null) {
					return true;
				} else {
					return false;
				}
			} else {
				if (s1 == null) {
					return false;
				} else {
					bool b = s1.Equals(s2);
					return b;
				}
			}
		}

		public static void Noop()
		{
		}

		public static string Normalize(string s)
		{
			if (s == null)
				return null;

			s = s.Trim();
			if (s.Length == 0)
				return null;

			return s;
		}

		public static int Max(params int[] values)
		{
			int max = 0;
			for (int i = 0; i < values.Length; i++) {
				if (values[i] > max) max = values[i];
			}
			return max;
		}

		public static string AddSeparatorChar(string path)
		{
			if (path.EndsWith("/") || path.EndsWith("\\")) return path;
			return path + Path.DirectorySeparatorChar;
		}

	}

}
