using Azure.Identity;
using Azure.Storage.Blobs;
using FTBAPI.Dtos;
using FTBAPI.HTTPResp;
using FTBAPI.HTTPResp.Models;
using FTBAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Text.Json;

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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PlayerController(
            DbFootballChciasContext dbContext,
            IConfiguration configuration, // 此項不須透過注入
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _db = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
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
        //[HttpPost]
        //public ActionResult<Playerinfo> Post([FromBody] Playerinfo[] ayPlayerInfos)
        //{
        //    try
        //    {
        //        using (var context = new DbFootballChciasContext())
        //        {
        //            // seeding database
        //            for(int player = 0; player < ayPlayerInfos.Length; player++)
        //            {
        //                Playerinfo oCurPlyrInfo = ayPlayerInfos[player];
        //                if (!oCurPlyrInfo.Gender.GetType().Equals(typeof(string)))
        //                {
        //                    return BadRequest("錯誤的性別型別");
        //                }
        //                else
        //                {
        //                    if (oCurPlyrInfo.Gender.Length > 1) return BadRequest("錯誤的性別型別");
        //                }
        //                context.Playerinfos.Add(oCurPlyrInfo);
        //            }
        //            context.SaveChanges();
        //        }
        //        //return CreatedAtAction(nameof(Get), new { id = value.Id }, value);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
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
        [HttpPost("AddPlayer")]
        public async Task<string> PostUpResource([FromForm] IFormCollection form)
        {
            try
            {
                string gender = form["gender"].ToString();
                string[] genderOK = new string[] { "M", "F" };
                if (!genderOK.Contains(gender))
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_PARM_ERR);
                }

                //var result = form;
                var file = form.Files["photo"];

                Playerinfo oPlayerInfo = new Playerinfo();
                oPlayerInfo.Name = form["name"].ToString();
                oPlayerInfo.Gender = form["gender"].ToString();
                oPlayerInfo.Brithday = "";
                oPlayerInfo.Weight = form["weight"].ToString();
                oPlayerInfo.Height = form["height"].ToString();
                oPlayerInfo.Description = "";
                oPlayerInfo.Seniority = -1;

                Playerinfo player = _db.Playerinfos.SingleOrDefault(player => player.Name == oPlayerInfo.Name && player.Brithday == oPlayerInfo.Brithday);
                
                if (player != null)
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_DATA_EXIST);
                }

                string url = await UploadFromBinaryDataAsync(file);
                oPlayerInfo.Photo = url;
                _db.Playerinfos.Add(oPlayerInfo);
                _db.SaveChanges();

                UploadOK uploadOK = new UploadOK();
                uploadOK.Url = url;
                uploadOK.Name = oPlayerInfo.Name;
                RespSuccessDoc.OK_COMMON.Data = uploadOK;
                return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
            }
            catch(Exception ex)
            {
                RespErrDoc.ERR_SERVER.ErrorMessage = ex.ToString();
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
        private async Task<string> UploadFromBinaryDataAsync(IFormFile file)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("AZURE_BLOB_STORAGE_CONNECTION_STRING");
                string IdentityClientID = _configuration.GetConnectionString("IDENTITY_CLIENT_ID");
                DefaultAzureCredential defaultCredential;
                
                if (_webHostEnvironment.IsDevelopment())
                {
                    defaultCredential = new DefaultAzureCredential();
                    //defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions() { ManagedIdentityClientId = IdentityClientID });
                }
                else
                {
                    defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions() { ManagedIdentityClientId = IdentityClientID });
                }
                string containerName = "ftb-web";
                var subdirectory = "PlayerPhotos"; // 子目录名称

                if (file != null && file.Length > 0)
                {
                    //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                    BlobServiceClient blobServiceClient = new BlobServiceClient(
                        new Uri(connectionString), defaultCredential);
                
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    // 确保子目录存在
                    //var subdirectoryClient = containerClient.GetBlobClient(subdirectory);
                    //await subdirectoryClient.UploadAsync(new MemoryStream(), overwrite: true); //不用執行，會抱錯

                    string fileName = Path.GetFileName(file.FileName);
                    // 在子目录中创建 BlobClient
                    BlobClient blobClient = containerClient.GetBlobClient($"{subdirectory}/{fileName}");

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }
                    // Assuming you want to return the URL of the uploaded blob for reference
                    return blobClient.Uri.AbsoluteUri;
                }
                else
                {
                    return "No file was selected for upload.";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
