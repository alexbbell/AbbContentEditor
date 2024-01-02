using AbbContentEditor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LangController : ControllerBase
    {
        private string _fileContent {get; set; }
        private IConfiguration _configuration { get; set; }
        private readonly ILogger<LangController> _logger;
        private readonly AbbFileRepository _fileRepository;
        
        public LangController(IConfiguration configuration, AbbFileRepository repository, ILogger<LangController> logger, AbbFileRepository fileRepository)
        {
            _configuration= configuration;
            _logger = logger;
            _fileRepository = fileRepository;
        }

        [HttpGet("{key}")]
        [Authorize]
        public SiteContent GetJSONValue(string key)
        {
            string t = String.Empty;
            SiteContent sc = new SiteContent();

            _fileContent = _fileRepository.ReadLangContent(key);
            _logger.LogInformation($"The file ${key} has been read");

            try
            {
                sc = JsonConvert.DeserializeObject< SiteContent>(_fileContent);
            }
            catch (Exception ex)
            {
                t = ex.Message.ToString();
                _logger.LogError($"{ex.Message.ToString()}");
            }

            //SiteContent siteContent = (SiteContent)JsonConvert.DeserializeObject(_fileContent);
            //Dictionary<string, string> dictObj = jsonObj.ToObject<Dictionary<string, string>>();
            //string result = String.Empty;
            //dictObj.TryGetValue("position", out result);
            return sc;
        }

        [Authorize]
        [HttpPost()]
        public Boolean UpdateFile(string lang, [FromBody] SiteContent content) 
        {
            Boolean result = false;
            //SiteContent s = JsonConvert.DeserializeObject<SiteContent>(content);
            _logger.LogInformation("Start update");
            _logger.LogInformation("Lang: " + lang);
            try
            {
                result = _fileRepository.WriteLangContent(lang, content);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"{ex.Message.ToString()}");
            }

            _logger.LogInformation( (result) ? $"Updated {lang} file. With content ${content.fullname}" : "problem with updating");


            return result;
        }



    }
}
