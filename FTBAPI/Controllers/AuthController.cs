using FTBAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace FTBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly DbFootballChciasContext _db;

        IDataProtector _protector;
        private struct RegisterAuth
        {
            public string Act { get; set; }
            public string Pwd { get; set; }
            public int Usrlevl { get; set; }
        }

        public AuthController(DbFootballChciasContext service, IDataProtectionProvider provider)
        {
            _db = service;
            _protector = provider.CreateProtector("");
        }

        // POST: api/<AuthController>
        [HttpPost]
        public ActionResult Create([FromBody] string strJsonAuthInfo)
        {
            try
            {
                UserAuthInfo oRegisInfo = JsonSerializer.Deserialize<UserAuthInfo>(strJsonAuthInfo);
                oRegisInfo.Pwd = _protector.Protect(oRegisInfo.Pwd);
                _db.UserAuthInfos.Add(oRegisInfo);
                _db.SaveChanges();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
