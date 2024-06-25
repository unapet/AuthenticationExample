using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Authentication.Models.Users;
using Authentication.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authentication.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly ITokenService _tokenService;

        public UserController(
			UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager,
			SignInManager<IdentityUser> signInManager,
			ITokenService tokenService)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register(UserRegisterRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user != null) return Conflict("User with this email already exist.");

			var identity = new IdentityUser { Email = request.Email, UserName = request.FullName};

			var createAction = await _userManager.CreateAsync(identity, request.Password);
			if (!createAction.Succeeded)
			{
				List<IdentityError> errorList = createAction.Errors.ToList();
				string errors = "";

				foreach (var error in errorList)
				{
					errors = errors + error.Description.ToString() + "\n";
				}

				return Content(errors);
			}

            var newClaims = new List<Claim>
			{
				new("FullName", request.FullName),
			};

			await _userManager.AddClaimsAsync(identity, newClaims);

			await AddRoleToUser(identity, newClaims);

			var claimsIdentity = new ClaimsIdentity(new Claim[]
			{
				new(JwtRegisteredClaimNames.Sub, identity.Email ?? throw new InvalidOperationException()),
				new(JwtRegisteredClaimNames.Email, identity.Email ?? throw new InvalidOperationException()),
			});

			claimsIdentity.AddClaims(newClaims);

			return Ok("User was registered successfuly.");
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(UserLoginRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user is null) return BadRequest("User with this email cannot be find, plase register first.");

			var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
			if (!result.Succeeded) return BadRequest("Couldn't log in");
			if (!result.IsNotAllowed) return BadRequest("User is not allowed to login");

            var claims = await _userManager.GetClaimsAsync(user);

			var roles = await _userManager.GetRolesAsync(user);

			var claimsIdentity = new ClaimsIdentity(new Claim[]
			{
				new(JwtRegisteredClaimNames.Sub, user.Email ?? throw new InvalidOperationException()),
				new(JwtRegisteredClaimNames.Email, user.Email ?? throw new InvalidOperationException()),
			});

			claimsIdentity.AddClaims(claims);

			foreach (var role in roles)
			{
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
			}

			var token = _tokenService.CreateSecurityToken(claimsIdentity);

			var response = new AuthenticationResult(_tokenService.WriteToken(token));

			return Ok(response);
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null) return BadRequest("User with this email cannot be found.");

			string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
			var passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

			if (!passwordChangeResult.Succeeded)
			{
				List<IdentityError> errorList = passwordChangeResult.Errors.ToList();
				string errors = "";

				foreach (var error in errorList)
				{
					errors = errors + error.Description.ToString() + "\n";
				}

				return Content(errors);
			}

			return Ok("Password updated successfuly.");
		}

        private async Task AddRoleToUser(IdentityUser identity, List<Claim> claims)
		{
			var role = await _roleManager.FindByNameAsync("User");
			if (role == null)
			{
				role = new IdentityRole("User");
				await _roleManager.CreateAsync(role);
			}

			await _userManager.AddToRoleAsync(identity, "User");
			claims.Add(new Claim(ClaimTypes.Role, "User"));
		}

		public record AuthenticationResult(string Token);
    }
}
