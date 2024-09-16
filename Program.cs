using System.Text;
using DotnetAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DevCors",
        (corsBuilder) =>
        {
            corsBuilder
                .WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:3000",
                    "http://localhost:8000"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
    options.AddPolicy(
        "ProdCors",
        (corsBuilder) =>
        {
            corsBuilder
                .WithOrigins("https://myProductionSite.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

string? tokenKeyString = builder.Configuration.GetSection("Appsettings:TokenKey").Value;

SymmetricSecurityKey tokenKey =
    new(Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));

TokenValidationParameters tokenValidationParameters =
    new()
    {
        IssuerSigningKey = tokenKey,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = false,
        ValidateAudience = false,
    };

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
