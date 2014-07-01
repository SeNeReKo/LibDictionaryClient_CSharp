using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LibJSONExt;


namespace LibDictionaryClient.impl
{

	public class JsonHttpRequester
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static string BASE_PATH = "/api/json/";

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		JsonHttpRequesterImpl requester;
		string sid;
		string userName;
		string pwd;
		ExtraServerFunctionInfo[] extraFunctionInfos;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public JsonHttpRequester(string host, int port, string userName, string pwd, bool bDebuggingEnabled)
		{
			requester = new JsonHttpRequesterImpl(host, port, BASE_PATH, bDebuggingEnabled);
			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.INFO, null);
			if (!response.Success) throw new Exception("Unexpected response received: Failed to contact server!");

			this.userName = userName;
			this.pwd = pwd;

			extraFunctionInfos = __ExtractExtraServerFunctions(response.Object);
		}

		public JsonHttpRequester(ConnectionProfile connectionProfile)
		{
			requester = new JsonHttpRequesterImpl(
				connectionProfile.Host,
				connectionProfile.Port,
				BASE_PATH,
				connectionProfile.DebuggingEnabled);
			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.INFO, null);
			if (!response.Success) throw new Exception("Unexpected response received: Failed to contact server!");

			this.userName = connectionProfile.UserName;
			this.pwd = connectionProfile.Password;

			extraFunctionInfos = __ExtractExtraServerFunctions(response.Object);
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public ExtraServerFunctionInfo[] ExtraServerFunctionInfos
		{
			get {
				return extraFunctionInfos;
			}
		}

		internal string SID
		{
			get {
				return sid;
			}
		}

		public bool DebuggingEnabled
		{
			get {
				return requester.DebuggingEnabled;
			}
			set {
				requester.DebuggingEnabled = value;
			}
		}

		public bool HasSessionID
		{
			get {
				return sid != null;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		private ExtraServerFunctionInfo[] __ExtractExtraServerFunctions(JObject obj)
		{
			List<ExtraServerFunctionInfo> extra = new List<ExtraServerFunctionInfo>();

			JArray a = obj._GetPropertyObject("api")._GetPropertyArray("functions");

			for (int i = 0; i < a.Count; i++) {
				JToken t = a[i];
				if (!(t is JValue)) continue;
				object v = ((JValue)t).Value;
				if (v is string) {
					string s = (string)v;

					bool bFound = false;
					for (int j = 0; j < IProtocolFunctions.PREFIXES.Length; j++) {
						if (s.StartsWith(IProtocolFunctions.PREFIXES[j] + "_")) {
							bFound = true;
							break;
						}
					}
					if (bFound) continue;

					int pos = s.IndexOf('_');
					if (pos <= 0) continue;

					string prefix = s.Substring(0, pos);
					string name = s.Substring(pos + 1);

					extra.Add(new ExtraServerFunctionInfo(prefix, name));
				}
			}

			return extra.ToArray();
		}

		public void Ping()
		{
			JsonHttpRequesterImpl.Response response = Get(IProtocolFunctions.INFO, false);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Ping failed!");
			}
		}

		public JsonHttpRequesterImpl.Response Get(string apiFunctionName, bool bSendSID, params KeyValuePair<string, object>[] arguments)
		{
			JsonHttpRequesterImpl.Response response = requester.Get(apiFunctionName, bSendSID ? sid : null, arguments);
			if (bSendSID && !response.Success && (response.ErrorID == EnumErrorID.InvalidSessionID)) {
				Reauthenticate();
				response = requester.Get(apiFunctionName, sid, arguments);
			}
			return response;
		}

		public JsonHttpRequesterImpl.Response Get(string apiFunctionName, bool bSendSID, List<KeyValuePair<string, object>> arguments)
		{
			JsonHttpRequesterImpl.Response response = requester.Get(apiFunctionName, bSendSID ? sid : null, arguments);
			if (bSendSID && !response.Success && (response.ErrorID == EnumErrorID.InvalidSessionID)) {
				Reauthenticate();
				response = requester.Get(apiFunctionName, sid, arguments);
			}
			return response;
		}

		public JsonHttpRequesterImpl.BatchResponse PostBatch(string apiFunctionName, bool bSendSID, params KeyValuePair<string, JObject>[] requests)
		{
			JsonHttpRequesterImpl.BatchResponse response = requester.PostBatch(apiFunctionName, bSendSID ? sid : null, requests);
			if (bSendSID && !response.Success && (response.ErrorID == EnumErrorID.InvalidSessionID)) {
				Reauthenticate();
				response = requester.PostBatch(apiFunctionName, sid, requests);
			}
			return response;
		}

		public void Reauthenticate()
		{
			sid = null;

			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.LOGIN, null,
				new KeyValuePair<string, object>("u", userName),
				new KeyValuePair<string, object>("p", pwd)
				);
			if (!response.Success) throw new Exception("Authentification failed!");
			sid = response.Object.Property("sid").Value.ToString();
		}

	}

}
