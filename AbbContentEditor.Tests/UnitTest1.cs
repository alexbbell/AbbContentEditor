using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace AbbContentEditor.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_GetBlog_AddsBlogToContext()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("test_settings.json", optional: true, reloadOnChange: true)
                .Build();

            DbContextOptions<AbbAppContext> options = new DbContextOptionsBuilder<AbbAppContext>()
            // .UseInMemoryDatabase(databaseName: "Test_AddBlog_AddsBlogToContext")
                .UseNpgsql(config.GetConnectionString("PGSQLConnectionString"))
           .Options;


            using var context = new AbbAppContext(options);

            var mockBlogRepository = new Repository<Blog>(context);
            var mockCategoryRepository = new Repository<Category>(context);
            var blogService = new BlogService(context, mockBlogRepository, mockCategoryRepository);

            var blog = new Blog
            {
                Id = 0,
                CategoryId = 1,
                IsDeleted = false,
                ImageUrl = "imageurl",
                Title = "test title",
                TheText = "the full text here",
                Preview = "preview here"
            };

            // Assert - Check if the blog was added to the context
            var getBlog = blogService.GetBlogById(2);
            Assert.NotNull(getBlog);
            Assert.AreEqual(4, getBlog.CategoryId);
        }


        //[Test]
        //public void Comapare2Files()
        //{
        //    string editedContent = File.ReadAllText(@"D:\Projects\alekseibeliaev3\client\public\locales\en\en-translation.json");
        //    string defaultContent = File.ReadAllText(@"D:\Projects\alekseibeliaev3\client\public\locales.bak\en\en-translation.json");

        //    var scEdited = JsonConvert.DeserializeObject<SiteContent>(editedContent);
        //    var scDefault = JsonConvert.DeserializeObject<SiteContent>(defaultContent);

        //    Assert.AreEqual(scEdited.main.titleAbout, scDefault.main.titleAbout);
        //    Assert.AreEqual(scEdited.main.email, scDefault.main.email);
        //    Assert.AreEqual(scEdited.main.description, scDefault.main.description);

        //    Assert.Pass();
        //}
    }
}
