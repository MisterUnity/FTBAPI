using Azure.Identity;
using Azure.Storage.Blobs;
using FTBAPI.HTTPResp;
using FTBAPI.HTTPResp.Models;
using FTBAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Data;
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
        private static readonly ColumnName[] _ofsColsDefault = new ColumnName[]
        {
            //new ColumnName() { Field = "Date", Header = "時間" },
            new ColumnName() { Field = "ToShoot", Header = "射門" },
            new ColumnName() { Field = "CornerBall", Header = "角球" },
            //new ColumnName() { Field = "GoalKick", Header = "球門球" },
            //new ColumnName() { Field = "Header", Header = "頭球" },
            new ColumnName() { Field = "PenaltyKick", Header = "12碼罰球" },
            new ColumnName() { Field = "FreeKick", Header = "自由球" },
            new ColumnName() { Field = "Goal", Header = "得分" }
        };
        //防守資料欄位定義
        private static readonly ColumnName[] _dfsColsDefault = new ColumnName[]
        {
            //new ColumnName() { Field = "Date", Header = "時間" },
            //new ColumnName() { Field = "BlockTackle", Header = "正面搶截" },
            //new ColumnName() { Field = "SlideTackle", Header = "鏟球" },
            //new ColumnName() { Field = "ToIntercept", Header = "截球" },
            //new ColumnName() { Field = "BodyCheck", Header = "身體阻擋" },
            //new ColumnName() { Field = "FairCharge", Header = "合理衝撞" }
            new ColumnName() { Field = "HandBall", Header = "手球" },
            new ColumnName() { Field = "Offside", Header = "越線" },
            new ColumnName() { Field = "TechnicalFoul", Header = "技術性犯規" },
            new ColumnName() { Field = "YellowCard", Header = "黃牌" },
            new ColumnName() { Field = "RedCard", Header = "紅牌" }
        };

        private class Player {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Team { get; set; }
        }
        private class ColumnName
        {
            public string Field { get; set; }
            public string Header { get; set; }
        }
        private class OffensiveData
        {
            public string ToShoot { get; set; }
            public string CornerBall { get; set; }
            public string PenaltyKick { get; set; }
            public string FreeKick { get; set; }
            public string Goal { get; set; }
        }
        private class DefensiveData
        {
            public string HandBall { get; set; }
            public string Offside { get; set; }
            public string TechnicalFoul { get; set; }
            public string YellowCard { get; set; }
            public string RedCard { get; set; }
        }
        private class GameData
        {
            public ColumnName[] OffColumns { get; set; }
            public ColumnName[] DefColumns { get; set; }
            public GameHistory[] Data { get; set; }
        }
        private class GameHistory
        {
            public string Date { get; set; }
            public string Team { get; set; } // 所屬隊伍
            public string Opponent { get; set; } // 對手
            public string Place { get; set; } // 所屬隊伍
            public int IsHome { get; set; } // 所屬隊伍
            public DefensiveData DefensiveData { get; set; }
            public OffensiveData OffensiveData { get; set; }
        }
        private class PlayerData
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Photo { get; set; }
            public string Age { get; set; }
            public string Gender { get; set; }
            public string Height { get; set; }
            public string Weight { get; set; }
            public string Position { get; set; }
            public string Team { get; set; }
            public string Description { get; set; }
            public GameData GameHistory { get; set; }
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
            try
            {
                // _httpContextAccessor.HttpContext.User.Claims => 可取得包含登入者相關資料的網路綜合資料
                //Console.WriteLine(_httpContextAccessor.HttpContext.User);
                CommonRespBody resp;
                List<Playerinfo> listPlayerInfo = _db.Playerinfos.ToList();

                if (listPlayerInfo.Count > 0)
                {
                    List<Player> listPlayerList = new List<Player>();
                    listPlayerInfo.ForEach(player =>
                    {
                        Player plyr = new Player() { ID = player.Id, Name = player.Name, Team = player.Team };
                        listPlayerList.Add(plyr);
                    });

                    resp = RespSuccessDoc.OK_COMMON;
                    resp.Result = listPlayerList.ToArray();
                    return JsonSerializer.Serialize(resp);
                }
                else
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_NO_DATA);
                }
            } catch(Exception ex) {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
        
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
                    playerData.Age = player.Age;
                    playerData.Height= player.Height;
                    playerData.Weight= player.Weight;
                    playerData.Position = player.Position;
                    playerData.Team = player.Team;
                    playerData.Description = player.Description;
                    playerData.Gender = player.Gender == "M" ? "男": "女";

                    var playerGameInfo = from rows in _db.Playergamesinfos
                                     where rows.Id == id.ToString()
                                     select rows;

                    if (playerGameInfo != null)
                    {
                        List<Playergamesinfo> playergamesinfos = playerGameInfo.ToList();
                        playerData.GameHistory = new GameData();
                        playerData.GameHistory.DefColumns = _dfsColsDefault;
                        playerData.GameHistory.OffColumns = _ofsColsDefault;
                        playerData.GameHistory.Data = new GameHistory[playergamesinfos.Count];

                        for (int i = 0; i < playergamesinfos.Count; i++)
                        {
                            playerData.GameHistory.Data[i] = new GameHistory();
                            playerData.GameHistory.Data[i].Date = playergamesinfos[i].Date.ToString("yyyy-MM-dd");
                            playerData.GameHistory.Data[i].Team = playergamesinfos[i].Team;
                            playerData.GameHistory.Data[i].Opponent = playergamesinfos[i].Opponent;
                            playerData.GameHistory.Data[i].IsHome = playergamesinfos[i].IsHome;
                            playerData.GameHistory.Data[i].Place = playergamesinfos[i].Place;
                            // 進攻數據讀取
                            playerData.GameHistory.Data[i].OffensiveData = new OffensiveData();
                            playerData.GameHistory.Data[i].OffensiveData.Goal = playergamesinfos[i].Goal.ToString();
                            playerData.GameHistory.Data[i].OffensiveData.ToShoot = playergamesinfos[i].Goal.ToString();
                            playerData.GameHistory.Data[i].OffensiveData.PenaltyKick = playergamesinfos[i].PenaltyKick.ToString();
                            playerData.GameHistory.Data[i].OffensiveData.FreeKick = playergamesinfos[i].FreeKick.ToString();
                            playerData.GameHistory.Data[i].OffensiveData.CornerBall = playergamesinfos[i].CornerBall.ToString();

                            // 防守數據讀取
                            playerData.GameHistory.Data[i].DefensiveData = new DefensiveData();
                            playerData.GameHistory.Data[i].DefensiveData.TechnicalFoul = playergamesinfos[i].TechnicalFoul.ToString();
                            playerData.GameHistory.Data[i].DefensiveData.Offside = playergamesinfos[i].Offside.ToString();
                            playerData.GameHistory.Data[i].DefensiveData.HandBall = playergamesinfos[i].HandBall.ToString();
                            playerData.GameHistory.Data[i].DefensiveData.YellowCard = playergamesinfos[i].YellowCard.ToString();
                            playerData.GameHistory.Data[i].DefensiveData.RedCard = playergamesinfos[i].RedCard.ToString();
                        }
                    }

                    CommonRespBody resp = RespSuccessDoc.OK_COMMON;
                    resp.Result = playerData;
                    return JsonSerializer.Serialize(resp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        [HttpPut("{id}")]
        public async Task<string> Put(Guid id, [FromForm] IFormCollection form)
        {
            try
            {
                
                if (id.ToString() != form["id"].ToString())
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_PARM_ERR);
                }

                string gender = form["gender"].ToString();
                string[] genderOK = new string[] { "M", "F" };
                string name = form["name"].ToString(), age = form["age"].ToString();
                if (!genderOK.Contains(gender))
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_PARM_ERR);
                }

                Playerinfo player = _db.Playerinfos.SingleOrDefault(player => player.Name == name && player.Age == age);

                if (player == null)
                {
                    return JsonSerializer.Serialize(RespErrDoc.ERR_NO_DATA);
                }

                var file = form.Files["photo"];
                string url = await UploadFromBinaryDataAsync(file);
                player.Photo = url;
                player.Id = id;
                player.Age = age;
                player.Name = name;
                player.Position = form["position"].ToString();
                player.Weight = form["weight"].ToString();
                player.Height = form["height"].ToString();
                player.Description = form["description"].ToString();
                player.Team = form["team"].ToString();
                player.Gender = form["gender"].ToString();
                _db.Entry(player).State = EntityState.Modified;
                _db.SaveChanges();
                return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
            } catch(Exception ex)
            {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public string Delete(Guid id)
        {
            try
            {
                var info = _db.Playerinfos.Single(player => player.Id == id);
                _db.Playerinfos.Remove(info);
                _db.SaveChanges();
                return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
            }
            catch
            {
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }

        [HttpPost("AddPlayer")]
        public async Task<string> Post([FromForm] IFormCollection form)
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
                oPlayerInfo.Age = form["age"].ToString();
                oPlayerInfo.Weight = form["weight"].ToString();
                oPlayerInfo.Height = form["height"].ToString();
                oPlayerInfo.Team = form["team"].ToString();
                oPlayerInfo.Position = form["position"].ToString();
                oPlayerInfo.Description = "";

                Playerinfo player = _db.Playerinfos.SingleOrDefault(player => player.Name == oPlayerInfo.Name && player.Age == oPlayerInfo.Age);
                
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
        [HttpPost("AddGameRecord")]
        public string AddGameRecord([FromBody] Playergamesinfo[] playergamesinfo)
        {
            try {

                string connectionString = _configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
                //SqlConnection connection = new SqlConnection(connectionString);
                string query = "SELECT * FROM PLAYERINFO WHERE NAME = @Value1 AND ID = @Value2";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    for (int i = 0; i < playergamesinfo.Length; i++)
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Value1", playergamesinfo[i].Name); // 替换为实际的值
                        command.Parameters.AddWithValue("@Value2", playergamesinfo[i].Id); // 替换为实际的值

                        //SqlDataAdapter adapter = new SqlDataAdapter(command);
                        //DataTable dataTable = new DataTable();
                        //adapter.Fill(dataTable);

                        SqlDataReader reader = command.ExecuteReader();
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        if (dataTable.Rows.Count == 0)
                        {
                            CommonRespBody resp = RespErrDoc.ERR_PARM_ERR;
                            resp.OtherMessage = $"傳入的 ID ({playergamesinfo[i].Id}) 與 姓名({playergamesinfo[i].Name})無法匹配，請確認參數!!";
                            return JsonSerializer.Serialize(resp);
                        }
                        else
                        {
                            string insertQuery =    "INSERT INTO PLAYERGAMESINFO" +
                                                    "(ID, Name, IsHome, Place, Date, Team, Opponent, Goal, ToShoot, PenaltyKick, FreeKick, CornerBall, HandBall, Offside, TechnicalFoul, YellowCard, RedCard)" +
                                                    "VALUES" +
                                                    "(@Value1, @Value2, @Value3, @Value4, @Value5, @Value6, @Value7, @Value8, @Value9, @Value10, @Value11, @Value12, @Value13, @Value14, @Value15, @Value16, @Value17)";
                            SqlCommand commandIns = new SqlCommand(insertQuery, connection);
                            commandIns.Parameters.AddWithValue("@Value1", playergamesinfo[i].Id);
                            commandIns.Parameters.AddWithValue("@Value2", playergamesinfo[i].Name);
                            commandIns.Parameters.AddWithValue("@Value3", playergamesinfo[i].IsHome);
                            commandIns.Parameters.AddWithValue("@Value4", playergamesinfo[i].Place);
                            commandIns.Parameters.AddWithValue("@Value5", playergamesinfo[i].Date);
                            commandIns.Parameters.AddWithValue("@Value6", playergamesinfo[i].Team);
                            commandIns.Parameters.AddWithValue("@Value7", playergamesinfo[i].Opponent);
                            commandIns.Parameters.AddWithValue("@Value8", playergamesinfo[i].Goal);
                            commandIns.Parameters.AddWithValue("@Value9", playergamesinfo[i].ToShoot);
                            commandIns.Parameters.AddWithValue("@Value10", playergamesinfo[i].PenaltyKick);
                            commandIns.Parameters.AddWithValue("@Value11", playergamesinfo[i].FreeKick);
                            commandIns.Parameters.AddWithValue("@Value12", playergamesinfo[i].CornerBall);
                            commandIns.Parameters.AddWithValue("@Value13", playergamesinfo[i].HandBall);
                            commandIns.Parameters.AddWithValue("@Value14", playergamesinfo[i].Offside);
                            commandIns.Parameters.AddWithValue("@Value15", playergamesinfo[i].TechnicalFoul);
                            commandIns.Parameters.AddWithValue("@Value16", playergamesinfo[i].YellowCard);
                            commandIns.Parameters.AddWithValue("@Value17", playergamesinfo[i].RedCard);
                            int rowsAffected = commandIns.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // 插入成功
                                Console.WriteLine("Data inserted successfully.");
                            }
                            else
                            {
                                // 插入失败 
                                Console.WriteLine("Data insertion failed.");
                            }
                        }
                    }
                    connection.Close();
                }
                
                return JsonSerializer.Serialize(RespSuccessDoc.OK_COMMON);
            }
            catch (Exception ex)
            {
                RespErrDoc.ERR_SERVER.ErrorMessage = ex.ToString();
                return JsonSerializer.Serialize(RespErrDoc.ERR_SERVER);
            }
        }
    }
}
