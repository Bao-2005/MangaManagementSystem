using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Chapters;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class ChapterPageProfile : Profile
    {
        public ChapterPageProfile()
        {
            CreateMap<ChapterPage, ChapterPageResponse>();
        }
    }
}
