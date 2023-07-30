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
    //        .AllowAnyOrigin() // ���\����ӷ�
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
    //option.AccessDeniedPath = new PathString(); //��ܨS���v�����ܭ��s�ɦV��S�w���}
    option.ExpireTimeSpan = TimeSpan.FromHours(8);
    option.Cookie.SameSite = SameSiteMode.None;
    option.Cookie.HttpOnly = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ����]�mController���n�v��
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

//�`�J HttpContextAccessor�A�o�˥i�H�b��L�ۭqService or Class���oUser��T
builder.Services.AddHttpContextAccessor();

// JWT �{��+���v
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

// ���q�g�k
////�]�w�w�]���A�n�b UseStaticFiles ���e
//app.UseDefaultFiles();
////�]�w�ϥ��R�A�ɮ�
//app.UseStaticFiles();

// �����g�k
//�ҥ� wwwroot �R�A�ɮץ\��
app.UseFileServer();

app.UseHttpsRedirection();

app.UseCors("MyGlobalCorsPolicy");

//���ǭn�@��
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.MapGet("/generateJwtToken", context =>
//{
//    return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
//});

app.Run();
