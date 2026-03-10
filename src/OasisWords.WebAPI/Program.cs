using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OasisWords.Application;
using OasisWords.Core.Application.Pipelines;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Mailing;
using OasisWords.Core.Security.JWT;
using OasisWords.Infrastructure;
using OasisWords.Persistence;
using Serilog;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Options ──────────────────────────────────────────────────────────────────
TokenOptions tokenOptions = builder.Configuration
    .GetSection("TokenOptions")
    .Get<TokenOptions>()!;

MailSettings mailSettings = builder.Configuration
    .GetSection("MailSettings")
    .Get<MailSettings>()!;

CacheSettings cacheSettings = builder.Configuration
    .GetSection("CacheSettings")
    .Get<CacheSettings>() ?? new CacheSettings();

// ── Layer Service Registrations ───────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Core Services ────────────────────────────────────────────────────────────
builder.Services.AddSingleton(tokenOptions);
builder.Services.AddSingleton(mailSettings);
builder.Services.AddSingleton(cacheSettings);
builder.Services.AddScoped<ITokenHelper, JwtHelper>();
builder.Services.AddScoped<IMailService, MailKitMailService>();

// ── Redis Distributed Cache ──────────────────────────────────────────────────
string? redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnection);
}
else
{
    builder.Services.AddDistributedMemoryCache(); // Fallback for development
}

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OasisWords API",
        Version = "v1",
        Description = "OasisWords — Vocabulary learning platform API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────────────────────
WebApplication app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

// ── Exception Handling Middleware ─────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

// ── Swagger UI ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OasisWords API v1"));
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
