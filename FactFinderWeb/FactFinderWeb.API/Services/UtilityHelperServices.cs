
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using Microsoft.Data.SqlClient;
using System.Net;


using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Security.Cryptography;



namespace FactFinderWeb.API.Services
{
	public class UtilityHelperServices
	{

	 
		private readonly long _userID;
		private readonly HttpContext _httpContext;
        private readonly IConfiguration _config;
        private static readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();
		public UtilityHelperServices(  IHttpContextAccessor httpContextAccessor, IConfiguration config)
		{ 
			_httpContext = httpContextAccessor.HttpContext;
			var userIdStr = _httpContext.Session.GetString("UserId");
			_userID = Convert.ToInt64(userIdStr);
            _config = config;
        }

	 

        public static string GenerateSecureToken(int length = 32)
        {
            var tokenBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes)
                         .Replace("+", "")
                         .Replace("/", "")
                         .Replace("=", ""); // optional: remove URL-unsafe chars
        }

        public static string PasswordHash(string plainTextPassword)
		{
			return _passwordHasher.HashPassword(null, plainTextPassword);
			//for use string hashed = UtilHelperServices.HashPassword("MySecurePassword123");
		}

		public static bool PasswordVerify(string hashedPassword, string plainTextPassword)
		{
			var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, plainTextPassword);
			return result == PasswordVerificationResult.Success;
			// verify bool isValid = UtilHelperServices.VerifyPassword(storedHashedPassword, enteredPassword);
		}

		public static string GenerateRandomPassword(int length)
		{
			const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			char[] chars = new char[length];
			Random random = new Random();
			for (int i = 0; i < length; i++)
			{
				chars[i] = validChars[random.Next(validChars.Length)];
			}
			return new string(chars);
		}

	}


}
public class AppSettings
{
    public string WebAppURL { get; set; }
}
