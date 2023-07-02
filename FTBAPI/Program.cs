using FTBAPI.Models;
using Microsoft.EntityFrameworkCore;

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
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin() // 允許任何來源
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    //options.AddDefaultPolicy(builder =>
    //{
    //    builder
    //        .WithOrigins("http://localhost:5173", "https://localhost:44300") // 只允許這兩個來源
    //        .AllowAnyMethod()
    //        .AllowAnyHeader();
    //});
});

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

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();
