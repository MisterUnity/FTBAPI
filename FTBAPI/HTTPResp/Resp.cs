using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTBAPI.HTTPResp
{
    struct CommonRespBody
    {
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
        public string? StatusMessage { get; set; }
        public string? OtherMessage { get; set; }
        public dynamic Data { get; set; }
    }

    class RespErrDoc
    {
        public static CommonRespBody ERR_SERVER = new CommonRespBody()
        {
            StatusCode = 0,
            ErrorCode = "E0000",
            ErrorMessage = "伺服器發生錯誤"
        };
        public static CommonRespBody ERR_ACC_OR_PWD = new CommonRespBody() {
            StatusCode = 0,
            ErrorCode = "E0001",
            ErrorMessage = "帳號或密碼錯誤"
        };
        public static CommonRespBody ERR_DATA_EXIST = new CommonRespBody()
        {
            StatusCode = 0,
            ErrorCode = "E0002",
            ErrorMessage = "資料已經存在"
        };
        public static CommonRespBody ERR_SESSION_EXPIRED = new CommonRespBody()
        {
            StatusCode = 0,
            ErrorCode = "E0003",
            ErrorMessage = "Session 已過期"
        };
        public static CommonRespBody ERR_NO_LOGIN = new CommonRespBody()
        {
            StatusCode = 0,
            ErrorCode = "E0004",
            ErrorMessage = "未登入"
        };
    }
    class RespSuccessDoc
    {
        public static CommonRespBody OK_COMMON = new CommonRespBody()
        {
            StatusCode = 1,
            StatusMessage = "Normal end."
        };
        public static CommonRespBody OK_ISLOGIN = new CommonRespBody()
        {
            StatusCode = 1,
            StatusMessage = "Normal end."
        };
    }
}
