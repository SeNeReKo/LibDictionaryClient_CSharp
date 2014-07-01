using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient
{

	public class ConnectionProfile
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

		public ConnectionProfile(string host, int port, string userName, string pwd)
		{
			this.Host = host;
			this.Port = port;
			this.UserName = userName;
			this.Password = pwd;
			this.DebuggingEnabled = false;
		}

		public ConnectionProfile(string host, int port, string userName, string pwd, bool bDebuggingEnabled)
		{
			this.Host = host;
			this.Port = port;
			this.UserName = userName;
			this.Password = pwd;
			this.DebuggingEnabled = bDebuggingEnabled;
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		public bool DebuggingEnabled
		{
			get;
			private set;
		}

		public string Host
		{
			get;
			private set;
		}

		public int Port
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

	}

}
