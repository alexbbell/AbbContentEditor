using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<AbbAppUser> _userManager;

        public WordController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<WordController> logger, UserManager<AbbAppUser> userManager)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("collections")]
        [Produces("application/json")]
        [Authorize]
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.FindByNameAsync(User.GetUsername()!);
            if (user == null) return BadRequest();
            IQueryable<WordCollectionBase> collections = _unitOfWork.wordCollectionRepository
                    .GetAll().Where(x=>x.Author.Id ==  user.Id)
                    .Select(x => x as WordCollectionBase);
            return Ok(collections);
        }

        [HttpGet]
        [Route("collections/{id}")]
        public async Task<IActionResult> GetCollection(int id)
        {
            
            var user = await _userManager.FindByNameAsync(User.GetUsername()!);
            if (user == null) return BadRequest();

            var r = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Id == id && x.Author.Id == user.Id)); 
            return Ok(JsonSerializer.Serialize(r));
        }

        [HttpPost]
        [Route("collections")]
        [Authorize]
        public async Task<IActionResult> CreateCollection([FromBody] CreateWordCollectionDto collection)
        {
            if (collection == null)
            {
                return BadRequest("Collection cannot be null.");
            }

            var user = await _userManager.FindByNameAsync(User.GetUsername()!);
            if (user == null) return BadRequest();
            // Configure JSON serializer to use camelCase
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var wordsList = collection.Words.Select(w => new Word
            {
                Id = w.Id,
                Translate1 = w.Translate1,
                Translate2 = w.Translate2,
                Translate3 = w.Translate3,
            }).ToList();
            // Transform DTO to Domain Entity
            var collectionEntity = new WordCollection
            {
                Author = user,
                Title = collection.Title,
                WordsCollection = JsonDocument.Parse(JsonSerializer.Serialize(wordsList, jsonOptions))
            };


            await _unitOfWork.wordCollectionRepository.AddAsync(collectionEntity);
            await _unitOfWork.Commit();
            return CreatedAtAction(nameof(GetCollection), new { id = collectionEntity.Id }, collectionEntity);
        }


        [HttpPut]
        [Route("collections/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] CreateWordCollectionDto collection)
        {
            if (collection == null)
            {
                return BadRequest("Collection cannot be null.");
            }
            var user = await _userManager.FindByNameAsync(User.GetUsername()!);
            if (user == null) return BadRequest();
            var existingCollection = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Id == id && x.Author.Id == user.Id)).FirstOrDefault();
            if (existingCollection == null) return NotFound();
            // Configure JSON serializer to use camelCase
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var wordsList = collection.Words.Select(w => new Word
            {
                Id = w.Id,
                Translate1 = w.Translate1,
                Translate2 = w.Translate2,
                Translate3 = w.Translate3,
            }).ToList();
            existingCollection.Title = collection.Title;
            existingCollection.WordsCollection = JsonDocument.Parse(JsonSerializer.Serialize(wordsList, jsonOptions));
            await _unitOfWork.wordCollectionRepository.UpdateAsync(existingCollection);
            await _unitOfWork.Commit();
            return NoContent();
        }

        [HttpDelete]
        [Route("collections/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var user = await _userManager.FindByNameAsync(User.GetUsername()!);
            if (user == null) return BadRequest();

            var collection = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Id == id && x.Author.Id == user.Id)).FirstOrDefault();
            if (collection == null) return NotFound();

            await _unitOfWork.wordCollectionRepository.DeleteAsync(collection);
            await _unitOfWork.Commit();
            return NoContent();
        }
    }
}
