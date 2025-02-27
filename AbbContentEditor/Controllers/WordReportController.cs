using AbbContentEditor.Data.Repositories;
using AbbContentEditor.Data.UoW;
using AbbContentEditor.Models.Words;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordReportController : ControllerBase
    {
        private readonly IRepository<WordCollection> _wordColelctionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<WordReportController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public WordReportController(IRepository<WordCollection> wordColelctionRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<WordReportController> logger, UserManager<IdentityUser> userManager)
        {
            _wordColelctionRepository = wordColelctionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("wordreport")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null) return BadRequest();

            //var r = _unitOfWork.wordCollectionRepository.Find(x => x.Where(x => x.Author.Id == t));;            
            var r = _unitOfWork.wordHistoryRepository.Find(x=>x.Where(x=>x.IdentityUserId == user.Id));// Find(x => x.Where(x => x.Author.Id == t)); ;
            List<WordHistoryDto> result = _mapper.Map<List<WordHistoryDto>>(r);

            string strResult = JsonConvert.SerializeObject(result,Formatting.Indented  );
            string decodedText = Regex.Unescape(strResult);

            return Ok(decodedText);
        }

    }


}
