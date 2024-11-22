using AbbContentEditor.Controllers;
using AbbContentEditor.Models;

namespace AbbContentEditor.Helpers
{
    public class ImageUtilities
    {
        private IConfiguration _configuration;
        private string _mainUploadDir;
        private ILogger _logger;

        public ImageUtilities(IConfiguration configuration, ILogger<ImageUtilities> logger)
        {
            _configuration = configuration;
            _mainUploadDir = configuration.GetSection("UploadFolder").Value;
            _logger = logger;
        }

        /// <summary>  
        /// This method Moves the files from the temporary folder to a directory with ID name
        /// </summary>  
        /// <remarks>This method Moves the files from the temporary folder to a directory with ID name</remarks>
        /// <param name="MoveFilesRequest">TODO</param>  
        /// <returns> List<OperationResult></returns> 
        public List<OperationResult> MoveFiles(MoveFilesRequest request)
        {
            string _sourceImageDirectory = Path.Combine(_mainUploadDir, "UploadedImagesTMP");
            string _destImageDirectory = Path.Combine(_mainUploadDir, "uploads", request.PostId.ToString());
            if (!Directory.Exists(_sourceImageDirectory)) Directory.CreateDirectory(_sourceImageDirectory);
            if (!Directory.Exists(_destImageDirectory)) Directory.CreateDirectory(_destImageDirectory);

            List<OperationResult> moveResult = new List<OperationResult>();
            int movedFiles = 0;
            int totalFiles = request.Files.Length;
            try
            {
                foreach (var item in request.Files)
                {
                    try
                    {
                        string srcFile = Path.Combine(_sourceImageDirectory, item);
                        string dstFile = Path.Combine(_destImageDirectory, item);
                        File.Move(srcFile, dstFile);
                        moveResult.Add(new OperationResult() { Title = item, Result = "Success" });
                        movedFiles++;
                    }
                    catch (Exception ex)
                    {
                        moveResult.Add(new OperationResult() { Title = item, Result = $"Error" });
                        _logger.LogError(ex.Message);
                    }
                }
                return moveResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public bool RemoveFile(string directory, string file)
        {
            string filePath = Path.Combine(directory, file);
            if (File.Exists(filePath ))
            {
                File.Delete(filePath);
                return true;
            }
            _logger.LogError($"File {filePath} doesn't exist");
            return false;
        }
    }
}
