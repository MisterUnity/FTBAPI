using System;
using System.Collections.Generic;

namespace FTBAPI.Models;

public partial class Playergamesinfo
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short? IsHome { get; set; }

    public string? Place { get; set; }

    public DateTime Date { get; set; }

    public string? Team { get; set; }

    public string? Opponent { get; set; }

    public int? Goal { get; set; }

    public int? ToShoot { get; set; }

    public int? PenaltyKick { get; set; }

    public int? FreeKick { get; set; }

    public int? CornerBall { get; set; }

    public int? HandBall { get; set; }

    public int? Offside { get; set; }

    public int? TechnicalFoul { get; set; }

    public int? YellowCard { get; set; }

    public int? RedCard { get; set; }
}
