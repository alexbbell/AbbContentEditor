using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace AbbContentEditor.Models.Words
{
    public class Word
    {
        public int Id {  get; set; }    
        public string Translate1 { get; set; }
        public string Translate2 { get; set; }
        public string Translate3 { get; set; }
    }

    public class WordCollection : BaseClass 
    {
        public int Id { get; set; }
        public JsonDocument WordsCollection { get; set; }
        public IdentityUser Author { get; set; }
    }

}
