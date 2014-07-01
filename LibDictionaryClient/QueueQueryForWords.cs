using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using LibDictionaryClient.impl;


namespace LibDictionaryClient
{

	/// <summary>
	/// This class implements a queue for bulk inserts. Whenever the queue is filled an automatic bulk insert operation
	/// is triggered.
	/// 
	/// Do not forget to call <code>flush()</code> after adding the last insert record: Otherwise you may find the last
	/// few inserts missing.
	/// </summary>
	public class QueueQueryForWords : IDisposable
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		DictionaryCollection api;
		int maxQueueLength;
		List<QueryElement> queue;
		OperationStatistics serverDurationMS;
		OperationStatistics clientDurationMS;
		List<QueryResult> results;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		internal QueueQueryForWords(DictionaryCollection api, int maxQueueLength)
		{
			this.maxQueueLength = maxQueueLength;
			this.api = api;
			this.queue = new List<QueryElement>();
			serverDurationMS = new OperationStatistics();
			clientDurationMS = new OperationStatistics();
			results = new List<QueryResult>();
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		/// <summary>
		/// Number of words in the queue currently waiting for an insert.
		/// </summary>
		public int Filled
		{
			get {
				return queue.Count;
			}
		}

		/// <summary>
		/// The maximum queue capacity. After this numer of elements a bulk insert is performed.
		/// </summary>
		public int MaxQueueLength
		{
			get {
				return maxQueueLength;
			}
		}

		/// <summary>
		/// Retrieve statistic information about the client-server communication.
		/// On each bulk insert the server responds with information about the duration it took
		/// on server side to perform the request. This duration is managed by the object returned
		/// here.
		/// </summary>
		public OperationStatistics ServerDurations
		{
			get {
				return serverDurationMS;
			}
		}

		/// <summary>
		/// Retrieve statistic information about the client-server communication.
		/// On each bulk insert the server responds with information about the duration it took
		/// on server side to perform the request. This duration is managed by the object returned
		/// by <code>ServerDuration</code>. For each duration information received the client side
		/// duration is noted as well. The object managing this client side information is returned
		/// here.
		/// </summary>
		public OperationStatistics ClientDurations
		{
			get {
				return clientDurationMS;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public IEnumerable<QueryResult> GetResults()
		{
			foreach (QueryResult r in results) {
				yield return r;
			}
			results.Clear();
		}

		public void Dispose()
		{
			try {
				Flush();
			} catch (Exception ee) {
			}
		}

		public void GetWordsByTags(string tag, string text)
		{
			if (tag == null) throw new Exception("No tag specified!");
			if (text == null) throw new Exception("No text specified!");

			this.queue.Add(new QueryElement(QueryElement.EnumQueryType.ByTag, tag, text));
			if (this.queue.Count >= maxQueueLength) {
				Flush();
			}
		}

		public void GetWordsByRegEx(string tag, string pattern)
		{
			if (tag == null) throw new Exception("No tag specified!");
			if (pattern == null) throw new Exception("No text specified!");

			this.queue.Add(new QueryElement(QueryElement.EnumQueryType.ByRegex, tag, pattern));
			if (this.queue.Count >= maxQueueLength) {
				Flush();
			}
		}

		/// <summary>
		/// Flush the queue: Perform all pending inserts.
		/// </summary>
		public void Flush()
		{
			if (queue.Count == 0) return;

			int durationMS_Server;
			long t0 = DateTime.Now.Ticks / 10000;
			QueryResult[] words = api.GetWords(out durationMS_Server, queue.ToArray());
			results.AddRange(words);
			long t1 = DateTime.Now.Ticks / 10000;
			int durationMS_Client = (int)(t1 - t0);
			if (durationMS_Server >= 0) {
				serverDurationMS.Add(durationMS_Server);
				clientDurationMS.Add(durationMS_Client);
			}
			queue.Clear();
		}

	}

}
