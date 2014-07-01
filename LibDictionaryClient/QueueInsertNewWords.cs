using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient
{

	/// <summary>
	/// This class implements a queue for bulk inserts. Whenever the queue is filled an automatic bulk insert operation
	/// is triggered.
	/// 
	/// Do not forget to call <code>flush()</code> after adding the last insert record: Otherwise you may find the last
	/// few inserts missing.
	/// </summary>
	public class QueueInsertNewWords : IDisposable
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		DictionaryCollection api;
		int maxQueueLength;
		List<DictWord> queue;
		OperationStatistics serverDurationMS;
		OperationStatistics clientDurationMS;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		internal QueueInsertNewWords(DictionaryCollection api, int maxQueueLength)
		{
			this.maxQueueLength = maxQueueLength;
			this.api = api;
			this.queue = new List<DictWord>();
			serverDurationMS = new OperationStatistics();
			clientDurationMS = new OperationStatistics();
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

		public void Dispose()
		{
			try {
				Flush();
			} catch (Exception ee) {
			}
		}

		/// <summary>
		/// Schedule a new dictionary word for insert.
		/// </summary>
		/// <param name="word">The new dictionary word to insert</param>
		public void Add(DictWord word)
		{
			if (word.ID != null) throw new Exception("Word already has an ID!");

			this.queue.Add(word);
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
			DictWord[] words = api.AddWords(out durationMS_Server, queue.ToArray());
			long t1 = DateTime.Now.Ticks / 10000;
			int durationMS_Client = (int)(t1 - t0);
			if (durationMS_Server >= 0) {
				serverDurationMS.Add(durationMS_Server);
				clientDurationMS.Add(durationMS_Client);
			}
			if (words == null) throw new Exception();
			for (int i = 0; i < words.Length; i++) {
				if (words[i] == null) throw new Exception();
			}
			queue.Clear();
		}

	}

}
