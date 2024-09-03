using AutoMapper;
using AbbContentEditor.Models;

namespace AbbContentEditor
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<Blog, BlogListItem>(); //.ForMember(dest => dest.CategoryName, act => act.MapFrom(src => src.Category.Name));
            CreateMap<Blog, BlogListItemUser>();
        }
    }
}
