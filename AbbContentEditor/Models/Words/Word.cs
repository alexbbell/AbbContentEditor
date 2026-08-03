using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace AbbContentEditor.Models.Words
{
    public class Word
    {
        public Guid Id {  get; set; }    
        public string Translate1 { get; set; }
        public string Translate2 { get; set; }
        public string? Translate3 { get; set; }
    }

    
    public class WordCollectionBase : BaseClass
    {
        private string? WordsCollectionString { get; set; }
        public AbbAppUser Author { get; set; }
    }

    public class WordCollection : WordCollectionBase
    {
        public JsonDocument? WordsCollection
        {
            get => WordsCollectionString is null ? null : JsonDocument.Parse(WordsCollectionString);
            set => WordsCollectionString = value?.RootElement.GetRawText();
        }
        private string? WordsCollectionString { get; set; }
        // public JsonDocument WordsCollection { get; set; }
        public AbbAppUser Author { get; set; }
    }

    public class CreateWordCollectionDto
    {
        public string Title { get; set; }
        public List<Word> Words { get; set; }
    }

}
