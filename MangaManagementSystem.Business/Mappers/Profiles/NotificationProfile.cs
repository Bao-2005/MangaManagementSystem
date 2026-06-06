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
                .ForMember(dest => dest.Title,
                    opt => opt.MapFrom(src => src.Notification.Title))
                .ForMember(dest => dest.Message,
                    opt => opt.MapFrom(src => src.Notification.Message))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Notification.Type))
                .ForMember(dest => dest.Link,
                    opt => opt.MapFrom(src => src.Notification.Link))
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Notification.Priority))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.Notification.CreatedAt));
        }
    }
}
