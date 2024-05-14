using AutoMapper;
using TicketFinder_Models;
using TicketsFinder_API.Models;

namespace TicketsFinder_API.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile() 
        {
            CreateMap<Notification, NotificationDTO>().ReverseMap();
            CreateMap<UserHistory, UserHistoryDTO>().ReverseMap();
        }
    }
}
