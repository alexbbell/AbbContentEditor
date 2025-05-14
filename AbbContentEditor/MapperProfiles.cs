using AutoMapper;
using AbbContentEditor.Models;
using AbbContentEditor.Models.Words;
using Microsoft.AspNetCore.Identity;

namespace AbbContentEditor
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<Blog, BlogListItem>(); //.ForMember(dest => dest.CategoryName, act => act.MapFrom(src => src.Category.Name));
            CreateMap<Blog, BlogListItemUser>();
            CreateMap<WordHistory, WordHistoryDto>(); //.ForMember(dest => dest.UserId, act => act.MapFrom(src=> src.IdentityUser.Id));
            CreateMap<WordHistoryDto, WordHistory>().ForMember(dest => dest.IdentityUser, opt => opt.Ignore());
            CreateMap<AbbAppUser, UserDto>();
        }
    }
}
