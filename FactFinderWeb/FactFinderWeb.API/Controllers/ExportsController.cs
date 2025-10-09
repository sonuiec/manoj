using FactFinderWeb.API.Models;
using FactFinderWeb.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactFinderWeb.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExportsController : ControllerBase
    {
        private readonly FactFinderDbContext _context;
        
        private readonly long _userID;
     
        private readonly JSONDataUtility _jsonData;
        private int updateRows;


        public ExportsController(FactFinderDbContext context, JSONDataUtility jsonDataUtility)
        {
            _context = context;
            _jsonData = jsonDataUtility;
           
        }


        [HttpGet("profile/{uid}")]
        public async Task<IActionResult> GetUserProfile(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return BadRequest(new
                {
                    message = "User ID is required."
                });
            }

            var user = await _context.TblffAwarenessProfileDetails.FirstOrDefaultAsync(a => a.UId == uid);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "No profile found for the specified user."
                });
            }

            var awarenessJson = await _jsonData.GetAwarenessJSON(user.ProfileId);
            if (awarenessJson == null)
            {
                return NotFound(new
                {

                    message = "No profile found for the specified user."
                });
            }

            Console.WriteLine(awarenessJson);
            object parsedJson;
            try
            {
                parsedJson = JsonSerializer.Deserialize<object>(awarenessJson);
            }
            catch (JsonException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Invalid JSON format returned by the data service."
                });
            }

            try
            {
                return Ok(new
                {
                    success = true,
                    uid = user.UId,
                    data = parsedJson
                });
            }
            catch (JsonException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Invalid JSON format returned by service."
                });
            }
        }



        [HttpGet("profile")]
        public async Task<IActionResult> ExportProfilesByDateRange( [FromQuery] DateTime? from,  [FromQuery] DateTime? to)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Both 'from' and 'to' dates are required."
                });
            }

            if (from > to)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "'from' date cannot be later than 'to' date."
                });
            }

            var users = await _context.TblffAwarenessProfileDetails
                .Where(a => a.CreateDate.Value.Date >= from.Value.Date && a.CreateDate.Value.Date <= to.Value.Date)
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No profiles found between {from:yyyy-MM-dd} and {to:yyyy-MM-dd}."
                });
            }

            // ✅ Collect all results here
            var allProfiles = new List<object>();

            foreach (var user in users)
            {
                var awarenessJson = "";
                try
                {
                     awarenessJson = await _jsonData.GetAwarenessJSON(user.ProfileId);

                    if (!string.IsNullOrWhiteSpace(awarenessJson))
                    {
                        //allProfiles.Add(new
                        //{
                        //    uid = user.UId,
                        //    status = "NotFound",
                        //    message = "No awareness data available."
                        //});
                        //continue;


                        // Parse the JSON to a dynamic object (optional)
                        var parsedJson = JsonSerializer.Deserialize<object>(awarenessJson);

                        allProfiles.Add(new
                        {
                            uid = user.UId,
                            status = "Success",
                            data = parsedJson
                        });
                    }
                    else
                    {

                    }
                }
                catch (JsonException ex)
                {
                }
                catch (Exception ex)
                {
                }
            }

            // ✅ Return everything together
            return Ok(new
            {
                success = true,
                fromDate = from?.ToString("yyyy-MM-dd"),
                toDate = to?.ToString("yyyy-MM-dd"),
                totalProfiles = allProfiles.Count,
                profiles = allProfiles
            });
        }



        [HttpGet("ping1")]
        public IActionResult Ping1()
        {
            return Ok("API is alive!");
        }
    }
}

