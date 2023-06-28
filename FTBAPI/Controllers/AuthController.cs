using FTBAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using FTBAPI.HTTPResp;

namespace FTBAPI.Controllers
{
    [Route("api/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly DbFootballChciasContext _db;

        IDataProtector _protector;
        public struct LoginInfo
        {
            public string account { get; set; }
            public string password { get; set; }
        }

        public AuthController(DbFootballChciasContext service, IDataProtectionProvider provider)
        {
            _db = service;
            _protector = provider.CreateProtector("protectorHAHAHA");
        }

        // POST: api/<AuthController>
        [HttpPost]
        public string Register([FromBody] UserAuthInfo authInfo)
        {
            try
            {
                //UserAuthInfo oRegisInfo = JsonSerializer.Deserialize<UserAuthInfo>(strJsonAuthInfo);
                var user = _db.UserAuthInfos.Find(authInfo.Act);
                if (user == null)
                {
                    authInfo.Pwd = _protector.Protect(authInfo.Pwd);
                    _db.UserAuthInfos.Add(authInfo);
                    _db.SaveChanges();
                    return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
                }
                else
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_DATA_EXIST);
                }
            }
            catch(Exception ex)
            {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
        [HttpPost]
        public string Login([FromBody] LoginInfo loginInfo)
        {
            CommonRespBody resp;
            try
            {
                LoginInfo oLogin = loginInfo;
                UserAuthInfo oUser = _db.UserAuthInfos.Single(user => user.Act == oLogin.account);

                if (oUser == null)
                {
                    resp = RespErrDoc.ERR_ACC_OR_PWD;
                    string strJsonResp = JsonSerializer.Serialize(resp);
                    return strJsonResp;
                }

                string dbUsrPwd = _protector.Unprotect(oUser.Pwd.Trim());

                if (dbUsrPwd == oLogin.password) {
                    resp = RespSuccessDoc.OK_COMMON;
                    string strJsonResp = JsonSerializer.Serialize(resp);
                    return strJsonResp;
                }
                else
                {
                    resp = RespErrDoc.ERR_ACC_OR_PWD;
                    string strJsonResp = JsonSerializer.Serialize(resp);
                    return strJsonResp;
                }
            }
            catch (Exception ex)
            {
                resp = RespErrDoc.ERR_SERVER;
                //resp.OtherMessage= ex.Message;
                string strJsonResp = JsonSerializer.Serialize(resp);
                return strJsonResp;
            }
        }
        [HttpGet]
        private string CheckSession() {
            return JsonSerializer.Serialize(RespErrDoc.ERR_SESSION_EXPIRED);
        }
    }
}
