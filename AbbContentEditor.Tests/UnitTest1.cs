using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AbbContentEditor.Tests
{
    public class Tests
    {

        IConfiguration _config;
        DbContextOptions<AbbAppContext> _options;
        private DbContext _dbContext;
        private AbbAppContext _mockContext;
        private IUnitOfWork _unitOfWork;
        private Repository<Category> _mockCatRepository;
        private Mock<BlogRepository> _blogRepository;

        public Tests()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("test_settings.json", optional: true, reloadOnChange: true)
                .Build();
            
        }
        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<AbbAppContext>()
               //.UseInMemoryDatabase(databaseName: "Test_AddBlog_AddsBlogToContext")
               .UseNpgsql(_config.GetConnectionString("PGSQLConnectionString"))
                .Options;
            
            _mockContext = new AbbAppContext(_options);
            _mockCatRepository = new Repository<Category>(_mockContext);
            _blogRepository = new Mock<BlogRepository>(_mockContext);

            // Set up the UnitOfWork with the mocked context and repository
            //_unitOfWork = new UnitOfWork(_mockContext.Object);
            _unitOfWork = new UnitOfWork(_mockContext);


        }

        [Test]
        public async Task Test_GetBlog_MoqAddsBlogToContext()
        {
            var result =  _unitOfWork.blogRepository.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.Count(), 5); // Assuming two items are in the mocked list

        }


        [Test]
        public async Task Test_GetCategory()
        {
            var category = new Category { Id = 2, Name = "Sport" };
            var result = _mockCatRepository.GetByIdAsync(2).Result;
            Console.WriteLine(result);
            Assert.IsNotNull(result);
            Assert.AreEqual("Sport", result.Name);
        }

        [Test]
        public async Task Test_AddBankPay()
        {
            BankOperation bo = new BankOperation()
            {
                Id = 1,
                Name = "test 2",
                IsPayable = true,
                TheSumm = 200
            };

            var result = _unitOfWork.bankOperationRepository.AddAsync(bo);
            _unitOfWork.Commit();
            Assert.IsNotNull(result);
            
        }



    }
}
