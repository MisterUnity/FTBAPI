using System;
using System.Collections.Generic;

namespace FTBAPI.Models;

public partial class UserAuthInfo
{
    public string Act { get; set; } = null!;

    public string Pwd { get; set; } = null!;

    public byte? Usrlevl { get; set; }

    public DateTime? PwdExpired { get; set; }

    public DateTime? CreatedAt { get; set; }
}
