using AbbContentEditor.Data;
using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordController : ControllerBase
    {
        private readonly IRepository<WordCollection> _wordColelctionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WordController> _logger;
        private readonly AbbAppContext _context;

        public WordController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<WordController> logger, AbbAppContext context)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("wordcollections")]
        public IActionResult Index()
        {
            string t = "b50bb495-4928-483a-b3b4-11afbe5e7b34";
            //var r = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Author.Id == t));;            
            var r = _unitOfWork.wordCollectionRepository.GetAll().FirstOrDefault();// Find(x => x.Where(x => x.Author.Id == t)); ;
            string strResult = JsonSerializer.Serialize(r);
            string decodedText = Regex.Unescape(strResult);

            return Ok(decodedText);
        }

        [HttpGet]
        [Route("collections/{id}")]
        public IActionResult GetCollection(int id)
        {
            string t = "b50bb495-4928-483a-b3b4-11afbe5e7b34";
            var r = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Id == id)); ;
            return Ok(JsonSerializer.Serialize(r));
        }
    }
}
