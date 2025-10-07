using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MessageService.Models;


namespace AuthService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		#region ctor
		private readonly AuthDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IConfiguration _configuration;
		private readonly IEmailConfirmator _emailConfirmator;
		public AuthController(AuthDbContext db, UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailConfirmator emailConfirmator)
		{
			_db = db;
			_userManager = userManager;
			_configuration = configuration;
			_emailConfirmator = emailConfirmator;
		}
		#endregion

		#region Register
		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterRequest request)
		{
			var existingUser = _db.Users.FirstOrDefault(u => u.Email == request.Email);
			if (existingUser != null)
			{
				return BadRequest(new { errors = new Dictionary<string, string[]>
				{ { "Email", new[] { "Email already in use." } } } });
			}

			ApplicationUser user = new ApplicationUser
			{
				UserName = request.UserName,
				Email = request.Email,
			};

			var result = _userManager.CreateAsync(user, request.Password).Result;

			if (!result.Succeeded)
			{
				Dictionary<string, string[]> errors = new ();

				foreach (var error in result.Errors)
				{
					errors[error.Code] = new[] { error.Description };					
				}

				return BadRequest(new { errors = errors});
			}

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			// build base url from HttpContext OR use fallback
			var baseUrl = HttpContext?.Request?.Host.HasValue == true
				? $"{Request.Scheme}://{Request.Host}"
				: "http://localhost"; // fallback for tests

			await _emailConfirmator.SendConfirmationEmailAsync(user, token, baseUrl);

			return Ok(new { message = "You successfully registered! Please confirm your email." });
		}
		#endregion

		#region confirmemail
		[HttpGet("confirmemail")]
		public IActionResult ConfirmEmail (string userId, string token)
		{
			if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
			{
				return BadRequest("User ID and Token are required");
			}

			var user = _db.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null)
			{
				return BadRequest("User was not found");
			}
			var result = _userManager.ConfirmEmailAsync(user, token).Result;

			if (!result.Succeeded)
			{
				string errors = string.Empty;

				foreach (var error in result.Errors)
				{
					errors += $"Error: {error.Code} - {error.Description}\n";
				}

				return BadRequest(errors);
			}

			return Ok("Email confirmed successfully!");
		}
		#endregion

		#region Login
		[HttpPost("login")]
		public  IActionResult Login(LoginRequest request)
		{
			var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
			if (user == null)
			{
				return BadRequest(new { errors = new Dictionary<string, string[]> { { "Email", new[] { "Invalid email or password." } } } });
			}
			bool passwordValid = _userManager.CheckPasswordAsync(user, request.Password).Result;
			if (!passwordValid)
			{
				return BadRequest(new { errors = new Dictionary<string, string[]> { { "Password", new[] { "Invalid email or password." } } } });
			}

			//Issie a JWT
			string token = GenerateToken(user);

			return Ok(new {token = token});
		}
		#endregion

		#region GenerateToken
		private string GenerateToken(ApplicationUser user)
		{
			//claims
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			};

			//key+credentials
			var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			//token
			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(7),
				signingCredentials: creds
				);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		#endregion

		#region UserExist
		[HttpGet("userexist/{userId}")]
		public IActionResult UserExist(string userId)
		{
			var user = _db.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null)
			{
				return NotFound("User was not found");
			}
			return Ok("User found");
		}
		#endregion

		#region FillContacts
		[HttpPost("fillcontacts")]
		public async Task<IActionResult> FillContacts(List<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				var user = await _db.Users.FindAsync(contact.UserId);
				if (user != null)
				{
					contact.UserName = user.UserName;
					contact.Email = user.Email;
				}
			}

			return Ok(contacts);
		}

		#endregion

	}
}
