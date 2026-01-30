using System.Net;
using App.DAL.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.RateLimiting;
using App.BLL;
using App.BLL.Contracts;
using App.Common;
using App.Domain;
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
    options.TracesSampleRate = 1.0;
});

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(envInitializer.PgDbConnection, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60);
        npgsqlOptions.EnableRetryOnFailure(3);
    }), poolSize: 500);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(envInitializer.RedisConnection));

builder.Services.AddSingleton<RedisRepository>(sp =>
{
    var mux = sp.GetRequiredService<IConnectionMultiplexer>();
    var logger = sp.GetRequiredService<ILogger<RedisRepository>>();
    return new RedisRepository(mux, logger);
});

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(); 
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

builder.Services.AddScoped<IPhotoService, OciPhotoService>();
builder.Services.AddScoped<IAttendanceManagementService, AttendanceManagementService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseManagementService, CourseManagementService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddSingleton<DbInitializer>();

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
                }
                return Task.CompletedTask;
            }
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
                return userLevel >= (int)level;
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



builder.Services.AddDistributedMemoryCache();

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

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=AdminPanel}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect($"/AdminPanel/Index")).RequireRateLimiting("fixed");

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.InitializeDb();
}

app.Run();