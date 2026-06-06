using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses;
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
