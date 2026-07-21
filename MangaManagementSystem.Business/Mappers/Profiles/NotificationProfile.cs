using AutoMapper;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.Business.Mappers.Profiles
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationResponse>();

            CreateMap<UserNotification, UserNotificationResponse>()
                .ForMember(dest => dest.Message,
                    opt => opt.MapFrom(src => src.Notification.Message))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.Notification.CreatedAt));
        }
    }
}
