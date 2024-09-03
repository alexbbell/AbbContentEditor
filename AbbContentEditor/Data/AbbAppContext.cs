using AbbContentEditor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data
{
    //public class AbbAppContext : DbContext
    public class AbbAppContext : IdentityDbContext<IdentityUser>
    {
        //public DbSet<CustomUser> CustomUsers { get; set; }
        private string _dbPath;
        private string _mysqlConn { get; set; }
        private string _pgConn { get; set; }
        IConfiguration _configuration;
        ILogger<AbbAppContext> _logger;
        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> Blogs { get; set; }

        public AbbAppContext(IConfiguration configuration, ILogger<AbbAppContext> logger) : base()
        {
            _configuration = configuration;
            _logger = logger;
            _dbPath = _configuration.GetConnectionString("SQLiteConnectionString");
            _mysqlConn = _configuration.GetConnectionString("SQLConnectionString");
            _pgConn = _configuration.GetConnectionString("PGSQLConnectionString");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                //optionsBuilder.UseSqlite(_dbPath).LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
                // optionsBuilder.UseMySql(_mysqlConn);
                optionsBuilder.UseNpgsql(_pgConn);
                _logger.LogInformation($"Connection to DB successfull");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database not available, {ex.Message}");
                throw new Exception($"Could not connect to database.  + { ex.Message }");

            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Override default AspNet Identity table names
            string prefix = "Abb_";
            // modelBuilder.Entity<CustomUser>(entity => { entity.ToTable(name: $"{prefix}Users"); });
            modelBuilder.Entity<IdentityUser>(entity => { entity.ToTable(name: $"{prefix}Users"); });
            modelBuilder.Entity<IdentityRole>(entity => { entity.ToTable(name: $"{prefix}Roles"); });
            modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable($"{prefix}UserRoles"); });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable($"{prefix}UserClaims"); });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable($"{prefix}UserLogins"); });
            modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable($"{prefix}UserTokens"); });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable($"{prefix}RoleClaims"); });


            modelBuilder.Entity<Category>().HasData(new Category[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Lifestyle"
                },
                new Category
                {
                       Id = 2, Name = "Sport"
                },
                new Category
                {
                       Id = 3, Name = "Software Development"
                } 
            }
            );
            modelBuilder.Entity<Blog>().HasData(new Blog[]
            {
                new Blog
                {
                    Id = 1, ImageUrl = "imageUrl", IsDeleted = false, Title = "My first blog post from dbcontext migration",

                    CategoryId = 1,
                    // Category = new Category { Id = 1, Name = "Lifestyle"}, 
                    Preview = "Vitafit Digital Personal Scales for People, Weighing Professional since 2001, Body Scales with Clear LED Display and Step-On, 180 kg, Batteries Included, Silver Black…",
                    TheText = "HIGH PRECISION GUARANTEE With more than 20 years experience in the scale industry, we have developed the scale with the best technology and expertise, guaranteeing high accuracy of 0.1lb/0.05kg throughout the life of the scale.\r\nEasy to use: the scale people uses up-to-date digital technology, along with many friendly functions, including: auto calibration, auto step up, auto power off, convenient large platform in 280 x 280 mm, 3 x AAA batteries included, 3 unit switch: lb/kg/st, and high precision in full weighing range.",

                }
            });


        }


    }
}
