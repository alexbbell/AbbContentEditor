using AbbContentEditor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AbbContentEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceController : ControllerBase
    {

        // Метод для получения списка категорий
        [HttpGet("categories")]
        public ActionResult<IEnumerable<RefItem>> GetCategories()
        {
            // Зашитые в код категории
            var categories = new List<RefItem> { 
                new RefItem { Id = 1, Title = "Electronics" },
                new RefItem { Id = 2, Title = "Electronics" },
                new RefItem { Id = 3, Title = "Electronics" } 
            };
            return Ok(categories);
        }

        // Метод для получения списка валют
        [HttpGet("currencies")]
        public ActionResult<IEnumerable<string>> GetCurrencies()
        {
            // Зашитые в код валюты
            var currencies = new List<string> { "USD", "EUR", "JPY", "GBP" };
            return Ok(currencies);
        }
    }
}
