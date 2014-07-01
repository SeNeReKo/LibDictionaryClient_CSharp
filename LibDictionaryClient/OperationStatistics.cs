using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient
{

	public class OperationStatistics
	{

		public struct Overview
		{
			public int Granularity;
			public int[] Values;

			public Overview(int[] values, int granularity)
			{
				this.Granularity = granularity;
				this.Values = values;
			}
		}

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static int[] GRANULARITIES = new int[] {
			1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 40, 50, 75, 100, 125, 150, 200, 250, 300, 400, 500, 750, 1000
		};

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		List<int> operations;
		long sum;
		int min;
		int max;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public OperationStatistics()
		{
			operations = new List<int>();
			min = Int32.MaxValue;
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public long Total
		{
			get {
				return sum;
			}
		}

		public int Count
		{
			get {
				return operations.Count;
			}
		}

		public double Average
		{
			get {
				if (operations.Count == 0) return 0;
				double d = sum;
				return d / operations.Count;
			}
		}

		public int Minimum
		{
			get {
				if (operations.Count == 0) return 0;
				return min;
			}
		}

		public int Maximum
		{
			get {
				return max;
			}
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public void Add(int value)
		{
			operations.Add(value);
			sum += value;
			if (value < min) min = value;
			if (value > max) max = value;
		}

		public Overview GetOverview()
		{
			if (operations.Count == 0) return new Overview();

			int granularity = GRANULARITIES[0];
			for (int i = 0; i < GRANULARITIES.Length; i++) {
				granularity = GRANULARITIES[i];
				if (max / granularity <= 30) break;
			}
			return GetOverview(granularity);
		}

		public Overview GetOverview(int granularity)
		{
			if (granularity <= 0) throw new Exception("Invalid granularity specified!");
			int[] values = new int[(max + granularity - 1) / granularity + 1];
			for (int i = 0; i < operations.Count; i++) {
				int n = operations[i] / granularity;
				values[n]++;
			}
			return new Overview(values, granularity);
		}

	}

}
