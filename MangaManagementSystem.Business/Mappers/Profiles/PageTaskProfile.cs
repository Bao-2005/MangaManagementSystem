using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Tasks;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles;

    public class PageTaskProfile : Profile
    {
        public PageTaskProfile()
        {
            CreateMap<PageTask, PageTaskResponse>()
            .ForMember(dest => dest.AssistantName, opt => opt.MapFrom(src => src.Assistant.DisplayName))
            .ForMember(dest => dest.Submissions, opt => opt.MapFrom(src => src.Submissions.OrderByDescending(x => x.VersionNo)));

            CreateMap<PageTaskSubmission, PageTaskSubmissionResponse>()
            .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.SubmittedFileAsset.OriginalFileName))
            .ForMember(dest => dest.ObjectPath, opt => opt.MapFrom(src => src.SubmittedFileAsset.ObjectPath));
    }
}
