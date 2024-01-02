using AbbContentEditor.Models;
using Newtonsoft.Json;

namespace AbbContentEditor.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Comapare2Files()
        {
            string editedContent = File.ReadAllText(@"D:\Projects\alekseibeliaev3\client\public\locales\en\en-translation.json");
            string defaultContent = File.ReadAllText(@"D:\Projects\alekseibeliaev3\client\public\locales.bak\en\en-translation.json");

            var scEdited = JsonConvert.DeserializeObject<SiteContent>(editedContent);
            var scDefault = JsonConvert.DeserializeObject<SiteContent>(defaultContent);

            Assert.AreEqual(scEdited.main.titleAbout, scDefault.main.titleAbout);
            Assert.AreEqual(scEdited.main.email, scDefault.main.email);
            Assert.AreEqual(scEdited.main.description, scDefault.main.description);

            Assert.Pass();
        }
    }
}
