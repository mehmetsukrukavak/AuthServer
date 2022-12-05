using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Services.Services
{
	public static class SignService
	{
		public static SecurityKey GetSymetricKey(string securityKey)
		{
			return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
		}
	}
}

