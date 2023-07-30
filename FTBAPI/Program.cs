using FTBAPI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;

//JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
//SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbFootballChciasContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));

builder.Services.AddDataProtection();
builder.Services.AddCors(options =>
{
    //options.AddDefaultPolicy(builder =>
    //{
    //    builder
    //        .AllowAnyOrigin() // 允許任何來源
    //        .AllowAnyMethod()
    //        .AllowAnyHeader();
    //});
    options.AddPolicy("MyGlobalCorsPolicy", builder =>
    {
        builder.WithOrigins(
            "http://localhost",
            "http://localhost:3000",
            "https://localhost",
            "https://localhost:3000",
            "https://ftb-api.azurewebsites.net"
            "https://orange-coast-0b9a4f00f.3.azurestaticapps.net"
        )
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option => {
    option.LoginPath = new PathString("/api/auth/NoLogin");
    //option.AccessDeniedPath = new PathString(); //表示沒有權限的話重新導向到特定網址
    option.ExpireTimeSpan = TimeSpan.FromHours(8);
    option.Cookie.SameSite = SameSiteMode.None;
    option.Cookie.HttpOnly = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// 全域設置Controller都要權限
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

//注入 HttpContextAccessor，這樣可以在其他自訂Service or Class取得User資訊
builder.Services.AddHttpContextAccessor();

// JWT 認證+授權
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
//    {
//        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
//        policy.RequireClaim(ClaimTypes.Name);
//    });
//});
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters =
//            new TokenValidationParameters
//            {
//                ValidateAudience = false,
//                ValidateIssuer = false,
//                ValidateActor = false,
//                ValidateLifetime = true,
//                IssuerSigningKey = SecurityKey
//            };
//    });

//string strJWTIssu = builder.Configuration.GetConnectionString("JWT_ISSU");
//string strJWTAudi = builder.Configuration.GetConnectionString("JWT_AUDI");

//string GenerateJwtToken(string name)
//{
//    if (string.IsNullOrEmpty(name))
//    {
//        throw new InvalidOperationException("Name is not specified.");
//    }
//    var claims = new[] { new Claim(ClaimTypes.Name, name) };
//    var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
//    var token = new JwtSecurityToken(
//        strJWTIssu,
//        strJWTAudi,
//        claims,
//        expires: DateTime.Now.AddSeconds(60),
//        signingCredentials: credentials
//    );
//    return JwtTokenHandler.WriteToken(token);
//}
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// 普通寫法
////設定預設文件，要在 UseStaticFiles 之前
//app.UseDefaultFiles();
////設定使用靜態檔案
//app.UseStaticFiles();

// 食物寫法
//啟用 wwwroot 靜態檔案功能
app.UseFileServer();

app.UseHttpsRedirection();

app.UseCors("MyGlobalCorsPolicy");

//順序要一樣
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.MapGet("/generateJwtToken", context =>
//{
//    return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
//});

app.Run();
