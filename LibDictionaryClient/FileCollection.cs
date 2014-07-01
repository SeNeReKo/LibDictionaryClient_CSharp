using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LibDictionaryClient
{

	public class FileCollection
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		DictionaryCollection collection;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public FileCollection(DictionaryCollection collection)
		{
			this.collection = collection;
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public FileEntry[] ListFiles()
		{
			List<FileEntry> ret = new List<FileEntry>();
			foreach (DictWord w in collection.GetEntryIterator()) {
				ret.Add(new FileEntry(((JValue)(w.Property("name").Value)).Value<string>(), (JObject)(w.GetValue("data"))));
			}
			return ret.ToArray();
		}

		public JObject LoadFileE(string name)
		{
			DictWord w = collection.GetWordByTags("name", name);
			if (w == null) throw new Exception("File not found: " + name);
			JObject obj = (JObject)(w.GetValue("data"));
			return obj;
		}

		public JObject LoadFile(string name)
		{
			DictWord w = collection.GetWordByTags("name", name);
			if (w == null) return null;
			JObject obj = (JObject)(w.GetValue("data"));
			return obj;
		}

		public void SaveFile(string name, JObject data)
		{
			DictWord w = null;
			if (data != null) {
				w = new DictWord();
				w.Add("name", name);
				w.Add("data", data);
			}

			DictWord oldWord = collection.GetWordByTags("name", name);
			if (oldWord != null) collection.DeleteWord(oldWord.ID);

			if (w != null) {
				int durationMS;
				DictWord[] words = collection.AddWords(out durationMS, w);
			}
		}

	}

}
