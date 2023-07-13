using FTBAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FTBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        private readonly DbFootballChciasContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PlayerController(
            DbFootballChciasContext dbContext,
            IConfiguration configuration, // 此項不須透過注入
            IHttpContextAccessor httpContextAccessor
        )
        {
            _db = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        /*
        //[Authorize(Roles = "Medium")]
        //對應到登入時new Claim(ClaimTypes.Role, "Administrator")指定的角色權限
        //可以在Program.cs 設定 option.AccessDeniedPath，表示沒有權限的話重新導向到特定網址
        */
        public IEnumerable<Playerinfo> Get()
        {
            try {
                // _httpContextAccessor.HttpContext.User.Claims => 可取得包含登入者相關資料的網路綜合資料
                //Console.WriteLine(_httpContextAccessor.HttpContext.User);
                return _db.Playerinfos.ToList();
            } catch(Exception ex) {
                throw ex;
            }
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public ActionResult<Playerinfo> Get(Guid id)
        {
            try
            {
                Playerinfo  player = _db.Playerinfos.Find(id);
                if (player == null)
                {
                    return NotFound();
                }
                else
                {
                    return player;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // POST api/<ValuesController>
        [HttpPost]
        public ActionResult<Playerinfo> Post([FromBody] Playerinfo[] ayPlayerInfos)
        {
            try
            {
                using (var context = new DbFootballChciasContext())
                {
                    // seeding database
                    for(int player = 0; player < ayPlayerInfos.Length; player++)
                    {
                        Playerinfo oCurPlyrInfo = ayPlayerInfos[player];
                        if (!oCurPlyrInfo.Gender.GetType().Equals(typeof(string)))
                        {
                            return BadRequest("錯誤的性別型別");
                        }
                        else
                        {
                            if (oCurPlyrInfo.Gender.Length > 1) return BadRequest("錯誤的性別型別");
                        }
                        context.Playerinfos.Add(oCurPlyrInfo);
                    }
                    context.SaveChanges();
                }
                //return CreatedAtAction(nameof(Get), new { id = value.Id }, value);
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public ActionResult<Playerinfo> Put(Guid id, [FromBody] Playerinfo oPlayerinfo)
        {
            try
            {
                if (id != oPlayerinfo.Id)
                {
                    return BadRequest();
                }
                //Playerinfo player = _db.Playerinfos.Find(id);//這個註解打開會抱錯
                //if (player == null)
                //{
                //    return NotFound();
                //}

                _db.Entry(oPlayerinfo).State = EntityState.Modified;
                _db.SaveChanges();
                return NoContent();
            } catch(Exception ex)
            {
                if (!_db.Playerinfos.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "存取發生錯誤");
                }
            }
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            var info = _db.Playerinfos.Single(player => player.Id == id);
            _db.Playerinfos.Remove(info);
            _db.SaveChanges();
        }
    }
}
