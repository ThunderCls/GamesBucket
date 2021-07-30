using AutoMapper;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;

namespace GamesBucket.App.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // https://www.parrisvarney.com/unflatten.html

            CreateMap<Game, SearchResult>().ReverseMap();
            
            //CreateMap<AppUser, UserProfileSecurityViewModel>().ReverseMap();
            //    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            //    .ForMember(dest => dest.NewPassword, opt => opt.MapFrom(src => src.Credentials.Password))
            //    .ReverseMap();

            //CreateMap<AppUser, UserViewModel>().ReverseMap();
            //CreateMap<Intake, CaloriesListGraphViewModel>()
            //    .ForMember(dest => dest.date, opt => opt.MapFrom(src => src.Date.ToString("d")))
            //    .ForMember(dest => dest.achieved, opt => opt.MapFrom(src => src.))
        }
    }
}