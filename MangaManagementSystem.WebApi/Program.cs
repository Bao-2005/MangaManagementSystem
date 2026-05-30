using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.API.Extensions;
using MangaManagementSystem.Business.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}
builder.Services.AddDbContext<MangaDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.Register();
builder.Services.AddEndpointsApiExplorer();

// Swagger: thêm ô nhập X-User-Id header để test API (tạm thay JWT)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MangaManagementSystem API",
        Version = "v1",
        Description = "API cho hệ thống quản lý manga. Dùng nút 'Authorize' để nhập X-User-Id."
    });

    // Định nghĩa security scheme cho X-User-Id header
    c.AddSecurityDefinition("X-User-Id", new OpenApiSecurityScheme
    {
        Name = "X-User-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Nhập UserId (GUID) của user đang đăng nhập. Ví dụ: A1000000-0000-0000-0000-000000000001"
    });

    // Áp dụng cho tất cả endpoint
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-User-Id"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
