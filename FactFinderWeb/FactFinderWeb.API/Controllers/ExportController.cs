using FactFinderWeb.Models;
using FactFinderWeb.Services;
using FactFinderWeb.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FactFinderWeb.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ResellerBoyinawebFactFinderWebContext _context;
        private readonly AwarenessServices _AwarenessServices;
        private readonly long _userID;
        private readonly HttpContext _httpContext;
        private readonly JSONDataUtility _jsonData;
        private int updateRows;


        public ExportController(ResellerBoyinawebFactFinderWebContext context, JSONDataUtility jsonDataUtility, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jsonData = jsonDataUtility;
            _httpContext = httpContextAccessor.HttpContext;
        }


        [HttpGet("userprofile/{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new
                {
                    message = "User ID is required."
                });
            }

            if (!long.TryParse(id, out var profileId))
            {
                return BadRequest(new
                {
                    message = "Invalid user ID format."
                });
            }

            var user = await _context.TblFfRegisterUsers.FindAsync(profileId);
            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var awarenessJson = await _jsonData.GetAwarenessJSON(profileId);
            if (awarenessJson == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            Console.WriteLine(awarenessJson);

            try
            {
                return Content(awarenessJson, "application/json");
            }
            catch (JsonException)
            {
                return StatusCode(500, new
                {
                    message = "Invalid JSON format returned by service."
                });
            }
        }

        [HttpGet("ping1")]
        public IActionResult Ping1()
        {
            return Ok("API is alive!");
        }
    }
}

