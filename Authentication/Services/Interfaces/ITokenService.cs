using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Authentication.Services.Interfaces
{
	public interface ITokenService
	{
		public SecurityToken CreateSecurityToken(ClaimsIdentity identity);
		public string WriteToken(SecurityToken token);
	}
}
