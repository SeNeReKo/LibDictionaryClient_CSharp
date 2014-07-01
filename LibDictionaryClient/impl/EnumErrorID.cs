using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LibDictionaryClient.impl
{

	public enum EnumErrorID
	{

		Unknown,
		AuthError,
		General,
		InsufficientPrivileges,
		Internal,
		InvalidRequest,
		InvalidSessionID,
		NoSessionID

	}

}
