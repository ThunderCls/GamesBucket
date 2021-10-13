using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace GamesBucket.App.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // https://www.parrisvarney.com/unflatten.html

            CreateMap<Game, SearchResult>().ReverseMap();
            CreateMap<Game, EditGameViewModel>().ReverseMap();
            CreateMap<Game, NewGameView>()
                .ForMember(ng => ng.Genres,
                    opt =>
                        opt.MapFrom(g => string.Join(";", g.Genres)));
            CreateMap<NewGameView, Game>()
                .ForMember(g => g.Genres,
                    opt =>
                        opt.MapFrom((s, d) =>
                        {
                            var genres = s.Genres.Split(";", StringSplitOptions.RemoveEmptyEntries);
                            var genresList = genres.Select(g => new Genres {Name = g.Trim()});
                            return genresList;
                        }));
            CreateMap<AppUser, ProfileDetailsViewModel>().ReverseMap();
            CreateMap<AppUser, ProfileSecurityViewModel>().ReverseMap();
            //CreateMap<AppUser, IdentityUser>().ReverseMap();
            CreateMap<AppUser, UserRegisterViewModel>().ReverseMap();
        }
    }
}