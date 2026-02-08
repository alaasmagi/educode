using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.RateLimiting;
using App.Application.Services.Attendance;
using App.Application.Services.AttendanceCheck;
using App.Application.Services.AttendanceType;
using App.Application.Services.Course;
using App.Application.Services.School;
using App.Application.Services.User;
using App.Application.Services.UserType;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Enums;
using App.Infrastructure.Argon2;
using App.Infrastructure.EFCore;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using App.Infrastructure.JWT;
using App.Infrastructure.Oracle;
using App.Infrastructure.Redis;
using App.Infrastructure.Sentry;
using App.Web.Clients;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Serilog;
using StackExchange.Redis;
using IPNetwork = System.Net.IPNetwork;

DotNetEnv.Env.Load("../.env");
var builder = WebApplication.CreateBuilder(args);

var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit:14)  
    .CreateLogger();

var envInitializer = new EnvInitializer(loggerFactory.CreateLogger<EnvInitializer>());
envInitializer.InitializeEnv();
builder.Services.AddSingleton(envInitializer);

builder.WebHost.UseSentry(options =>
{
    options.Dsn = envInitializer.SentryDsn;
    options.Environment = builder.Environment.EnvironmentName; 
    options.Release = "1.0.0"; 
    options.TracesSampleRate = 0.1;
});

builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseNpgsql(envInitializer.PgDbConnection, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MinBatchSize(10);
        npgsqlOptions.MaxBatchSize(128);
        npgsqlOptions.EnableRetryOnFailure(3);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, poolSize: 128);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(envInitializer.RedisConnection));

builder.Services.AddScoped<IDatabase>(sp =>
{
    var mux = sp.GetRequiredService<IConnectionMultiplexer>();
    return mux.GetDatabase();
});

builder.Services.AddScoped<SentryService>();
builder.Services.AddScoped<ISentryService>(sp => sp.GetRequiredService<SentryService>());

builder.Services.AddScoped<RedisRepository>(sp =>
{
    var database = sp.GetRequiredService<IDatabase>();
    var logger = sp.GetRequiredService<ILogger<RedisRepository>>();
    var sentry = sp.GetRequiredService<SentryService>();
    return new RedisRepository(database, logger, sentry);
});

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); 
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

// Infrastructure Services
builder.Services.AddScoped<IPasswordService, ArgonService>();
builder.Services.AddScoped<IAccessTokenService, JwtService>();
builder.Services.AddScoped<IPhotoService, OciPhotoService>();
builder.Services.AddScoped<IEmailService, EmailClient>();

// Application Services - Attendance
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAttendanceCheckService, AttendanceCheckService>();
builder.Services.AddScoped<IAttendanceTypeService, AttendanceTypeService>();

// Application Services - User & Auth
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Application Services - Course & School
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISchoolService, SchoolService>();

// Seeding Services
builder.Services.AddScoped<IUserTypeSeedingService, UserTypeSeedingService>();
builder.Services.AddScoped<IUserSeedingService, UserSeedingService>();
builder.Services.AddScoped<IAttendanceTypeSeedingService, AttendanceTypeSeedingService>();
builder.Services.AddScoped<ICourseStatusSeedingService, CourseStatusSeedingService>();

builder.Services.AddSingleton<DbInitializer>();

// Register repositories
builder.Services.AddScoped<IAttendanceCheckRepository, AttendanceCheckRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceTypeRepository, AttendanceTypeRepository>();
builder.Services.AddScoped<IClassroomRepository, ClassroomRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseStatusRepository, CourseStatusRepository>();
builder.Services.AddScoped<ICourseTeacherRepository, CourseTeacherRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTypeRepository, UserTypeRepository>();
builder.Services.AddScoped<IWorkplaceRepository, WorkplaceRepository>();
builder.Services.AddScoped<ICacheRepository, RedisRepository>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    // 10.0.0.0/24 is subnet in which load balancer and VMs are
    options.KnownIPNetworks.Add(new IPNetwork(
        IPAddress.Parse("10.0.0.0"),
        24
    ));

});


builder.Services.AddCors(options =>
{
    var frontendUrls = Helpers.SplitWords(envInitializer.FrontendUrls);
    
    options.AddPolicy("Frontend", policyBuilder =>
    {
        if (frontendUrls.Length > 0)
        {
            policyBuilder
                .WithOrigins(frontendUrls)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });

    options.DefaultPolicyName = "Frontend";
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(envInitializer.JwtKey)),
            ValidateIssuer = true,
            ValidIssuer = envInitializer.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = envInitializer.JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    return Task.CompletedTask;
                }

                context.HandleResponse();
                context.Response.Redirect("/AdminPanel/Index?message=Please+login");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token) &&
                    context.Request.Cookies.TryGetValue("jwt", out var jwtToken))
                {
                    context.Token = jwtToken;
                    Log.Information($"JWT token read from cookie for path: {context.Request.Path}");
                }
                else
                {
                    Log.Warning($"No JWT token found in cookie for path: {context.Request.Path}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context => Task.CompletedTask,
            OnAuthenticationFailed = context => Task.CompletedTask,
            OnForbidden = context => Task.CompletedTask
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromSeconds(1);
        limiterOptions.QueueLimit = 2;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddAuthorization(options =>
{
    foreach (EAccessLevel level in Enum.GetValues(typeof(EAccessLevel)))
    {
        options.AddPolicy(level.ToString(), policy =>
            policy.RequireAssertion(context =>
            {
                var userLevel = Helpers.GetAccessLevelFromClaims(context);
                var requiredLevel = (int)level;
                var result = userLevel >= requiredLevel;
                
                var logger = context.Resource as HttpContext;
                if (logger != null)
                {
                    var loggerFactory = logger.RequestServices.GetService<ILoggerFactory>();
                    var log = loggerFactory?.CreateLogger("Authorization");
                    log?.LogInformation($"Authorization check for policy '{level}': UserLevel={userLevel}, RequiredLevel={requiredLevel}, Result={result}");
                }
                
                return result;
            }));
    }
});

builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EduCodeAPI",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Please enter 'Bearer' followed by your token"
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = envInitializer.RedisConnection;
    options.InstanceName = "EduCode:Session:";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (!builder.Environment.IsDevelopment())
{
    Microsoft.AspNetCore.Hosting.StaticWebAssets.StaticWebAssetsLoader
        .UseStaticWebAssets(builder.Environment, builder.Configuration);
}

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/AdminPanel/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseSession();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Educode API");
});

app.MapStaticAssets();

app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=AdminPanel}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapGet("/", () => Results.Redirect($"/AdminPanel/Index")).RequireRateLimiting("fixed");

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeDb();
}

app.Run();