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

	public class DictionaryCollection
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static readonly int CHUNK_SIZE = 1000;

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		DictionaryClient parent;
		string collectionName;
		Dictionary<string, DictionaryCollection> collectionObjects;
		bool bIsValid;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		internal DictionaryCollection(DictionaryClient parent, Dictionary<string, DictionaryCollection> collectionObjects, string collectionName)
		{
			this.parent = parent;
			this.collectionObjects = collectionObjects;
			this.collectionName = collectionName;
			this.bIsValid = true;
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public string Name
		{
			get {
				return collectionName;
			}
		}

		public bool DebuggingEnabled
		{
			get {
				return parent.DebuggingEnabled;
			}
			set {
				parent.DebuggingEnabled = value;
			}
		}

		public DictionaryClient Parent
		{
			get {
				return parent;
			}
		}

		public bool IsDestroyed
		{
			get {
				return !bIsValid;
			}
		}

		public bool IsValid
		{
			get {
				return bIsValid;
			}
		}

		public int Count
		{
			get {
				return CountWords();
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public DictWord[] GetWordsByLemma(string lemma)
		{
			return GetWordsByTags("lemma", lemma);
		}

		public int CountWordsByLemma(string lemma)
		{
			return CountWordsByTags("lemma", lemma);
		}

		public bool ContainsWordsByLemma(string lemma)
		{
			return ContainsWordsByTags("lemma", lemma);
		}

		public DictWord[] GetWordsByRegEx(string tagname, string regexValue)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYREGEX, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("tagname", tagname),
				new KeyValuePair<string, object>("regexvalue", regexValue)
				);

			if (response.Success) {
				DictWord[] ret = new DictWord[response.Array.Count];
				for (int i = 0; i < response.Array.Count; i++) {
					ret[i] = new DictWord((JObject)(response.Array[i]));
				}
				return ret;
			} else {
				if (response.ErrorMessage != null)
					throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord GetWordByTags(string key, string value)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYTAGS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("filter", new JObject(
					new JProperty(key, value)))
				);

			if (response.Success) {
				if (response.Array.Count == 0) return null;
				return new DictWord((JObject)(response.Array[0]));
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] GetWordsByTags(string key, string value)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYTAGS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("filter", new JObject(
					new JProperty(key, value)))
				);

			if (response.Success) {
				DictWord[] ret = new DictWord[response.Array.Count];
				for (int i = 0; i < response.Array.Count; i++) {
					ret[i] = new DictWord((JObject)(response.Array[i]));
				}
				return ret;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] GetWordsByTags(out int durationMS, string key, string value)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYTAGS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("filter", new JObject(
					new JProperty(key, value)))
				);

			if (response.Success) {
				DictWord[] ret = new DictWord[response.Array.Count];
				for (int i = 0; i < response.Array.Count; i++) {
					ret[i] = new DictWord((JObject)(response.Array[i]));
				}
				durationMS = response.Duration;
				return ret;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		internal QueryResult[] GetWords(out int durationMS, QueryElement[] queryElements)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			if (queryElements.Length == 0) {
				durationMS = 0;
				return new QueryResult[0];
			}

			KeyValuePair<string, JObject>[] requests = new KeyValuePair<string, JObject>[queryElements.Length];
			for (int i = 0; i < queryElements.Length; i++) {
				QueryElement qe = queryElements[i];
				switch (qe.type) {
					case QueryElement.EnumQueryType.ByTag:
						requests[i] = new KeyValuePair<string, JObject>(IProtocolFunctions.GETWORDSBYTAGS,
							new JObject(
								new JProperty("colname", collectionName),
								new JProperty("filter", new JObject(
									new JProperty(qe.tag, qe.text)))
								)
							);
						break;
					case QueryElement.EnumQueryType.ByRegex:
						requests[i] = new KeyValuePair<string, JObject>(IProtocolFunctions.GETWORDSBYREGEX,
							new JObject(
								new JProperty("colname", collectionName),
								new JProperty("tagname", qe.tag),
								new JProperty("regexvalue", qe.text)
								)
							);
						break;
					default:
						throw new ImplementationErrorException();
				}
			}

			JsonHttpRequesterImpl.BatchResponse response = parent.Requester.PostBatch(IProtocolFunctions.BATCH, true, requests);
			if (response.Success) {
				QueryResult[] retAll = new QueryResult[queryElements.Length];
				for (int i = 0; i < queryElements.Length; i++) {
					if (response.Responses[i].Value.Success) {

						JsonHttpRequesterImpl.Response sr = response.Responses[i].Value;
						DictWord[] ret = new DictWord[sr.Array.Count];
						for (int j = 0; j < sr.Array.Count; j++) {
							ret[j] = new DictWord((JObject)(sr.Array[j]));
						}

						retAll[i] = new QueryResult(queryElements[i].text, ret);
					} else {
						JsonHttpRequesterImpl.Response r = response.Responses[i].Value;
						throw new Exception(r.ErrorID + " " + r.ErrorMessage);
					}
				}
				durationMS = response.Duration;
				return retAll;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] GetWordsByTags(string key1, string value1, string key2, string value2)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYTAGS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("filter", new JObject(
					new JProperty(key1, value1),
					new JProperty(key2, value2)
					))
				);

			if (response.Success) {
				DictWord[] ret = new DictWord[response.Array.Count];
				for (int i = 0; i < response.Array.Count; i++) {
					ret[i] = new DictWord((JObject)(response.Array[i]));
				}
				return ret;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public bool ContainsWordsByTags(string key, string value)
		{
			return CountWordsByTags(key, value) > 0;
		}

		public int CountWordsByTags(string key, string value)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDSBYTAGS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("filter", new JObject(
					new JProperty(key, value)))
				);

			if (response.Success) {
				JValue v = (JValue)(response.Object.Property("success").Value);
				return (int)(v.Value);
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord AddWord(DictWord word)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			if (word.ID != null) throw new Exception("DictWord already in data base!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.ADDWORD, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("data", word.ToJObject(false))
				);
			if (response.Success) {
				return new DictWord(response.Object);
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] AddWords(out int durationMS, params DictWord[] words)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			if (words.Length == 0) {
				durationMS = 0;
				return words;
			}

			for (int i = 0; i < words.Length; i++) {
				if (words[i].ID != null) throw new Exception("DictWord already in data base!");
			}

			KeyValuePair<string, JObject>[] requests = new KeyValuePair<string, JObject>[words.Length];
			for (int i = 0; i < words.Length; i++) {
				requests[i] = new KeyValuePair<string, JObject>(IProtocolFunctions.ADDWORD,
					new JObject(
						new JProperty("colname", collectionName),
						new JProperty("data", words[i].ToJObject(false))
						)
					);
			}

			JsonHttpRequesterImpl.BatchResponse response = parent.Requester.PostBatch(IProtocolFunctions.BATCH, true, requests);
			if (response.Success) {
				for (int i = 0; i < words.Length; i++) {
					if (response.Responses[i].Value.Success) {
						words[i] = new DictWord(response.Responses[i].Value.Object);
					} else {
						JsonHttpRequesterImpl.Response r = response.Responses[i].Value;
						throw new Exception(r.ErrorID + " " + r.ErrorMessage);
					}
				}
				durationMS = response.Duration;
				return words;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] SetWords(out int durationMS, params DictWord[] words)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			if (words.Length == 0) {
				durationMS = 0;
				return words;
			}

			for (int i = 0; i < words.Length; i++) {
				if (words[i].ID == null) throw new Exception("DictWord not in data base!");
			}

			KeyValuePair<string, JObject>[] requests = new KeyValuePair<string, JObject>[words.Length];
			for (int i = 0; i < words.Length; i++) {
				requests[i] = new KeyValuePair<string, JObject>(IProtocolFunctions.SETWORDBYID,
					new JObject(
						new JProperty("id", words[i].ID),
						new JProperty("colname", collectionName),
						new JProperty("data", words[i].ToJObject(false))
						)
					);
			}

			JsonHttpRequesterImpl.BatchResponse response = parent.Requester.PostBatch(IProtocolFunctions.BATCH, true, requests);
			if (response.Success) {
				for (int i = 0; i < words.Length; i++) {
					if (response.Responses[i].Value.Success) {
						words[i] = new DictWord(response.Responses[i].Value.Object);
					} else {
						JsonHttpRequesterImpl.Response r = response.Responses[i].Value;
						throw new Exception(r.ErrorID + " " + r.ErrorMessage);
					}
				}
				durationMS = response.Duration;
				return words;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public DictWord[] GetWords(string[] ids, out int durationMS)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			if (ids.Length == 0) {
				durationMS = 0;
				return new DictWord[0];
			}

			KeyValuePair<string, JObject>[] requests = new KeyValuePair<string, JObject>[ids.Length];
			for (int i = 0; i < ids.Length; i++) {
				requests[i] = new KeyValuePair<string, JObject>(IProtocolFunctions.GETWORDBYID,
					new JObject(
						new JProperty("colname", collectionName),
						new JProperty("id", new JValue(ids[i]))
						)
					);
			}

			JsonHttpRequesterImpl.BatchResponse response = parent.Requester.PostBatch(IProtocolFunctions.BATCH, true, requests);
			if (response.Success) {
				DictWord[] ret = new DictWord[ids.Length];
				for (int i = 0; i < ids.Length; i++) {
					if (response.Responses[i].Value.Success) {
						ret[i] = new DictWord(response.Responses[i].Value.Object);
					} else {
						ret[i] = null;
					}
				}
				durationMS = response.Duration;
				return ret;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public void DeleteAllWords()
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.DELETEALLWORDS, true,
				new KeyValuePair<string, object>("colname", collectionName)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public string[] GetWordIDs(int ofs, int count)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDIDS, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("ofs", ofs),
				new KeyValuePair<string, object>("count", count)
				);
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

		public int CountWords()
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.COUNTWORDS, true,
				new KeyValuePair<string, object>("colname", collectionName)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				if (response.Value is long) {
					return (int)(long)(response.Value);
				} else
				if (response.Value is int) {
					return (int)(response.Value);
				}
				throw new Exception("Error!");
			}
		}

		public void Destroy()
		{
			if (!bIsValid) return;

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.DESTROYCOLLECTION, true,
				new KeyValuePair<string, object>("colname", collectionName)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				bIsValid = false;
				lock (collectionObjects) {
					collectionObjects.Remove(collectionName);
				}
			}
		}

		/// <summary>
		/// Create a queue for inserting new words into the collection.
		/// </summary>
		/// <param name="maxQueueLength">The maximum queue length. After adding this number of elements the queue is flushed
		/// automatically. (Do not forget to manually flush the queue if you did not (yet) reach this number of elements during
		/// a bulk insert.)</param>
		/// <returns>Returns the queue.</returns>
		public QueueInsertNewWords CreateQueueInsertNewWords(int maxQueueLength)
		{
			return new QueueInsertNewWords(this, maxQueueLength);
		}

		public QueueUpdateExistingWords CreateQueueUpdateExistingWords(int maxQueueLength)
		{
			return new QueueUpdateExistingWords(this, maxQueueLength);
		}

		public QueueQueryForWords CreateQueueQueryForWords(int maxQueueLength)
		{
			return new QueueQueryForWords(this, maxQueueLength);
		}

		public bool CreateIndex(string fieldName)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.CREATEINDEX, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("fieldname", fieldName)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				if (response.Value is bool) {
					return (bool)(response.Value);
				}
				throw new Exception("Error!");
			}
		}

		public DictWord GetWord(string id)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETWORDBYID, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("id", id)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null)
					throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				return new DictWord(response.Object);
			}
		}

		public bool DeleteWord(string id)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.DELETEWORDBYID, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("id", id)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null)
					throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				if (response.Value is Int32) {
					return (int)(response.Value) > 0;
				} else
				if (response.Value is Int64) {
					return (long)(response.Value) > 0;
				} else {
					throw new Exception("Unexpected reply format!");
				}
			}
		}

		public DictWord UpdateWord(string id, params KeyValuePair<string, object>[] fieldsToModify)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JObject setter = new JObject();
			JArray unsetter = new JArray();
			if (fieldsToModify != null) {
				foreach (KeyValuePair<string, object> kvp in fieldsToModify) {
					string key = kvp.Key;
					if (key == null) continue;
					key = key.TrimEnd();
					if (key.Length == 0) continue;

					if (kvp.Value == null) {
						unsetter.Add(new JValue(key));
						setter.Remove(key);
					} else {
						setter.Remove(key);
						setter.Add(key, new JValue(kvp.Value));
						unsetter.Remove(key);
					}
				}
			}

			List<KeyValuePair<string, object>> arguments = new List<KeyValuePair<string, object>>();
			if (setter.Count > 0) arguments.Add(new KeyValuePair<string, object>("set", setter));
			if (unsetter.Count > 0) arguments.Add(new KeyValuePair<string, object>("unset", unsetter));

			if (arguments.Count == 0) {
				return GetWord(id);
			}

			arguments.Add(new KeyValuePair<string, object>("id", id));
			arguments.Add(new KeyValuePair<string, object>("colname", collectionName));

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.UPDATEWORDBYID, true, arguments);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				return new DictWord(response.Object);
			}
		}

		public DictWord SetWord(DictWord word)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.SETWORDBYID, true,
				new KeyValuePair<string, object>("id", word.ID),
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("data", word.ToJObject(false))
				);
			if (!response.Success) {
				if (response.ErrorMessage != null)
					throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				return new DictWord(response.Object);
			}
		}

		public IEnumerable<string> GetIDIterator()
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			int ofs = 0;
			while (true) {
				string[] ids = GetWordIDs(ofs, CHUNK_SIZE);
				ofs += CHUNK_SIZE;

				if (ids.Length == 0) yield break;

				for (int i = 0; i < ids.Length; i++) {
					yield return ids[i];
				}
			}
		}

		public IEnumerable<DictWord> GetEntryIterator(int chunkSize)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			List<string> idBuffer = new List<string>();
			int durationMS;

			IEnumerable<string> e = GetIDIterator();
			foreach (string id in e) {
			//while (e.MoveNext()) {
				//idBuffer.Add(e.Current);
				idBuffer.Add(id);
				if (idBuffer.Count == chunkSize) {
					DictWord[] words = GetWords(idBuffer.ToArray(), out durationMS);
					foreach (DictWord w in words) {
						if (w != null) yield return w;
					}
					idBuffer.Clear();
				}
			}

			if (idBuffer.Count > 0) {
				DictWord[] words = GetWords(idBuffer.ToArray(), out durationMS);
				foreach (DictWord w in words) {
					if (w != null) yield return w;
				}
			}
		}

		public IEnumerable<DictWord> GetEntryIterator(int chunkSize, IProgress progressIndicator)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			List<string> idBuffer = new List<string>();
			int durationMS;

			string[] allIDs = (new List<string>(GetIDIterator())).ToArray();
			int counter = 0;

			foreach (string id in allIDs) {
				//idBuffer.Add(e.Current);
				idBuffer.Add(id);
				if (idBuffer.Count == chunkSize) {
					DictWord[] words = GetWords(idBuffer.ToArray(), out durationMS);
					foreach (DictWord w in words) {
						if (w != null) {
							counter++;
							if (progressIndicator != null) {
								progressIndicator.UpdateProgress(null, ((double)(counter)) / ((double)(allIDs.Length)));
							}
							yield return w;
						}
					}
					idBuffer.Clear();
				}
			}

			if (idBuffer.Count > 0) {
				DictWord[] words = GetWords(idBuffer.ToArray(), out durationMS);
				foreach (DictWord w in words) {
					if (w != null) {
						counter++;
						if (progressIndicator != null) {
							progressIndicator.UpdateProgress(null, ((double)(counter)) / ((double)(allIDs.Length)));
						}
						yield return w;
					}
				}
			}
		}

		public IEnumerable<DictWord> GetEntryIterator()
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			return GetEntryIterator(CHUNK_SIZE);
		}

		public IEnumerable<DictWord> GetEntryIterator(IProgress progressIndicator)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			return GetEntryIterator(CHUNK_SIZE, progressIndicator);
		}

		public void Rename(string newName)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.RENAMECOLLECTION, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("newcolname", newName)
				);
			if (!response.Success) {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			} else {
				lock (collectionObjects) {
					collectionObjects.Remove(collectionName);
					collectionName = newName;
					collectionObjects.Add(newName, this);
				}
			}
		}

		/// <summary>
		/// Performs an update on all entries. For each entry the specified processor is called.
		/// If the processor returns <code>true</code> it means that the processor has modified the
		/// dictionary entry. This method will automatically update modified entries.
		/// </summary>
		/// <typeparam name="T">The class of the processor.</typeparam>
		/// <param name="processor">The processor used to process all entries</param>
		/// <returns>Returns the processor specified for convenience.</returns>
		public T ProcessAllEntriesWith<T>(T processor)
			where T : IDictEntryProcessor
		{
			return ProcessAllEntriesWith<T>(processor, true);
		}

		/// <summary>
		/// Performs an update on all entries. For each entry the specified processor is called.
		/// If the processor returns <code>true</code> it means that the processor has modified the
		/// dictionary entry. This method will automatically update modified entries.
		/// </summary>
		/// <typeparam name="T">The class of the processor.</typeparam>
		/// <param name="processor">The processor used to process all entries</param>
		/// <returns>Returns the processor specified for convenience.</returns>
		public T ProcessAllEntriesWith<T>(T processor, IProgress progressIndicator)
			where T : IDictEntryProcessor
		{
			return ProcessAllEntriesWith<T>(processor, true, progressIndicator);
		}

		/// <summary>
		/// Performs an update on all entries. For each entry the specified processor is called.
		/// If the processor returns <code>true</code> it means that the processor has modified the
		/// dictionary entry. If <code>bAllowWriting</code> is <code>true</code> this method will
		/// automatically update modified entries.
		/// </summary>
		/// <typeparam name="T">The class of the processor.</typeparam>
		/// <param name="processor">The processor used to process all entries</param>
		/// <param name="bAllowWriting">If <code>false</code> is specified here all modifications are ignored
		/// and no changes are applied to the collection.</param>
		/// <returns>Returns the processor specified for convenience.</returns>
		public T ProcessAllEntriesWith<T>(T processor, bool bAllowWriting)
			where T : IDictEntryProcessor
		{
			QueueUpdateExistingWords queue = bAllowWriting ? CreateQueueUpdateExistingWords(200) : null;
			foreach (DictWord dictWord in GetEntryIterator()) {
				if (processor.Process(this, dictWord)) {
					if (bAllowWriting) queue.Add(dictWord);
				}
			}
			if (bAllowWriting) queue.Flush();
			return processor;
		}

		/// <summary>
		/// Performs an update on all entries. For each entry the specified processor is called.
		/// If the processor returns <code>true</code> it means that the processor has modified the
		/// dictionary entry. If <code>bAllowWriting</code> is <code>true</code> this method will
		/// automatically update modified entries.
		/// </summary>
		/// <typeparam name="T">The class of the processor.</typeparam>
		/// <param name="processor">The processor used to process all entries</param>
		/// <param name="bAllowWriting">If <code>false</code> is specified here all modifications are ignored
		/// and no changes are applied to the collection.</param>
		/// <returns>Returns the processor specified for convenience.</returns>
		public T ProcessAllEntriesWith<T>(T processor, bool bAllowWriting, IProgress progressIndicator)
			where T : IDictEntryProcessor
		{
			QueueUpdateExistingWords queue = bAllowWriting ? CreateQueueUpdateExistingWords(200) : null;
			foreach (DictWord dictWord in GetEntryIterator(progressIndicator)) {
				if (processor.Process(this, dictWord)) {
					if (bAllowWriting) queue.Add(dictWord);
				}
			}
			if (bAllowWriting) queue.Flush();
			return processor;
		}

		/// <summary>
		/// Performs an update on all entries. For each entry the specified processor is called.
		/// If the processor returns <code>true</code> it means that the processor has modified the
		/// dictionary entry. If <code>bAllowWriting</code> is <code>true</code> this method will
		/// automatically update modified entries.
		/// </summary>
		/// <typeparam name="T">The class of the processor.</typeparam>
		/// <param name="processor">The processor used to process all entries</param>
		/// <param name="bAllowWriting">If <code>false</code> is specified here all modifications are ignored
		/// and no changes are applied to the collection.</param>
		/// <returns>Returns the processor specified for convenience.</returns>
		public T ProcessAllEntriesWith<T>(T processor, bool bAllowWriting, Flag terminationFlag, IProgress progressIndicator)
			where T : IDictEntryProcessor
		{
			QueueUpdateExistingWords queue = CreateQueueUpdateExistingWords(200);
			int countItems = 0;
			if (progressIndicator != null) {
				countItems = CountWords();
				progressIndicator.UpdateProgress("0/" + countItems, 0);
			}
			int i = 0;
			int maxSteps = queue.MaxQueueLength;
			int stepCount = 0;
			foreach (DictWord dictWord in GetEntryIterator()) {
				if (terminationFlag.Value) break;

				if (progressIndicator != null) {
					i++;
					stepCount++;
					if (stepCount == maxSteps) {
						string text = i + "/" + countItems;
						progressIndicator.UpdateProgress(text, i / (double)countItems);
						stepCount = 0;
					}
				}
				if (processor.Process(this, dictWord)) {
					if (bAllowWriting) queue.Add(dictWord);
				}
			}

			if (progressIndicator != null) {
				if (terminationFlag.Value) {
					progressIndicator.UpdateProgress("Terminated prematurely.", null);
				} else {
					progressIndicator.UpdateProgress(countItems + "/" + countItems, 1);
				}
			}
			if (bAllowWriting) {
				if (terminationFlag.Value) queue.DisposeWithoutFlush();
				else queue.Flush();
			}
			return processor;
		}

		public JObject GetMetaData(string key)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.GETMETADATA, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("key", key));

			if (response.Success) {
				return response.Object;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

		public void PutMetaData(string key, JObject value)
		{
			if (!bIsValid) throw new Exception("Collection has been destroyed!");

			JsonHttpRequesterImpl.Response response = parent.Requester.Get(IProtocolFunctions.PUTMETADATA, true,
				new KeyValuePair<string, object>("colname", collectionName),
				new KeyValuePair<string, object>("key", key),
				new KeyValuePair<string, object>("value", value));

			if (response.Success) {
				return;
			} else {
				if (response.ErrorMessage != null) throw new Exception(response.ErrorMessage);
				throw new Exception("Error!");
			}
		}

	}

}
