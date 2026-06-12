using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
            CreateMap<BoardDecision, BoardDecisionResponse>()
                .ForMember(dest => dest.VoteCount,
                    opt => opt.MapFrom(src => src.BoardVotes.Count(v => v.DeletedAt == null)));

            CreateMap<BoardVote, BoardVoteResponse>()
                .ForMember(dest => dest.VoterName,
                    opt => opt.MapFrom(src => src.Voter.DisplayName));
        }
    }
}
