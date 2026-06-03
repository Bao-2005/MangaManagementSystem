using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.DataAccess.Entities.Models;
using System;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class ManuscriptProfile : Profile
    {
        public ManuscriptProfile()
        {
            CreateMap<Manuscript, ManuscriptResponse>()
                .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.Chapter != null && src.Chapter.Series != null ? (Guid?)src.Chapter.Series.SeriesId : null))
                .ForMember(dest => dest.SeriesTitle, opt => opt.MapFrom(src => src.Chapter != null && src.Chapter.Series != null ? src.Chapter.Series.Title : null))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.Chapter != null ? src.Chapter.ChapterNo : 0))
                .ForMember(dest => dest.ChapterTitle, opt => opt.MapFrom(src => src.Chapter != null ? src.Chapter.Title : null))
                .ForMember(dest => dest.Progress, opt => opt.Ignore());

            CreateMap<Manuscript, ManuscriptSummaryResponse>()
                .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.Chapter != null && src.Chapter.Series != null ? (Guid?)src.Chapter.Series.SeriesId : null))
                .ForMember(dest => dest.SeriesTitle, opt => opt.MapFrom(src => src.Chapter != null && src.Chapter.Series != null ? src.Chapter.Series.Title : null))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.Chapter != null ? src.Chapter.ChapterNo : 0))
                .ForMember(dest => dest.ChapterTitle, opt => opt.MapFrom(src => src.Chapter != null ? src.Chapter.Title : null))
                .ForMember(dest => dest.Progress, opt => opt.Ignore());
        }
    }
}
