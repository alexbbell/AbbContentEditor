namespace AbbContentEditor.Models
{
    public class SiteContent
    {
        public string morelink { get; set; }
        public string fullname { get; set; }
        public Main main { get; set; }
        public Menu menu { get; set; }
        public Skills skills { get; set; }
        public Work work { get; set; }
        public Education education{ get; set; }

        public TheGame theGame {  get; set; }   
    }

    public class Menu
    {
        public string position { get; set; }
        public string resume { get; set; }
        public string contacts { get; set; }
        public string sitemap { get; set; }
        public string home{ get; set; }
        public string experience { get; set; }
        public string skills { get; set; }
        public string blogs { get; set; }
        public string about { get; set; }
        public string education { get; set; }
        public string math { get; set; }
        // public string gallery { get; set; }
        // public string lngmngr { get; set; }

    }


    public class Skills
    {
        public string title { get; set; }
        public IList<Skill> content { get; set; }
    }
    public class Skill
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Education
    {
        public string title { get; set; }
        public IList<EducationItem> content { get; set; }
    }

    public class Main
    {
        public string greeting { get; set; }
        public string beforename { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string titleAbout { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string bio { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string about { get; set; }
        public string website { get; set; }
        public string resumedownload { get; set; }
        public IList<Social> social { get; set; }

    }

    public class Social
    {
        public string name { get; set; }
        public string url { get; set; }
        public string className { get; set; }

    }

    public class Work
    {
        public string title { get; set; }
        public IList<WorkItem> content { get; set; }

    }

    public class WorkItem
    {
        public string company { get; set; }
        public string title { get; set; }
        public string years { get; set; }
        public string description { get; set; }

    }

    public class EducationItem
    {
        public string school { get; set; }
        public string degree { get; set; }
        public string graduated { get; set; }
        public string description { get; set; }

    }

    public class TheGame
    {
        public string chooseAnwer { get; set; }
        public string next { get; set; }

        public string gameTitle { get; set; }
        public string settings { get; set; }
        public string action { get; set; }
        public string addition { get; set; }
        public string subtraction { get; set; }
        public string multiplication { get; set; }
        public string division { get; set; }
        public string minValue { get; set; }
        public string maxValue { get; set; }
        public string errorMinMaxText { get; set; }

    }

}

