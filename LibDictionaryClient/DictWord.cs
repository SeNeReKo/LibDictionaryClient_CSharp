using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LibJSONExt;

using LibDictionaryClient.impl;


namespace LibDictionaryClient
{

	public class DictWord : JObject
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		public object Tag;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public DictWord()
		{
		}

		public DictWord(string lemma, string attrPOS, string originalData)
		{
			if (lemma != null) Add("lemma", new JValue(lemma));
			if (attrPOS != null) Add("pos", new JValue(attrPOS));
			if (originalData != null) Add("originalData", new JValue(originalData));
		}

		public DictWord(string word)
		{
			if (word != null) Add("word", new JValue(word));
		}

		public DictWord(JObject jsonData)
		{
			if (jsonData == null) throw new Exception("jsonData == null!");

			ID = jsonData.GetPropertyValueAsNormalizedString("id");
			// if (ID == null) throw new Exception("No ID!");

			foreach (JProperty p in jsonData.Properties()) {
				if (p.Name.Equals("id")) continue;
				object obj = p.Value;
				if (obj is JValue) obj = ((JValue)obj).Value;
				if (obj != null) {
					if (obj is JArray) {
						Add(p.Name, (JArray)obj);
					} else
					if (obj is JObject) {
						Add(p.Name, (JObject)obj);
					} else {
						Add(p.Name, new JValue(obj));
					}
				}
			}
		}

		private DictWord(string id, JObject jsonData)
		{
			if (jsonData == null) throw new Exception("jsonData == null!");

			this.ID = id;

			foreach (JProperty p in jsonData.Properties()) {
				if (p.Name.Equals("id")) continue;
				object obj = p.Value;
				if (obj is JValue) obj = ((JValue)obj).Value;
				if (obj != null) {
					if (obj is JArray) {
						Add(p.Name, (JArray)obj);
					} else
					if (obj is JObject) {
						Add(p.Name, (JObject)obj);
					} else {
						Add(p.Name, new JValue(obj));
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public string[] Keys
		{
			get {
				List<string> ret = new List<string>();

				foreach (JProperty p in Properties()) {
					ret.Add(p.Name);
				}

				return ret.ToArray();
			}
		}

		public string Lemma
		{
			get {
				return __GetStr("form", "lemma");
			}
			set {
				__SetStr("form", "lemma", value);
			}
		}

		public string Word
		{
			get {
				return __GetStr("word");
			}
			set {
				__SetStr("word", value);
			}
		}

		public string OriginalData
		{
			get {
				return __GetStr("originalData");
			}
			set {
				__SetStr("originalData", value);
			}
		}

		public string ID
		{
			get;
			private set;
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public void RemoveID()
		{
			ID = null;
		}

		private string __GetStr(string key)
		{
			return this.GetPropertyValueAsNormalizedString(key);
		}

		public bool RemoveByPath(params string[] pathElements)
		{
			JProperty p = __GetProperty(pathElements);
			if (p == null) return false;
			p.Remove();
			return true;
		}

		public string GetStrByPath(params string[] pathElements)
		{
			return __GetStr(pathElements);
		}

		public void SetStrByPath(string[] pathElements, string value)
		{
			__SetStr(pathElements, value);
		}

		public void SetStrByPath(IEnumerable<string> pathElements, string value)
		{
			__SetStr(pathElements, value);
		}

		public void SetStrByPath(params string[] elements)
		{
			string[] pathElements = elements.Take(elements.Length - 1).ToArray();
			__SetStr(pathElements, elements[elements.Length - 1]);
		}

		public void SetStrArrayByPath(IEnumerable<string> pathElements, string[] values)
		{
			__SetStrArray(pathElements, values);
		}

		public void SetStrArrayByPath(string[] pathElements, string[] values)
		{
			__SetStrArray(pathElements, values);
		}

		public string[] GetStrArrayByPath(params string[] pathElements)
		{
			return __GetStrArray(pathElements);
		}

		public JProperty GetPropertyByPath(params string[] pathElements)
		{
			return __GetProperty(pathElements);
		}

		public JProperty GetCreatePropertyByPath(params string[] pathElements)
		{
			return __GetCreateProperty(pathElements);
		}

		private string __GetStr(params string[] pathElements)
		{
			JProperty p = __GetProperty(pathElements);
			if (p == null) return null;
			if (p.Value is JValue) {
				JValue v = (JValue)(p.Value);
				if (v.Value is string) {
					return (string)(v.Value);
				}
			}
			return null;
		}

		private string[] __GetStrArray(params string[] pathElements)
		{
			JProperty p = __GetProperty(pathElements);
			if (p == null) return null;
			if (p.Value is JArray) {
				JArray v = (JArray)(p.Value);
				string[] ret = new string[v.Count];
				int i = -1;
				foreach (JToken vv in v) {
					i++;
					if (!(vv is JValue)) return null;
					object v2 = ((JValue)(vv)).Value;
					if (v2 == null) continue;
					if (v2 is string) {
						ret[i] = (string)(v2);
					} else {
						return null;
					}
				}
				return ret;
			}
			return null;
		}

		private JProperty __GetProperty(params string[] pathElements)
		{
			if (pathElements.Length == 0) return null;

			JProperty p = null;
			JObject o = this;
			for (int i = 0; i < pathElements.Length; i++) {
				p = o.Property(pathElements[i]);
				if (p == null) return null;
				if (p.Value is JObject) {
					o = (JObject)(p.Value);
					continue;
				}
				if (i < pathElements.Length - 1) {
					return null;
				}
			}

			if (p == null) return null;
			return p;
		}

		private JProperty __GetCreateProperty(params string[] pathElements)
		{
			if (pathElements.Length == 0) return null;

			JProperty p = null;
			JObject o = this;
			for (int i = 0; i < pathElements.Length; i++) {
				p = o.Property(pathElements[i]);
				if (p == null) {
					o = new JObject();
					o.Add(pathElements[i], o);
					continue;
				}
				if (p.Value is JObject) {
					o = (JObject)(p.Value);
					continue;
				}
				if (i < pathElements.Length) {
					o = new JObject();
					o.Add(pathElements[i], o);
					continue;
				}
			}

			if (p == null) return null;
			return p;
		}

		private JObject __GetCreateObject(params string[] pathElements)
		{
			if (pathElements.Length == 0) return null;

			JObject o = this;
			for (int i = 0; i < pathElements.Length; i++) {
				JProperty p = o.Property(pathElements[i]);
				if (p != null) {
					if (p.Value is JObject) {
						o = (JObject)(p.Value);
						continue;
					} else {
						throw new Exception("Path element " + pathElements[i] + " is not a JObject!");
					}
				}
				if (i < pathElements.Length) {
					JObject o2 = new JObject();
					o.Add(pathElements[i], o2);
					o = o2;
					continue;
				}
			}

			return o;
		}

		private void __SetStr(string key, string value)
		{
			LibJSONExt.StaticJObjectExt.SetStringValueTrim(this, key, value);
		}

		private void __SetStr(params string[] elements)
		{
			if (elements.Length < 2) throw new Exception("At least one key followed by a value must be specified!");
			string[] keys = new string[elements.Length - 2];
			for (int i = 0; i < keys.Length; i++) {
				keys[i] = elements[i];
			}
			JObject obj = __GetCreateObject(keys);
			LibJSONExt.StaticJObjectExt.SetStringValueTrim(obj, elements[elements.Length - 2], elements[elements.Length - 1]);
		}

		private void __SetStr(string[] pathElements, string value)
		{
			if (pathElements.Length < 1) throw new Exception("At least one key followed by a value must be specified!");
			string[] keys = new string[pathElements.Length - 1];
			for (int i = 0; i < keys.Length; i++) {
				keys[i] = pathElements[i];
			}
			JObject obj = __GetCreateObject(keys);
			LibJSONExt.StaticJObjectExt.SetStringValueTrim(obj, pathElements[pathElements.Length - 1], value);
		}

		private void __SetStr(IEnumerable<string> pathElements, string value)
		{
			List<string> pathElements2 = new List<string>(pathElements);
			if (pathElements2.Count < 1)
				throw new Exception("At least one key followed by a value must be specified!");
			string[] keys = new string[pathElements2.Count - 1];
			for (int i = 0; i < keys.Length; i++) {
				keys[i] = pathElements2[i];
			}
			JObject obj = __GetCreateObject(keys);
			LibJSONExt.StaticJObjectExt.SetStringValueTrim(obj, pathElements2[pathElements2.Count - 1], value);
		}

		private void __SetStrArray(string[] pathElements, string[] values)
		{
			if (pathElements.Length < 1) throw new Exception("At least one key followed by a value must be specified!");
			string[] keys = new string[pathElements.Length - 1];
			for (int i = 0; i < keys.Length; i++) {
				keys[i] = pathElements[i];
			}
			JObject obj = __GetCreateObject(keys);
			LibJSONExt.StaticJObjectExt.SetStringArray(obj, pathElements[pathElements.Length - 1], values);
		}

		private void __SetStrArray(IEnumerable<string> pathElements, string[] values)
		{
			List<string> pathElements2 = new List<string>(pathElements);
			if (pathElements2.Count < 1) throw new Exception("At least one key followed by a value must be specified!");
			string[] keys = new string[pathElements2.Count - 1];
			for (int i = 0; i < keys.Length; i++) {
				keys[i] = pathElements2[i];
			}
			JObject obj = __GetCreateObject(keys);
			LibJSONExt.StaticJObjectExt.SetStringArray(obj, pathElements2[pathElements2.Count - 1], values);
		}

		public JObject ToJObject(bool bAddID)
		{
			JObject newWordObj = new JObject();

			if (bAddID) {
				LibJSONExt.StaticJObjectExt.SetStringValueTrim(newWordObj, "id", ID);
			}

			foreach (JProperty kvp in Properties()) {
				newWordObj.Add(kvp.Name, kvp.Value);	// TODO: clone values
			}

			return newWordObj;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("ID=" + ID);
			
			foreach (JProperty kvp in Properties()) {
				sb.Append(", ");
				sb.Append(kvp.Name);
				sb.Append("=");
				sb.Append(__ToStr(kvp.Value));
			}

			return sb.ToString();
		}

		private string __ToStr(object value)
		{
			if (value == null) return "(null)";

			if (value is double) return ((double)value).ToString(Util.NFI_DOUBLE);
			if (value is string) {
				string s = (string)value;
				if (s.Length > 40) s = s.Substring(0, 37) + "...";
				return "\"" + s.Replace("\\", "\\\\") + "\"";
			}
			return value.ToString();
		}

		public void SetPoS(string value)
		{
			__SetStr("grammar", "pos", "type", value);
		}

		public void SetPoSSubtype(string subType)
		{
			__SetStr("grammar", "pos", "subtype", subType);
		}

		public string GetPoS()
		{
			return __GetStr("grammar", "pos", "type");
		}

		public DictWord Clone()
		{
			return new DictWord(ID, (JObject)(base.DeepClone()));
		}

		/// <summary>
		/// "Reverse"-cloning method: Fill the curent object with some copy of data provided by another object.
		/// </summary>
		/// <param name="other"></param>
		public void CopyFrom(DictWord other)
		{
			if (other == null) throw new Exception("other == null!");

			this.ID = other.ID;

			RemoveAll();

			foreach (JProperty p in other.Properties()) {
				if (p.Name.Equals("id")) continue;
				object obj = p.Value;
				if (obj is JValue) obj = ((JValue)obj).Value;
				if (obj != null) {
					if (obj is JArray) {
						Add(p.Name, (JArray)(((JArray)obj).DeepClone()));
					} else
					if (obj is JObject) {
						Add(p.Name, (JToken)(((JObject)obj).DeepClone()));
					} else {
						Add(p.Name, new JValue(obj));
					}
				}
			}
		}

		public AbstractJAccessor AccessWithEditInfo(string path, bool bCanHaveHistory, bool bAutoStoreOldValuesInHistory, IEditorInfo editor)
		{
			if (!JPath.IsValid(path)) throw new Exception("Path is not valid: " + path);
			return JPath.CreateAccessorWithEditInfo(this, path, bCanHaveHistory, bAutoStoreOldValuesInHistory, editor);
		}

	}

}

