using System;
namespace AuthServer.Core.Configuration
{
	public class Client
	{
		public string Id { get; set; }

		public List<String> Audiences { get; set; }
	}
}

