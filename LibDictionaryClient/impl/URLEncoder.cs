using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient.impl
{

	public class URLEncoder
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static string HEX_CHARS = "0123456789abcdef";
		private static string VALID_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-~";

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public URLEncoder()
		{
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public static string Encode(string text, EnumURLEncodings encoding)
		{
			switch (encoding) {
				case EnumURLEncodings.UTF8:
					return EncodeUTF8(text);
				default:
					throw new ImplementationErrorException();
			}
		}

		private static string EncodeUTF8(string text)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in text) {
				if (c == ' ') sb.Append('+');
				else {
					if (VALID_CHARS.IndexOf(c) >= 0) {
						sb.Append(c);
					} else {
						byte[] data = UTF8Encoding.UTF8.GetBytes("" + c);
						for (int i = 0; i < data.Length; i++) {
							sb.Append('%');
							int hi = (data[i] >> 4) & 15;
							sb.Append(HEX_CHARS[hi]);
							int lo = data[i] & 15;
							sb.Append(HEX_CHARS[lo]);
						}						
					}
				}
			}
			return sb.ToString();
		}

	}

}
