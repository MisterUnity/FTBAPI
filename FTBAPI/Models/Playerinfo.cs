using System;
using System.Collections.Generic;

namespace FTBAPI.Models;

public partial class Playerinfo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Height { get; set; } = null!;

    public string Weight { get; set; } = null!;

    public string? Position { get; set; }

    public string? Photo { get; set; }

    public string? Description { get; set; }

    public string? NextPosition { get; set; }

    public int? Seniority { get; set; }

    public string? Brithday { get; set; }
}
