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

	public class JsonHttpRequesterImpl
	{

		private static readonly JsonSerializer jsonSerializer = new JsonSerializer();

		public struct Response
		{
			public bool Success;
			public JObject Object;
			public JArray Array;
			public object Value;
			public int Duration;

			public Response(bool bSuccess, JValue obj, int duration)
			{
				this.Success = bSuccess;
				this.Array = null;
				this.Object = null;
				this.Value = obj.Value;
				this.Duration = duration;
			}

			public Response(bool bSuccess, JArray obj, int duration)
			{
				this.Success = bSuccess;
				this.Array = obj;
				this.Object = null;
				this.Value = null;
				this.Duration = duration;
			}

			public Response(bool bSuccess, JObject obj, int duration)
			{
				this.Success = bSuccess;
				this.Array = null;
				this.Object = obj;
				this.Value = null;
				this.Duration = duration;
			}

			public string ErrorMessage
			{
				get {
					if (Success) return null;
					if (Object == null) return null;
					JProperty p = Object.Property("errMsg");
					if (p == null) return null;
					if (p.Value == null) return null;
					string s = p.Value.ToString();
					if (s != null) {
						s = s.Trim();
						if (s.Length == 0) s = null;
					}
					return s;
				}
			}

			public EnumErrorID ErrorID
			{
				get {
					string s = ErrorIDString;
					if (s == null) return EnumErrorID.Unknown;
					try {
						return (EnumErrorID)(Enum.Parse(typeof(EnumErrorID), s, true));
					} catch (Exception ee) {
						return EnumErrorID.Unknown;
					}
				}
			}

			public string ErrorIDString
			{
				get {
					if (Success) return null;
					if (Object == null) return null;
					JProperty p = Object.Property("errID");
					if (p == null) return null;
					if (p.Value == null) return null;
					string s = p.Value.ToString();
					if (s != null) {
						s = s.Trim();
						if (s.Length == 0) s = null;
					}
					return s;
				}
			}

		}

		public struct BatchResponse
		{
			public bool Success;
			public JObject Error;
			public KeyValuePair<string, Response>[] Responses;
			public int Duration;

			public BatchResponse(bool bSuccess, JObject obj, int duration)
			{
				this.Success = bSuccess;
				this.Error = obj;
				this.Responses = null;
				this.Duration = duration;
			}

			public BatchResponse(bool bSuccess, KeyValuePair<string, Response>[] obj, int duration)
			{
				this.Success = bSuccess;
				this.Error = null;
				this.Responses = obj;
				this.Duration = duration;
			}

			public string ErrorMessage
			{
				get {
					if (Success) return null;
					if (Error == null) return null;
					JProperty p = Error.Property("errMsg");
					if (p == null) return null;
					if (p.Value == null) return null;
					return p.Value.ToString();
				}
			}

			public EnumErrorID ErrorID
			{
				get {
					string s = ErrorIDString;
					if (s == null) return EnumErrorID.Unknown;
					try {
						return (EnumErrorID)(Enum.Parse(typeof(EnumErrorID), s, true));
					} catch (Exception ee) {
						return EnumErrorID.Unknown;
					}
				}
			}

			public string ErrorIDString
			{
				get {
					if (Success) return null;
					JProperty p = Error.Property("errID");
					if (p == null) return null;
					if (p.Value == null) return null;
					string s = p.Value.ToString();
					if (s != null) {
						s = s.Trim();
						if (s.Length == 0) s = null;
					}
					return s;
				}
			}

		}

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		string host;
		int port;
		string basePath;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public JsonHttpRequesterImpl(string host, int port, string basePath, bool bDebuggingEnabled)
		{
			this.DebuggingEnabled = bDebuggingEnabled;
			this.host = host;
			this.port = port;
			this.basePath = Util.AddSeparatorChar(basePath);
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public bool DebuggingEnabled
		{
			get;
			set;
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		private string __BuildURL(string apiFunctionName, string sid, params KeyValuePair<string, object>[] arguments)
		{
			HttpURLBuilder urlBuilder = new HttpURLBuilder(host, port, basePath + apiFunctionName);
			if (sid != null) urlBuilder.AddParameterStr("sid", sid);
			if (arguments != null) {
				foreach (KeyValuePair<string, object> arg in arguments) {
					if (arg.Value == null)
						continue;
					urlBuilder.AddParameterJSON(arg.Key, arg.Value);
				}
			}
			return urlBuilder.ToString();
		}

		private string __BuildURL(string apiFunctionName, string sid, IList<KeyValuePair<string, object>> arguments)
		{
			HttpURLBuilder urlBuilder = new HttpURLBuilder(host, port, basePath + apiFunctionName);
			if (sid != null) urlBuilder.AddParameterStr("sid", sid);
			if (arguments != null) {
				foreach (KeyValuePair<string, object> arg in arguments) {
					if (arg.Value == null) continue;
					urlBuilder.AddParameterJSON(arg.Key, arg.Value);
				}
			}
			return urlBuilder.ToString();
		}

		private string __BuildURL(string apiFunctionName, string sid, IDictionary<string, object> arguments)
		{
			HttpURLBuilder urlBuilder = new HttpURLBuilder(host, port, basePath + apiFunctionName);
			if (sid != null)
				urlBuilder.AddParameterStr("sid", sid);
			if (arguments != null) {
				foreach (KeyValuePair<string, object> arg in arguments) {
					if (arg.Value == null)
						continue;
					urlBuilder.AddParameterJSON(arg.Key, arg.Value);
				}
			}
			return urlBuilder.ToString();
		}

		////////////////////////////////////////////////////////////////

		/*
		public Response GetRaw(string apiFunctionName, IDictionary<string, object> arguments)
		{
			string surl = __BuildURL(apiFunctionName, arguments);
			return __PerformGet(surl, true);
		}

		public Response GetRaw(string apiFunctionName, params KeyValuePair<string, object>[] arguments)
		{
			string surl = __BuildURL(apiFunctionName, arguments);
			return __PerformGet(surl, true);
		}
		*/

		public Response Get(string apiFunctionName, string sid, IDictionary<string, object> arguments)
		{
			string surl = __BuildURL(apiFunctionName, sid, arguments);
			return __PerformGet(surl);
		}

		public Response Get(string apiFunctionName, string sid, IList<KeyValuePair<string, object>> arguments)
		{
			string surl = __BuildURL(apiFunctionName, sid, arguments);
			return __PerformGet(surl);
		}

		public Response Get(string apiFunctionName, string sid, params KeyValuePair<string, object>[] arguments)
		{
			string surl = __BuildURL(apiFunctionName, sid, arguments);
			return __PerformGet(surl);
		}

		public BatchResponse PostBatch(string apiFunctionName, string sid, params KeyValuePair<string, JObject>[] requests)
		{
			if (apiFunctionName == null) throw new Exception("apiFunctionName == null");
			if (sid == null) throw new Exception("sid == null");
			if (requests.Length == 0) throw new Exception("requests.Length == 0");

			JArray commands = new JArray();
			foreach (KeyValuePair<string, JObject> request in requests) {
				commands.Add(new JObject(
					new JProperty(request.Key, request.Value)
					));
			}

			JObject requestObj = new JObject(
				new JProperty("sid", new JValue(sid)),
				new JProperty("commands", commands)
				);

			string surl = __BuildURL(apiFunctionName, null);

			StringWriter sw = new StringWriter();
			jsonSerializer.Serialize(sw, requestObj);

			Response response = __PerformPost(surl, sw.ToString());

			if (!response.Success) {
				return new BatchResponse(false, response.Object, response.Duration);
			}

			KeyValuePair<string, Response>[] responses = new KeyValuePair<string, Response>[requests.Length];
			if (response.Array != null) {
				JArray ja = (JArray)(response.Array);
				for (int i = 0; i < ja.Count; i++) {
					JObject jobj = (JObject)(ja[i]);
					JProperty p2 = jobj.Property(requests[i].Key);
					if (p2 == null) throw new Exception("Invalid response received!");
					responses[i] = new KeyValuePair<string, Response>(requests[i].Key, __ParseResponse(p2.Value));
				}
			} else {
				throw new Exception("Invalid response received!");
			}

			return new BatchResponse(true, responses, response.Duration);
		}

		private Response __ParseResponse(object obj)
		{
			if (obj is JObject) {
				JObject jobj = (JObject)obj;
				int duration = -1;
				JProperty p2 = jobj.Property("duration");
				if (p2 != null) {
					JValue v2 = (JValue)(p2.Value);
					if (v2.Value is long) {
						duration = (int)(long)(v2.Value);
					} else
					if (v2.Value is int) {
						duration = (int)(v2.Value);
					} else {
						throw new Exception("Invalid response received!");
					}
				}
				JProperty p = jobj.Property("success");
				bool bSuccess = true;
				if (p == null) {
					bSuccess = false;
					p = jobj.Property("error");
				}
				if (p == null) throw new Exception("Invalid response received!");
				JToken v = p.Value;
				if (v is JValue) {
					return new Response(bSuccess, (JValue)v, duration);
				} else
				if (v is JArray) {
					return new Response(bSuccess, (JArray)v, duration);
				} else
				if (v is JObject) {
					return new Response(bSuccess, (JObject)v, duration);
				} else {
					throw new Exception("Invalid response received!");
				}
			} else {
				throw new Exception("Invalid response received!");
			}
		}

		public Response Post(string apiFunctionName, JObject requestData)
		{
			string surl = __BuildURL(apiFunctionName, null);

			StringWriter sw = new StringWriter();
			jsonSerializer.Serialize(sw, requestData);

			return __PerformPost(surl, sw.ToString());
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

		////////////////////////////////////////////////////////////////
		// Methods: Core methods - these methods are called by all other methods
		////////////////////////////////////////////////////////////////

		private Response __PerformPost(string surl, string requestData)
		{
			byte[] rawData = UTF8Encoding.UTF8.GetBytes(requestData);

			if (DebuggingEnabled) {
				Console.Out.WriteLine("POST: " + surl);
				Console.Out.WriteLine("\tDATA (" + rawData.Length + " bytes): " + requestData);
			}

			HttpWebRequest request;
			try {
				request = (HttpWebRequest)(WebRequest.Create(surl));
			} catch (Exception ee) {
				throw new Exception("Invalid URL!");
			}
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = rawData.Length;
			using (Stream dataStream = request.GetRequestStream ()) {
				dataStream.Write(rawData, 0, rawData.Length);
			}

			HttpWebResponse response;
			try {
				response = (HttpWebResponse)(request.GetResponse());
			} catch (WebException ee) {
				response = (HttpWebResponse)(ee.Response);
			}

			if (response == null) throw new Exception("Failed to connect to remote host or connection prematurely closed by remote host!");

			using (Stream dataStream = response.GetResponseStream()) {
				using (StreamReader sr = new StreamReader(dataStream, Encoding.UTF8, true)) {
					string text = sr.ReadToEnd();
					if (DebuggingEnabled) Console.Out.WriteLine("\tRESPONSE DATA (" + response.StatusCode + "): " + text);
					object obj = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(text)));
					int statusCode = (int)(response.StatusCode);
					bool bSuccess = (statusCode >= 200) && (statusCode <= 299);
					return __ParseResponse(obj);
				}
			}
		}

		private Response __PerformGet(string surl)
		{
			if (DebuggingEnabled) Console.Out.WriteLine("GET: " + surl);

			HttpWebRequest request;
			try {
				request = (HttpWebRequest)(WebRequest.Create(surl));
			} catch (Exception ee) {
				throw new Exception("Invalid URL!");
			}
			request.Method = "GET";

			HttpWebResponse response;
			try {
				response = (HttpWebResponse)(request.GetResponse());
			} catch (WebException ee) {
				response = (HttpWebResponse)(ee.Response);
			}

			if (response == null) throw new Exception("Failed to connect to remote host or connection prematurely closed by remote host!");

			using (Stream dataStream = response.GetResponseStream()) {
				using (StreamReader sr = new StreamReader(dataStream, Encoding.UTF8, true)) {
					string text = sr.ReadToEnd();
					if (DebuggingEnabled) Console.Out.WriteLine("\tRESPONSE DATA (" + response.StatusCode + "): " + text);
					object obj = null;
					try {
						obj = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(text)));
					} catch (Exception ee) {
						throw new Exception("Invalid response received: " + (char)13 + (char)10 + text);
					}
					if (obj is JObject) {
						// int statusCode = (int)(response.StatusCode);
						// bool bSuccess = (statusCode >= 200) && (statusCode <= 299);
						return __ParseResponse(obj);
					} else {
						throw new Exception("Invalid response received: " + (char)13 + (char)10 + text);
					}
				}
			}
		}

	}

}
