using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class SeriesProfile : Profile
    {
        public SeriesProfile()
        {
            CreateMap<Series, SeriesResponse>()
                .ForMember(dest => dest.MangakaName,
                    opt => opt.MapFrom(src => src.Mangaka.DisplayName))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Genres,
                    opt => opt.MapFrom(src =>
                        src.SeriesGenres
                           .Where(sg => sg.Genre.DeletedAt == null)
                           .Select(sg => sg.Genre.Title)
                           .ToList()));

            CreateMap<Series, SeriesDetailResponse>()
                .IncludeBase<Series, SeriesResponse>()
                .ForMember(dest => dest.ProposalPages,
                    opt => opt.MapFrom(src =>
                        src.ProposalPages.Where(p => p.DeletedAt == null)));
        }
    }
}
