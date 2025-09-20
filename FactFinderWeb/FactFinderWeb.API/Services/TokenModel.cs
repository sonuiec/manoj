namespace FactFinderWeb.API.Services
{
    public class TokenModel
    {
        public int UserId { get; set; }
        public string GivenName { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsCaregiver { get; set; }
        public DateTime Expiration { get; set; }
        public bool CaregiverDB { get; set; }

    }
}
