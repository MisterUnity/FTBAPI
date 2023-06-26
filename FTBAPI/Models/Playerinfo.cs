using FTBAPI.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FTBAPI.Models;

public partial class Playerinfo
{
    public Guid Id { get; set; }
    [NonSpace, MaxLength(10)]
    public string Name { get; set; } = null!;
    [NonSpace, MaxLength(1)]
    public string Gender { get; set; } = null!;
    [NonSpace, MaxLength(10)]
    public string Height { get; set; } = null!;
    [NonSpace, MaxLength(10)]
    public string Weight { get; set; } = null!;
    [NonSpace, MaxLength(10)]
    public string? Position { get; set; }
    [NonSpace, MaxLength(500)]
    public string? Photo { get; set; }
    [NonSpace, MaxLength(600)]
    public string? Description { get; set; }
    [NonSpace, MaxLength(10)]
    public string? NextPosition { get; set; }

    public int? Seniority { get; set; }
    [NonSpace, MaxLength(30)]
    public string? Brithday { get; set; }
}
