using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbbContentEditor.Data
{
    //public class AbbAppContext : DbContext
    public class AbbAppContext : IdentityDbContext<AbbAppUser, AbbAppUserRole, string>
    {
        //public DbSet<CustomUser> CustomUsers { get; set; }

        ILogger<AbbAppContext> _logger;
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Blog> Blogs { get; set; }
        public virtual DbSet<BankOperation> BankOperations { get; set; }

        public virtual DbSet<Countdown> Countdowns { get; set; }
        public virtual DbSet<WordCollection> WordCollections { get; set; }
        public virtual DbSet<WordHistory> WordHistories { get; set; }

        //public AbbAppContext(IConfiguration configuration) : base()
        public AbbAppContext(DbContextOptions<AbbAppContext> options) : base(options)    {  }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            if (Database.IsSqlite())
            {
                // SQLite does not support JsonDocument, so use a string column
                modelBuilder.Entity<WordCollection>()
                    .Property<string>("WordsCollectionString");


                modelBuilder.Entity<WordCollection>()
                    .Ignore(e => e.WordsCollection);                
                
            }
            else
            {
                // PostgreSQL supports JsonDocument natively
                modelBuilder.Entity<WordCollection>()
                    .Property(e => e.WordsCollection)
                    .HasColumnType("jsonb");
            }
            // Override default AspNet Identity table names
            string prefix = "Abb_";
            modelBuilder.Entity<AbbAppUser>(entity => { entity.ToTable(name: $"{prefix}Users"); });
            //modelBuilder.Entity<IdentityUser>(entity => { entity.ToTable(name: $"{prefix}AppUsers"); });
            modelBuilder.Entity<AbbAppUserRole>(entity => { entity.ToTable(name: $"{prefix}Roles"); });
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
