using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
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


        public DemoContentController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<DemoContentController> logger, AbbAppContext context)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _context = context;
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
            string filePath = @"D:\Projects\AbbContentEditor\AbbContentEditor\StaticFiles\demoCollection.json";
            // Read the JSON file
            string jsonContent = System.IO.File.ReadAllText(filePath);
            List<Word> words = JsonSerializer.Deserialize<List<Word>>(jsonContent);

            var userId = _context.Users.FirstOrDefault(x => x.NormalizedUserName == "ALEXEY@BELIAEFF.RU");
            if (userId == null) { return false; }

            WordCollection wc = new WordCollection();
            wc.Author = userId;
            wc.Title = "The first collection";
            wc.PubDate = DateTime.UtcNow;
            wc.UpdDate = DateTime.UtcNow;
            wc.WordsCollection = JsonDocument.Parse(jsonContent);

            await _unitOfWork.wordCollectionRepository.AddAsync(wc);
            await _unitOfWork.Commit();
            return true;
        }
    }
}
