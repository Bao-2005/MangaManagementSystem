using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class GenreProfile : Profile
    {
        public GenreProfile()
        {
            CreateMap<Genre, GenreResponse>();
        }
    }
}
