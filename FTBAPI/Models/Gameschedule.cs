using System;
using System.Collections.Generic;

namespace FTBAPI.Models;

public partial class Gameschedule
{
    public DateTime? Date { get; set; }

    public string? Field { get; set; }

    public string? Opponent { get; set; }

    public Guid Id { get; set; }
}
