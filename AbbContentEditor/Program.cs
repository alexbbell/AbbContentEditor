using AbbContentEditor;
using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Helpers;
using AbbContentEditor.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");



try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    
    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    var connStr = builder.Configuration.GetConnectionString("PGSQLConnectionString");

    builder.Services.AddDbContext<AbbAppContext>(options =>
    {
        options.UseNpgsql(connStr);
    });
    // var connStr = builder.Configuration.GetConnectionString("PGSQLConnectionString");

    
    // builder.Services.AddScoped<AbbAppContext>();
    builder.Services.AddScoped<AbbFileRepository>();

    IHostEnvironment env = builder.Environment;
    var conf = builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    // string allowedHosts = builder.Configuration.GetSection("AllowedHosts").Value.ToString();
    //string[] allowedHosts = builder.Configuration.GetSection("AllowedHosts").Value.ToString().Split(";");

    Console.WriteLine(env.IsDevelopment());
    //
    string allowedHosts = (env.IsDevelopment()) ? "http://localhost:3000, http://localhost:3001" : "https://alexey.beliaeff.ru";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
            policy =>
            {
                policy
                .WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5000", "https://localhost:5001")

                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
    });

    Console.WriteLine(MyAllowSpecificOrigins.ToString());
    builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
    var secretKey = builder.Configuration.GetSection("JWTSettings:SecretKey").Value;
    var issuer = builder.Configuration.GetSection("JWTSettings:Issuer").Value;
    var audience = builder.Configuration.GetSection("JWTSettings:Audience").Value;
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

    builder.Services.Configure<IdentityOptions >(options =>
    {
        options.Password.RequireDigit = true;
        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings.
        options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = false;

    });

    //builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddAutoMapper(typeof(Program).Assembly);
    builder.Services.AddTransient<IConfiguration>( sp =>
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        // configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
        return configurationBuilder.Build();
    });

    Console.WriteLine(DateTime.Now);
    builder.Services.AddTransient<AbbAppContext>().
            AddTransient<ITokenManager, TokenManager>();

    // builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false; // for test only!
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        
        
    })
    .AddEntityFrameworkStores<AbbAppContext>()
    .AddDefaultTokenProviders();


    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = signingKey,
            ValidateIssuerSigningKey = true
        };
    });

    

    var app = builder.Build();
    app.UseCors(MyAllowSpecificOrigins);
    app.UseRouting();

    Console.WriteLine("Dev or not");
    Console.WriteLine(app.Environment.IsDevelopment());
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // var context = app.Services.GetService<AbbAppContext>();
    // CreateDefaultData createDefaultData = new CreateDefaultData(context);


    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{

    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
