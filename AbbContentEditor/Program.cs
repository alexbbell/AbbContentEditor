using AbbContentEditor;
using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Helpers;
using AbbContentEditor.Middleware;
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

    //string  connStr = builder.Configuration.GetConnectionString("PGSQLConnectionString");
    string connStr = builder.Configuration.GetConnectionString("SQLiteConnectionString"); 


        //// options.UseSqlite(connStr);

    builder.Services.AddDbContext<AbbAppContext>(options =>
    {
        options.UseSqlite(connStr);
         //options.UseNpgsql(connStr);
        //options.UseNpgsql(connStr, npgsqlOptions =>
        //{
        //    npgsqlOptions.EnableRetryOnFailure(
        //        maxRetryCount: 5,
        //        maxRetryDelay: TimeSpan.FromSeconds(10),
        //        errorCodesToAdd: null
        //        );
        //});
    });
    
    builder.Services.AddScoped<AbbFileRepository>();

    IHostEnvironment env = builder.Environment;
    var conf = builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);


    var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    Console.WriteLine(env.IsDevelopment());
    string allowedHosts = (env.IsDevelopment()) ? "http://localhost:3000, http://localhost:3001" : "https://alexey.beliaeff.ru";
    

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
            policy =>
            {
                policy
                .WithOrigins("http://localhost:3000", 
                        "http://localhost:3001",
                        "http://localhost:5173",
                        "http://localhost:5000", 
                        "https://localhost:5001", 
                        "https://dev.beliaeff.ru",
                        "https://api.beliaeff.ru")
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
        options.Tokens.ProviderMap.Add("CustomEmailConfirmation",
        new TokenProviderDescriptor(
            typeof(CustomEmailConfirmationTokenProvider<IdentityUser>)));
        options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

    });
    builder.Services.AddTransient<CustomEmailConfirmationTokenProvider<IdentityUser>>();

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
    builder.Services.AddScoped<AbbAppContext>().
            AddTransient<ITokenManager, TokenManager>();

    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<ImageUtilities>();

    //.AddHttpContextAccessor()
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


    builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false; // for test only!
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AbbAppContext>()
    .AddDefaultTokenProviders();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminsOnly", policy => policy.RequireRole("Admins"));
    });

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminsOnly", policy => policy.RequireRole("Admins"));
        options.AddPolicy("Guest", policy => policy.RequireRole("Guest"));
    });

    var app = builder.Build();
    //app.UseMiddleware<TokenExpirationMiddleware>();
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


    //var context = app.Services.GetService<AbbAppContext>();
    //CreateDefaultData createDefaultData = new CreateDefaultData(context);
    using (var scope = app.Services.CreateScope())
    {
        var scopedProvider = scope.ServiceProvider;
        var context = scopedProvider.GetRequiredService<AbbAppContext>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
        CreateDefaultData createDefaultData = new CreateDefaultData(context, roleManager );        
    }

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
