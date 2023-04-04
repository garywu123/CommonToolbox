#region License

// author:         Gary Wu
// created:        11:21 PM
// description:

#endregion

namespace CommonUtils
{
	public struct SmbAccountInfo
	{
		#region Constructors

		public SmbAccountInfo(string username, string password)
		{
			Username = username;
			Password = password;
		}

		#endregion

		#region Properties

		public string Password { get; set; }

		public string Username { get; set; }

		#endregion
	}
}
