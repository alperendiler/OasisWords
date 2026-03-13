using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OasisWords.Application;
using OasisWords.Core.Application.Pipelines;
using OasisWords.Core.CrossCuttingConcerns.Exceptions;
using OasisWords.Core.Mailing;
using OasisWords.Core.Security.JWT;
using OasisWords.Infrastructure;
using OasisWords.Persistence;
using OasisWords.WebAPI.Jobs;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Options ───────────────────────────────────────────────────────────────────
TokenOptions tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()!;
MailSettings mailSettings = builder.Configuration.GetSection("MailSettings").Get<MailSettings>()!;
CacheSettings cacheSettings = builder.Configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? new CacheSettings();

// ── Layer Service Registrations ───────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Core Singletons ───────────────────────────────────────────────────────────
builder.Services.AddSingleton(tokenOptions);
builder.Services.AddSingleton(mailSettings);
builder.Services.AddSingleton(cacheSettings);
builder.Services.AddScoped<ITokenHelper, JwtHelper>();
builder.Services.AddScoped<IMailService, MailKitMailService>();

// ── Distributed Cache (Redis → in-memory fallback) ────────────────────────────
string? redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
    builder.Services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnection);
else
    builder.Services.AddDistributedMemoryCache();

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Sadece /hangfire yoluna gelen isteklerde query'den token oku
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hangfire"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
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

// ── CORS ──────────────────────────────────────────────────────────────────────
string[] allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global policy — generous limit for general API endpoints
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Strict AI policy — max 2 requests / second per user
    // Protects the Gemini API quota from abuse
    options.AddSlidingWindowLimiter("ai_strict", opt =>
    {
        opt.PermitLimit = 2;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.SegmentsPerWindow = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // No queueing — immediately 429
    });

    // Auth endpoints — prevent credential stuffing / brute force
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(15);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// ── Hangfire ──────────────────────────────────────────────────────────────────
string hangfireConnection = builder.Configuration.GetConnectionString("OasisWordsDB")
    ?? throw new InvalidOperationException("OasisWordsDB connection string is required.");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(hangfireConnection)));

builder.Services.AddHangfireServer(opts =>
{
    opts.WorkerCount = 2; // lightweight — we only have one recurring job
    opts.Queues = new[] { "default", "critical" };
});

// Register job class as scoped so EF DbContext is injected properly
builder.Services.AddScoped<StreakResetJob>();

// ── Controllers & API Explorer ────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger with Bearer support ───────────────────────────────────────────────
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
        Description = "Enter: Bearer {your JWT token}"
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

// ── Exception Handling ────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

// ── Swagger ───────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OasisWords API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ── Hangfire Dashboard ────────────────────────────────────────────────────────
// Development  : dashboard open to all local requests (no token needed in browser)
// Production   : every request must carry a valid JWT with the "Admin" role
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment()
        // Allow all in dev — simplifies local debugging
        ? new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
        // Admin-JWT guard in all other environments
        : new Hangfire.Dashboard.IDashboardAuthorizationFilter[]
          { new OasisWords.WebAPI.Filters.HangfireAuthorizationFilter() },

    // Prevent the dashboard from being indexed by search engines
    AppPath = "/swagger",    // "Back to site" link points to Swagger UI
    DisplayStorageConnectionString = false
});

app.MapControllers();

// ── Schedule Recurring Jobs ───────────────────────────────────────────────────
using (IServiceScope scope = app.Services.CreateScope())
{
    // Nightly at 00:05 UTC — gives midnight DB writes a 5-minute grace period
    RecurringJob.AddOrUpdate<StreakResetJob>(
        recurringJobId: "streak-reset-nightly",
        methodCall: job => job.ExecuteAsync(CancellationToken.None),
        cronExpression: "5 0 * * *",  // 00:05 UTC daily
        timeZone: TimeZoneInfo.Utc);
}

app.Run();
