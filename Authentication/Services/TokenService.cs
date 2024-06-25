using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Authentication.Options;
using Authentication.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Services
{
	public class TokenService : ITokenService
	{
		private readonly JwtSettings? _settings;
		private readonly byte[] _key;

		public TokenService(IOptions<JwtSettings?> jwtOptions)
		{
			_settings = jwtOptions.Value;
			ArgumentNullException.ThrowIfNull(_settings);
			ArgumentNullException.ThrowIfNull(_settings.SigningKey);
			ArgumentNullException.ThrowIfNull(_settings.Audiences);
			ArgumentNullException.ThrowIfNull(_settings.Audiences[0]);
			ArgumentNullException.ThrowIfNull(_settings.Issuer);
			_key = Encoding.ASCII.GetBytes(_settings.SigningKey);
		}

		private static JwtSecurityTokenHandler TokenHandler => new();

		public SecurityToken CreateSecurityToken(ClaimsIdentity identity)
		{
			if (identity == null) { 
				throw new ArgumentNullException(nameof(identity));
			}

			var tokenDescriptor = GetTokenDescriptor(identity);
			return TokenHandler.CreateToken(tokenDescriptor);
		}

		public string WriteToken(SecurityToken token)
		{
			return TokenHandler.WriteToken(token);
		}

		private SecurityTokenDescriptor GetTokenDescriptor(ClaimsIdentity identity)
		{
			return new SecurityTokenDescriptor()
			{
				Subject = identity,
				Expires = DateTime.Now.AddHours(2),
				Audience = _settings!.Audiences?[0]!,
				Issuer = _settings.Issuer,
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key),
					SecurityAlgorithms.HmacSha256Signature)
			};
		}
	}
}
