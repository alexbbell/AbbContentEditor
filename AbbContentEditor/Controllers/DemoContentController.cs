using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoContentController : ControllerBase
    {
        private readonly IRepository<WordCollection> _wordColelctionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DemoContentController> _logger;
        private readonly AbbAppContext _context;
        private readonly UserManager<AbbAppUser> _userManager;

        public DemoContentController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<DemoContentController> logger, AbbAppContext context, UserManager<AbbAppUser> usermanager)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _context = context;
            _userManager = usermanager;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {

            bool result = await PopulateWordHistory();
            if (result) return Ok("wordhistory added");
            return BadRequest("No wordhistory added");

        }

        private async Task<bool> PopulateWordHistory()
        {
            //Check this generation later
            string filePath = @"./StaticFiles/demoCollection.json";
            // Read the JSON file
            string jsonContent = System.IO.File.ReadAllText(filePath);
            List<Word> words = JsonSerializer.Deserialize<List<Word>>(jsonContent);

            //var userId = _context.Users.FirstOrDefault(x => x.NormalizedUserName == "ALEXEY@BELIAEFF.RU");
            var userId = await _userManager.FindByEmailAsync("alexey@beliaeff.ru");
            if (userId == null) { return false; }

            WordCollection wc = new WordCollection();
            wc.Author = userId;
            wc.Title = "The first collection";
            wc.PubDate = DateTime.UtcNow;
            wc.UpdDate = DateTime.UtcNow;
            wc.WordsCollection = JsonDocument.Parse(jsonContent);
            try
            {
                await _unitOfWork.wordCollectionRepository.AddAsync(wc);
                await _unitOfWork.Commit();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Words were not added, {ex.Message}");
            }
            return true;
        }
    }
}
