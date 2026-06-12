using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class ProposalPageProfile : Profile
    {
        public ProposalPageProfile()
        {
            CreateMap<ProposalPage, ProposalPageResponse>();
        }
    }
}
