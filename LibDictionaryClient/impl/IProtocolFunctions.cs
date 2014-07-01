using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient.impl
{

	internal class IProtocolFunctions
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		public static readonly string NOOP = "core_noop";
		public static readonly string BATCH = "core_batch";
		public static readonly string LOGIN = "core_login";
		public static readonly string VERIFYSESSION = "core_verifysession";
		public static readonly string INFO = "core_info";
		public static readonly string LISTPRIVILEGES = "core_listprivileges";

		public static readonly string LISTCOLLECTIONS = "dict_listcollections";
		public static readonly string GETWORDBYID = "dict_getwordbyid";
		public static readonly string UPDATEWORDBYID = "dict_updatewordbyid";
		public static readonly string SETWORDBYID = "dict_setwordbyid";
		public static readonly string GETWORDSBYTAGS = "dict_getwordsbytags";
		public static readonly string GETWORDSBYREGEX = "dict_getwordsbyregex";
		public static readonly string COUNTWORDSBYTAGS = "dict_countwordsbytags";
		public static readonly string GETWORDIDS = "dict_getwordids";
		public static readonly string COUNTWORDS = "dict_countwords";
		public static readonly string GETCOLLECTIONINFO = "dict_getcollectioninfo";
		public static readonly string CREATEINDEX = "dict_createindex";
		public static readonly string ADDWORD = "dict_addword";
		public static readonly string ADDWORDIFNOTEXISTS = "dict_addwordifnotexists";
		public static readonly string DELETEWORDBYID = "dict_deletewordbyid";
		public static readonly string DELETEWORDSBYTAGS = "dict_deletewordsbytags";
		public static readonly string DELETEALLWORDS = "dict_deleteallwords";
		public static readonly string CREATECOLLECTION = "dict_createcollection";
		public static readonly string RENAMECOLLECTION = "dict_renamecollection";
		public static readonly string DESTROYCOLLECTION = "dict_destroycollection";

		public static readonly string GETMETADATA = "dict_getmetadata";
		public static readonly string PUTMETADATA = "dict_putmetadata";

		public static readonly string[] PREFIXES = new string[] { "dict", "core" };

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

	}

}
