using FTBAPI.HTTPResp;
using FTBAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FTBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly DbFootballChciasContext _db;
        public ScheduleController(DbFootballChciasContext dbContext)
        {
            _db = dbContext;
        }
        [HttpGet]
        public string GetAllSchedule()
        {
            try
            {
                Gameschedule[] gameschedules = _db.Gameschedules.ToArray();
                CommonRespBody body = RespSuccessDoc.OK_COMMON;
                body.Result = gameschedules;
                return JsonSerializer.Serialize(body);
            }catch(Exception ex)
            {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
        [HttpPost]
        public string AddSchedule([FromBody] Gameschedule[] gameschedule) {
            int i = 0;
            try
            {
                for (i = 0; i< gameschedule.Length; i++)
                {
                    var schedule = (from rows in _db.Gameschedules
                                    where rows.Date == gameschedule[i].Date && rows.Field == gameschedule[i].Field && rows.Team1 == gameschedule[i].Team2
                                    select rows).SingleOrDefault();

                    if (schedule != null)
                    {
                        CommonRespBody respBody = RespErrDoc.ERR_DATA_EXIST;
                        respBody.ErrorMessage = "不可新增相同資料";
                        return JsonSerializer.Serialize(respBody);
                    }

                    _db.Gameschedules.Add(gameschedule[i]);
                    _db.SaveChanges();
                }
                
                return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
            }
            catch (Exception e) {
                CommonRespBody resp = RespErrDoc.ERR_SERVER;
                resp.ErrorMessage = $"傳入的陣列第{i}筆發生錯誤";
                return JsonSerializer.Serialize(resp);
            }
        }
        [HttpDelete]
        public string DeleteSchedule(Guid id)
        {
            try
            {
                var schedule = (from rows in _db.Gameschedules
                               where rows.Id == id
                               select rows).SingleOrDefault();
                if (schedule != null)
                {
                    _db.Gameschedules.Remove(schedule);
                    _db.SaveChanges();
                    return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
                }
                CommonRespBody respBody = RespErrDoc.ERR_NO_DATA;
                respBody.StatusMessage = "指定的資源不存在或已刪除!";
                return JsonSerializer.Serialize(RespErrDoc.ERR_NO_DATA);
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
    }
}
