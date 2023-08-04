using Azure.Identity;
using Azure.Storage.Blobs;
using FTBAPI.Dtos;
using FTBAPI.HTTPResp;
using FTBAPI.HTTPResp.Models;
using FTBAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;
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
        // 進攻資料欄位定義
        private readonly ColumnName _offensiveCol1 = new ColumnName() { Field = "Date", Header = "時間" };
        private readonly ColumnName _offensiveCol2 = new ColumnName() { Field = "ToShoot", Header = "射門次數" };
        private readonly ColumnName _offensiveCol3 = new ColumnName() { Field = "CornerBall", Header = "角球" };
        private readonly ColumnName _offensiveCol4 = new ColumnName() { Field = "GoalKick", Header = "球門球" };
        private readonly ColumnName _offensiveCol5 = new ColumnName() { Field = "Header", Header = "頭球" };
        private readonly ColumnName _offensiveCol6 = new ColumnName() { Field = "PenaltyKick", Header = "點球" };
        private readonly ColumnName _offensiveCol7 = new ColumnName() { Field = "FreeKick", Header = "自由球" };
        //防守資料欄位定義
        private readonly ColumnName _defensiveCol1 = new ColumnName() { Field = "Date", Header = "時間" };
        private readonly ColumnName _defensiveCol2 = new ColumnName() { Field = "BlockTackle", Header = "正面搶截" };
        private readonly ColumnName _defensiveCol3 = new ColumnName() { Field = "SlideTackle", Header = "鏟球" };
        private readonly ColumnName _defensiveCol4 = new ColumnName() { Field = "ToIntercept", Header = "截球" };
        private readonly ColumnName _defensiveCol5 = new ColumnName() { Field = "BodyCheck", Header = "身體阻擋" };
        private readonly ColumnName _defensiveCol6 = new ColumnName() { Field = "FairCharge", Header = "合理衝撞" };

        private ColumnName[] _ofsColsDefault;
        private ColumnName[] _dfsColsDefault;

        private class Player {
            public Guid ID { get; set; }
            public string Name { get; set; }
        }
        private class ColumnName
        {
            public string Field { get; set; }
            public string Header { get; set; }
        }
        private class OffensiveDataContent
        {
            public string Date { get; set; }
            public string ToShoot { get; set; }
            public string CornerBall { get; set; }
            public string GoalKick { get; set; }
            public string Header { get; set; }
            public string PenaltyKick { get; set; }
            public string FreeKick { get; set; }
        }
        private class DefensiveDataContent
        {
            public string Date { get; set; }
            public string BlockTackle { get; set; }
            public string SlideTackle{ get; set; }
            public string ToIntercept{ get; set; }
            public string BodyCheck { get; set; }
            public string FairCharge { get; set; }
        }
        private class OffensiveData
        {
            public ColumnName[] ColumnName { get; set; }
            public OffensiveDataContent[] Data { get; set; }
        }
        private class DefensiveData
        {
            public ColumnName[] ColumnName { get; set; }
            public DefensiveDataContent[] Data { get; set; }
        }
        private class PlayerData
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Photo { get; set; }
            public string Age { get; set; }
            public string Height { get; set; }
            public string Weight { get; set; }
            public string Position { get; set; }
            public string Team { get; set; }
            public string Description { get; set; }
            public OffensiveData OffensiveData { get; set; }
            public DefensiveData DefensiveData { get; set; }
        }
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

            _ofsColsDefault = new ColumnName[] {
                _offensiveCol1,
                _offensiveCol2,
                _offensiveCol3,
                _offensiveCol4,
                _offensiveCol5,
                _offensiveCol6,
                _offensiveCol7,
            };
            _dfsColsDefault = new ColumnName[]
            {
                _defensiveCol1,
                _defensiveCol2,
                _defensiveCol3,
                _defensiveCol4,
                _defensiveCol5,
                _defensiveCol6
            };
        }

        // GET: api/<ValuesController>
        [HttpGet]
        /*
        //[Authorize(Roles = "Medium")]
        //對應到登入時new Claim(ClaimTypes.Role, "Administrator")指定的角色權限
        //可以在Program.cs 設定 option.AccessDeniedPath，表示沒有權限的話重新導向到特定網址
        */
        public string Get()
        {
            try {
                // _httpContextAccessor.HttpContext.User.Claims => 可取得包含登入者相關資料的網路綜合資料
                //Console.WriteLine(_httpContextAccessor.HttpContext.User);
                CommonRespBody resp;
                List<Playerinfo> listPlayerInfo = _db.Playerinfos.ToList();
                List<Player> listPlayerList = new List<Player>();
                listPlayerInfo.ForEach(player =>
                {
                    Player plyr = new Player() { ID = player.Id, Name = player.Name };
                    listPlayerList.Add(plyr);
                });

                resp = RespSuccessDoc.OK_COMMON;
                resp.Result = listPlayerList.ToArray();
                return JsonSerializer.Serialize(resp);
            } catch(Exception ex) {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(Guid id)
        {
            try
            {
                Playerinfo player = _db.Playerinfos.Find(id);
                PlayerData playerData = new PlayerData();
                if (player == null)
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_NO_DATA);
                }
                else
                {
                    playerData.ID = player.Id;
                    playerData.Name = player.Name;
                    playerData.Photo = player.Photo;
                    playerData.Age = player.Brithday;
                    playerData.Height= player.Height;
                    playerData.Weight= player.Weight;
                    playerData.Position = player.Position;
                    //playerData.Team = player.Team;
                    playerData.Description = player.Description;

                    playerData.OffensiveData = new OffensiveData();
                    playerData.DefensiveData = new DefensiveData();
                    playerData.OffensiveData.ColumnName = _ofsColsDefault;
                    playerData.DefensiveData.ColumnName = _dfsColsDefault;

                    //金工和防守資料暫時先寫死
                    List<OffensiveDataContent> offensiveDataContents = new List<OffensiveDataContent>();
                    List<DefensiveDataContent> defensiveDataContents = new List<DefensiveDataContent>();

                    for (int i = 0; i < 20; i++)
                    {
                        offensiveDataContents.Add(new OffensiveDataContent()
                        {
                            Date = $"時間{i}",
                            ToShoot = $"射門次數{i}",
                            CornerBall = $"角球{i}",
                            GoalKick = $"球門球{i}",
                            Header = $"頭球{i}",
                            PenaltyKick = $"點球{i}",
                            FreeKick = $"自由球{i}"
                        });
                        defensiveDataContents.Add(new DefensiveDataContent()
                        {
                            Date = $"時間{i}",
                            BlockTackle = $"正面搶截{i}",
                            SlideTackle = $"鏟球{i}",
                            ToIntercept = $"截球{i}",
                            BodyCheck = $"身體阻擋{i}",
                            FairCharge = $"合理衝撞{i}"
                        });
                    }

                    playerData.OffensiveData.Data = offensiveDataContents.ToArray();
                    playerData.DefensiveData.Data = defensiveDataContents.ToArray();
                    return JsonSerializer.Serialize(playerData);
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
                RespSuccessDoc.OK_COMMON.Result = uploadOK;
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
                    BlobClient blobClient = containerClient.GetBlobClient($"{subdirectory}/{fileName}.jpg");


                    // 1. 从前端接收 Blob 图像并转换为 JPG 格式
                    using (var inputStream = file.OpenReadStream())
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            using (Image image = Image.Load(inputStream))
                            {
                                image.Mutate(x => x.BackgroundColor(Color.White));
                                image.Save(outputStream, new JpegEncoder());
                            }

                            outputStream.Position = 0;

                            // 2. 上传转换后的 JPG 图像到 Azure Blob Storage
                            //var connectionString = "YOUR_AZURE_BLOB_STORAGE_CONNECTION_STRING";
                            //var containerName = "YOUR_CONTAINER_NAME";
                            //var blobName = "converted-image.jpg"; // 在 Blob Storage 中保存的文件名

                            //BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                            //BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            //BlobClient blobClient = containerClient.GetBlobClient(blobName);

                            await blobClient.UploadAsync(outputStream, true);
                        }
                    }

                    //using (var stream = file.OpenReadStream())
                    //{
                    //    await blobClient.UploadAsync(stream, true);
                    //}
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
