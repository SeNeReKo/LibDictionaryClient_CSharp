﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient
{

	public interface IDictEntryProcessor
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		bool Process(DictionaryCollection collection, DictWord dictWord);

	}

}
