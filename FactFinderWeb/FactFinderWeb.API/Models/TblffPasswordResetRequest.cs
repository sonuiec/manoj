using System;
using System.Collections.Generic;

namespace FactFinderWeb.API.Models;

public partial class TblffPasswordResetRequest
{
    public int Id { get; set; }

    public long? UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime Expiration { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? CreatedAt { get; set; }
}
