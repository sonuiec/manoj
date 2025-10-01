﻿using System;
using System.Collections.Generic;

namespace FactFinderWeb.API.Models;

public partial class TblFfAlertnesNewLiability
{
    public int Id { get; set; }

    public int? NetWorthId { get; set; }

    public long ProfileId { get; set; }

    public string? LiabilityName { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedAt { get; set; }
}
