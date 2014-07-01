using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LibDictionaryClient.impl
{

	public class HttpURLBuilder
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static readonly JsonSerializer jsonSerializer = new JsonSerializer();

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		StringBuilder sb;

		bool bNeedsQuestionMark = true;
		bool bNeedsAmpersand = false;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public HttpURLBuilder(string host, int port, string basePath)
		{
			sb = new StringBuilder("http://");
			sb.Append(host);
			if (port != 80) {
				sb.Append(':');
				sb.Append(port);
			}
			sb.Append(basePath);
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public void AddParameterStr(string key, String value)
		{
			if (bNeedsQuestionMark) {
				sb.Append('?');
				bNeedsQuestionMark = false;
			}
			if (bNeedsAmpersand)
				sb.Append('&');
			else
				bNeedsAmpersand = true;

			sb.Append(URLEncoder.Encode(key, EnumURLEncodings.UTF8));
			sb.Append('=');
			if ((value != null) && (value.Length > 0)) {
				sb.Append(URLEncoder.Encode(value, EnumURLEncodings.UTF8));
			}
		}

		public void AddParameterJSON(string key, object value)
		{
			if (bNeedsQuestionMark) {
				sb.Append('?');
				bNeedsQuestionMark = false;
			}
			if (bNeedsAmpersand)
				sb.Append('&');
			else
				bNeedsAmpersand = true;

			sb.Append(URLEncoder.Encode(key, EnumURLEncodings.UTF8));
			sb.Append('=');
			if (value != null) {
				string s = __ToStr(value);
				if ((s != null) && (s.Length > 0)) {
					sb.Append(URLEncoder.Encode(s, EnumURLEncodings.UTF8));
				}
			}
		}

		public override string ToString()
		{
			return sb.ToString();
		}

		private string __ToStr(object value)
		{
			if (value == null) {
				return "";
			} else
			if (value is string) {
				return (string)value;
			} else
			if (value is bool) {
				if ((bool)value) {
					return "true";
				} else {
					return "false";
				}
			} else
			if (value is double) {
				return ((double)value).ToString(Util.NFI_DOUBLE);
			} else
			if (value is float) {
				return ((float)value).ToString(Util.NFI_FLOAT);
			} else
			if (value is JObject) {
				StringWriter sw = new StringWriter();
				jsonSerializer.Serialize(sw, ((JObject)value));
				return sw.ToString();
			} else
			if (value is JValue) {
				StringWriter sw = new StringWriter();
				jsonSerializer.Serialize(sw, ((JValue)value));
				return sw.ToString();
			} else
			if (value is JArray) {
				StringWriter sw = new StringWriter();
				jsonSerializer.Serialize(sw, ((JArray)value));
				return sw.ToString();
			} else {
				return value.ToString();
			}
		}

	}

}
