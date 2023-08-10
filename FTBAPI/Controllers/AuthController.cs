using FTBAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using FTBAPI.HTTPResp;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FTBAPI.HTTPResp.Models;

namespace FTBAPI.Controllers
{
    [Route("api/auth/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly DbFootballChciasContext _db;
        private readonly IConfiguration _configuration;
        IDataProtector _protector;
        public class QueryUser
        {
            public string act { get; set; }
        }
        public struct LoginInfo
        {
            public string act { get; set; }
            public string pwd { get; set; }
        }

        public AuthController(
            DbFootballChciasContext service,
            IDataProtectionProvider provider,
            IConfiguration configuration
        ){
            _db = service;
            _configuration = configuration;
            //_protector = provider.CreateProtector(_configuration.GetValue<string>("Authentication:PD_PROTECTOR"));
            _protector = provider.CreateProtector(_configuration.GetConnectionString("PD_PROTECTOR"));
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
                //string[] ayAllowHosts =  _configuration.GetSection("Authentication:AllowedHosts").Get<string[]>();
                //int iAtmpt = _configuration.GetValue<int>("Authentication:Attempts");

                LoginInfo oLogin = loginInfo;
                var oUser = _db.UserAuthInfos.SingleOrDefault(user => user.Act == oLogin.act);

                if (oUser == null)
                {
                    resp = RespErrDoc.ERR_ACC_OR_PWD;
                    string strJsonResp = JsonSerializer.Serialize(resp);
                    return strJsonResp;
                }

                string dbUsrPwd = _protector.Unprotect(oUser.Pwd.Trim());

                if (dbUsrPwd == oLogin.pwd) {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, oUser.Act),
                        new Claim("FullName", oUser.Act),
                       // new Claim(ClaimTypes.Role, "Administrator")// 一般來說不會直接在這邊new，而是會從table中撈出
                    };

                    //1. 從資料庫撈取對應帳號的角色
                    //2. claims.Add(new Claim(ClaimTypes.Role, "撈出來的角色"));

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties() { ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(2) };
                    //這是針對局部單一一支的 cookie 期限，不同於 Program.cs 所設置
                    //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
                }
                else
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_ACC_OR_PWD);
                }
            }
            catch (Exception ex)
            {
                string strJsonResp = JsonSerializer.Serialize(RespErrDoc.ERR_ACC_OR_PWD);
                return strJsonResp;
            }
        }
        [HttpGet]
        private string CheckSession() {
            return JsonSerializer.Serialize(RespErrDoc.ERR_SESSION_EXPIRED);
        }
        [HttpDelete]
        public string Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
        }
        //[HttpGet("login")]
        [HttpGet]
        public string NoLogin()
        {
            return "未登入";
        }
        [HttpGet]
        public string CheckLogin() {
            bool bIsLogin = HttpContext.User.Identity.IsAuthenticated;
            if (bIsLogin)
            {
                string strCurUser = HttpContext.User.Identity.Name;
                var result = _db.UserAuthInfos.Where(userinfo => userinfo.Act == strCurUser).ToList();
                if (result.Count != 1)
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
                }

                CurrnetLoginUser clu = new CurrnetLoginUser();
                clu.Act = strCurUser;
                clu.Email = "待開發欄位";
                clu.Name = result[0].Name;

                var resp = RespSuccessDoc.OK_COMMON;
                resp.Result = clu;
                return JsonSerializer.Serialize(resp);
            }
            return JsonSerializer.Serialize(RespErrDoc.ERR_NO_LOGIN);
        }
    }
}
