using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using LibDictionaryClient.impl;


namespace LibDictionaryClient
{

	public class ExtraServerFunctionCollection
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		string prefix;
		Dictionary<string, ExtraServerFunction> functions;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		internal ExtraServerFunctionCollection(string prefix)
		{
			this.prefix = prefix;
			this.functions = new Dictionary<string, ExtraServerFunction>();
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public string Prefix
		{
			get {
				return prefix;
			}
		}

		public string[] Names
		{
			get {
				List<string> keys = new List<string>(functions.Keys);
				string[] ret = keys.ToArray();
				Array.Sort(ret);
				return ret;
			}
		}

		public ExtraServerFunction this[string name]
		{
			get {
				if (name == null) return null;

				ExtraServerFunction ret;
				if (functions.TryGetValue(name, out ret)) {
					return ret;
				} else {
					return null;
				}
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		internal ExtraServerFunction Create(DictionaryClient dictClient, JsonHttpRequester requester, string name)
		{
			ExtraServerFunction e = new ExtraServerFunction(this, dictClient, requester, name);
			functions.Add(name, e);
			return e;
		}

	}

}
