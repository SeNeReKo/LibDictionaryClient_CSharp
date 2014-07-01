using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LibDictionaryClient.impl;

using LibJSONExt;


namespace LibDictionaryClient
{

	public class ExtraServerFunction
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		ExtraServerFunctionCollection functionCollection;
		DictionaryClient dictClient;
		JsonHttpRequester requester;
		string name;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		internal ExtraServerFunction(ExtraServerFunctionCollection functionCollection,
			DictionaryClient dictClient, JsonHttpRequester requester, string name)
		{
			this.functionCollection = functionCollection;
			this.dictClient = dictClient;
			this.requester = requester;
			this.name = name;
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public string Prefix
		{
			get {
				return functionCollection.Prefix;
			}
		}

		public string Name
		{
			get {
				return name;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public JObject Invoke(params KeyValuePair<string, object>[] arguments)
		{
			string fullFunctionName = Prefix + "_" + name;

			JsonHttpRequesterImpl.Response response = requester.Get(fullFunctionName, true, arguments);
			if (response.Success) {
				return response.Object;
			} else {
				string errMsg = "Failed to invoke extra function '" + fullFunctionName + "'!";
				if (response.ErrorMessage != null) {
					errMsg += " Reason: " + response.ErrorMessage;
				}
				throw new Exception(errMsg);
			}
		}

	}

}
