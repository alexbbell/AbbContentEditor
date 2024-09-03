namespace AbbContentEditor.Models
{

    public abstract class BaseClass
    {
        public int Id { get; set; }
        public DateTime? PubDate { get; set; }
        public DateTime UpdDate { get; set; } = DateTime.UtcNow;

    }
    public class Category : BaseClass
    {
        public string Name { get; set; }
    }

    public class Blog : BaseClass
    {
        
        public string Title { get; set; }
        public string Preview { get; set; }
        public string TheText { get; set; }
        public string ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public Boolean IsDeleted {  get; set; }
    }

    public class BlogListItem : BaseClass
    {
        public string Title { get; set; }
        public string Preview { get; set; }
        public string ImageUrl { get; set; }
        public string? CategoryName {  get; set; }
        public int? CategoryId { get; set;}
        public Boolean IsDeleted { get; set; }  
    }

    public class BlogListItemUser: BlogListItem
    {     
        public string TheText { get; set;}
     
    }

}
