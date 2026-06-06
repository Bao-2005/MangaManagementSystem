using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class PageTaskProfile : Profile
    {
        public PageTaskProfile()
        {
            CreateMap<PageTask, PageTaskResponse>()
                .ForMember(dest => dest.AssistantName,
                    opt => opt.MapFrom(src => src.Assistant.DisplayName))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<PageTaskSubmission, SubmissionResponse>();
        }
    }
}
