using System;
using System.Collections.Generic;

namespace FTBAPI.Models;

public partial class Gameschedule
{
    public DateTime? Date { get; set; }

    public string? Field { get; set; }

    public string? Team2 { get; set; }

    public Guid Id { get; set; }

    public string? Team1 { get; set; }

    public int? IsHome { get; set; }
}
