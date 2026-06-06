using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class VoteProfile : Profile
    {
        public VoteProfile()
        {
            CreateMap<VoteRecord, VoteRecordResponse>();
            CreateMap<RankingSnapshot, RankingSnapshotResponse>();
        }
    }
}
