using FactFinderWeb.API.Models;
using FactFinderWeb.API.Utils;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FactFinderWeb.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly FactFinderDbContext _context;
        
        private readonly long _userID;
     
        private readonly JSONDataUtility _jsonData;
        private int updateRows;


        public ExportController(FactFinderDbContext context, JSONDataUtility jsonDataUtility)
        {
            _context = context;
            _jsonData = jsonDataUtility;
           
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

            var user = await _context.TblffAwarenessProfileDetails.FindAsync(profileId);
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

