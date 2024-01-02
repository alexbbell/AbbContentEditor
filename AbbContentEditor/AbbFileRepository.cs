using AbbContentEditor.Models;
using Newtonsoft.Json;

namespace AbbContentEditor
{
    public class AbbFileRepository
    {
        public string fileContent { get; set; }
        private readonly IConfiguration _configuration;
        private readonly ILogger<AbbFileRepository> _logger;
        private string _workDir { get; set; } 
        private string _workFile { get; set; } 

        public AbbFileRepository(IConfiguration configuration, ILogger<AbbFileRepository> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _workDir = _configuration.GetSection("FileDir").Value;
        }

        public string ReadLangContent(string lang)
        {
            _workFile = Path.Combine(_workDir, lang, $"{lang}-translation.json");
            _logger.LogInformation(_workFile);
            try
            {
                fileContent = System.IO.File.ReadAllText(_workFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return fileContent;
        }

        public Boolean WriteLangContent(string lang, SiteContent content)
        {
            try
            {
                _workFile = Path.Combine(_workDir, lang, $"{lang}-translation.json");
                _logger.LogInformation($"{_workFile} opened for record");
                using (StreamWriter file = File.CreateText(_workFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, content);
                }
                _logger.LogInformation($"{_workFile} closed after record");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
            return true;
        }

    }
}
