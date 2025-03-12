﻿using AbbContentEditor.Data.Repositories;
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
            var r = _unitOfWork.wordHistoryRepository.Find(x=>x.Where(x=>x.IdentityUserId == user.Id), 0, int.MaxValue);// Find(x => x.Where(x => x.Author.Id == t)); ;
            List<WordHistoryDto> results = _mapper.Map<List<WordHistoryDto>>(r);

            int correntResults = results.Count(x => x.Correct);
            int total = results.Count();
            DateTime minTime = results.Min(x => x.AnswerTime);
            DateTime maxTime = results.Max(x => x.AnswerTime);
            var dateDiff = (maxTime - minTime).TotalHours;

            WordsReport wordsReport = new WordsReport()
            {
                Attempts = total,
                CorrectAnswers = correntResults,
                TheTime = Math.Round(dateDiff, 2),
                User = user.UserName
            };
            string strResult = JsonConvert.SerializeObject(results,Formatting.Indented  );
            string decodedText = Regex.Unescape(strResult);

            return Ok(wordsReport);
        }

    }


}
