using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorWasmCookieAuth.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmCookieAuth.Api.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase {
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger) {
			_logger = logger;
		}
		[HttpGet("check")]
		public IActionResult CheckTokenFromCookie() {
			// ��Ű���� access_token ��������
			if (Request.Cookies.TryGetValue("access_token", out var token)) {
				// ��ū�� ������ ��� ó��
				return Ok(new { message = "��ū ����", token });
			}

			return Unauthorized(new { message = "��ū ����" });
		}


		[HttpGet]
		public IEnumerable<WeatherForecast> GetPublic() {
			return CreateData();
		}

		[HttpGet("protected")]
		[Authorize]
		public IEnumerable<WeatherForecast> GetProtected() {
			return CreateData();
		}

		private IEnumerable<WeatherForecast> CreateData() {
			var rng = new Random();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
				Date = DateTime.Now.AddDays(index),
				TemperatureC = rng.Next(-20, 55),
				Summary = Summaries[rng.Next(Summaries.Length)]
			})
			.ToArray();
		}


		[HttpGet("login")]
		public IActionResult Login() {
			// ���⼭�� ���÷� ������ �α��� ���� (�����δ� DB �˻� �� �ʿ�)

			var token = GenerateJwtToken("hyunsoft");

			var cookieOptions = new CookieOptions {
				HttpOnly = true,
				//Secure = true, // HTTPS������ ���۵�
				SameSite = SameSiteMode.Lax,
				Expires = DateTimeOffset.UtcNow.AddHours(1)
			};

			Response.Cookies.Append("access_token", token, cookieOptions);

			return Ok(new { message = "�α��� ����" });

		}

		private string GenerateJwtToken(string username) {
			// JWT ���� ���� (������ ����)
			var handler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes("your-super-long-secret-key-1234567890");

			var descriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[]
				{
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Role, "admin"),
			}),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = handler.CreateToken(descriptor);
			return handler.WriteToken(token);
		}
	}
}
