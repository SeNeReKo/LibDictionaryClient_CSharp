using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using LibDictionaryClient.impl;


namespace LibDictionaryClient
{

	public class DictionaryClient
	{

#region class ServerFunctionGroups
		public class ServerFunctionGroups
		{

			private Dictionary<string, ExtraServerFunctionCollection> extraServerFunctions;

			internal ServerFunctionGroups(Dictionary<string, ExtraServerFunctionCollection> extraServerFunctions)
			{
				this.extraServerFunctions = extraServerFunctions;
			}

			public string[] GroupNames
			{
				get {
					List<string> keys = new List<string>(extraServerFunctions.Keys);
					string[] ret = keys.ToArray();
					Array.Sort(ret);
					return ret;
				}
			}

			public ExtraServerFunctionCollection this[string groupName]
			{
				get {
					if (groupName == null) return null;

					ExtraServerFunctionCollection collection;
					if (extraServerFunctions.TryGetValue(groupName, out collection)) {
						return collection;
					} else {
						return null;
					}
				}
			}

			public ExtraServerFunction GetE(string name)
			{
				string _prefix;
				string _name;

				int pos = name.IndexOf('_');
				if (pos >= 0) {
					_prefix = name.Substring(0, pos);
					_name = name.Substring(pos + 1);
				} else {
					_prefix = null;
					_name = name;
				}

				ExtraServerFunction f = null;
				if (_prefix != null) {
					ExtraServerFunctionCollection c = this[_prefix];
					if (c == null) throw new Exception("No such function group: '" + _prefix + "'");
					f = c[_name];
				} else {
					foreach (ExtraServerFunctionCollection c in extraServerFunctions.Values) {
						ExtraServerFunction f2 = c[_name];
						if (f2 == null) continue;
						if (f == null) f = f2;
						else throw new Exception("Multiple server functions with name '" + _name + "' available in different groups!");
					}
				}
				if (f == null) throw new Exception("No such function: '" + _name + "'");
				return f;
			}

			public JObject Invoke(string name, params KeyValuePair<string, object>[] arguments)
			{
				ExtraServerFunction f = GetE(name);
				return f.Invoke(arguments);
			}

		}
#endregion

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		JsonHttpRequester requester;

		Dictionary<string, DictionaryCollection> collectionObjects;
		Dictionary<string, ExtraServerFunctionCollection> extraServerFunctions;
		ServerFunctionGroups serverFunctionGroups;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public DictionaryClient(string host, int port, string userName, string pwd, bool bDebuggingEnabled)
		{
			ConnectionProfile = new LibDictionaryClient.ConnectionProfile(host, port, userName, pwd, bDebuggingEnabled);

			requester = new JsonHttpRequester(host, port, userName, pwd, bDebuggingEnabled);

			collectionObjects = new Dictionary<string, DictionaryCollection>();

			Reauthenticate();
		}

		public DictionaryClient(ConnectionProfile connectionProfile)
		{
			ConnectionProfile = connectionProfile;

			requester = new JsonHttpRequester(connectionProfile);

			collectionObjects = new Dictionary<string, DictionaryCollection>();

			Reauthenticate();
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public ConnectionProfile ConnectionProfile
		{
			get;
			private set;
		}

		public DictionaryCollection this[string collectionName]
		{
			get {
				return GetCollection(collectionName);
			}
		}

		public string[] Collections
		{
			get {
				return ListCollections();
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

		internal JsonHttpRequester Requester
		{
			get {
				return requester;
			}
		}

		internal string SID
		{
			get {
				return requester.SID;
			}
		}

		public ServerFunctionGroups ExtraFunctionGroups
		{
			get {
				return serverFunctionGroups;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public string[] ListCollections()
		{
			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.LISTCOLLECTIONS, true);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				string[] ret = new string[response.Array.Count];
				for (int i = 0; i < ret.Length; i++) {
					ret[i] = (string)(((JValue)(response.Array[i])).Value);
				}
				return ret;
			}
		}

		public bool CollectionExists(string collectionName)
		{
			foreach (string colName in ListCollections()) {
				if (colName.Equals(collectionName)) return true;
			}
			return false;
		}

		public DictionaryCollection CreateCollection(string collectionName)
		{
			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.CREATECOLLECTION, true, 
				new KeyValuePair<string, object>("colname", collectionName));
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				return GetCollection(collectionName, false);
			}
		}

		public void Reauthenticate()
		{
			requester.Reauthenticate();

			Dictionary<string, ExtraServerFunctionCollection> extraServerFunctions = new Dictionary<string, ExtraServerFunctionCollection>();

			foreach (ExtraServerFunctionInfo info in requester.ExtraServerFunctionInfos) {
				ExtraServerFunctionCollection collection;

				if (!extraServerFunctions.TryGetValue(info.Prefix, out collection)) {
					collection = new ExtraServerFunctionCollection(info.Prefix);
					extraServerFunctions.Add(info.Prefix, collection);
				}

				collection.Create(this, requester, info.Name);
			}

			this.extraServerFunctions = extraServerFunctions;
			this.serverFunctionGroups = new ServerFunctionGroups(extraServerFunctions);
		}

		public bool ContainsCollection(string collectionName)
		{
			lock (collectionObjects) {
				DictionaryCollection ret;
				if (collectionObjects.TryGetValue(collectionName, out ret)) {
					return true;
				} else {
					return false;
				}
			}
		}

		public DictionaryCollection GetCollection(string collectionName)
		{
			return GetCollection(collectionName, false);
		}

		public DictionaryCollection GetCreateCollection(string collectionName)
		{
			return GetCollection(collectionName, true);
		}

		public DictionaryCollection GetCollection(string collectionName, bool bCreateIfNotExists)
		{
			if (CollectionExists(collectionName)) {
				DictionaryCollection ret;
				lock (collectionObjects) {
					if (!collectionObjects.TryGetValue(collectionName, out ret)) {
						ret = new DictionaryCollection(this, collectionObjects, collectionName);
						collectionObjects.Add(collectionName, ret);
					}
				}
				return ret;
			} else {
				if (bCreateIfNotExists) {
					CreateCollection(collectionName);

					DictionaryCollection ret;
					lock (collectionObjects) {
						if (!collectionObjects.TryGetValue(collectionName, out ret)) {
							ret = new DictionaryCollection(this, collectionObjects, collectionName);
							collectionObjects.Add(collectionName, ret);
						}
					}
					return ret;
				} else {
					throw new Exception("Collection does not exist: " + collectionName);
				}
			}
		}

		public string[] ListPrivileges()
		{
			JsonHttpRequesterImpl.Response response = requester.Get(IProtocolFunctions.LISTPRIVILEGES, true);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				string[] ret = new string[response.Array.Count];
				for (int i = 0; i < ret.Length; i++) {
					ret[i] = ((JValue)(response.Array[i])).ToString();
				}
				return ret;
			}
		}

		public HashSet<string> ListPrivilegesAsSet()
		{
			HashSet<string> set = new HashSet<string>();
			string[] privileges = ListPrivileges();
			if (privileges == null) return null;
			foreach (string p in privileges) {
				set.Add(p);
			}
			return set;
		}

	}

}
