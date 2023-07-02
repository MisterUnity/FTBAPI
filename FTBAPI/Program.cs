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
            .AllowAnyOrigin() // ���\����ӷ�
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    //options.AddDefaultPolicy(builder =>
    //{
    //    builder
    //        .WithOrigins("http://localhost:5173", "https://localhost:44300") // �u���\�o��Өӷ�
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

// ���q�g�k
////�]�w�w�]���A�n�b UseStaticFiles ���e
//app.UseDefaultFiles();
////�]�w�ϥ��R�A�ɮ�
//app.UseStaticFiles();

// �����g�k
//�ҥ� wwwroot �R�A�ɮץ\��
app.UseFileServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

app.Run();
