using FactFinderWeb.API.Models;
using FactFinderWeb.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FactFinderWeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly FactFinderDbContext _context;
        public AuthController(JwtService jwtService, FactFinderDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // ✅ Check if the request matches validation rules
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns validation errors
            }

            // If model is valid, continue with login logic
            var user = await _context.TblFfAdminUsers
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Accesskey == "api" && u.AccountStatus== "Active");
            bool isValid = UtilityHelperServices.PasswordVerify(user.Password, model.Password);
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Name, "Yes");

            return Ok(new { token });
        }


        public class LoginModel
        {
            [Required(ErrorMessage = "Email is required")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }
        }
    }
}
