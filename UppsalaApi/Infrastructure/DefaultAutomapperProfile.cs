using System;
using AutoMapper;
using UppsalaApi.Controllers;
using UppsalaApi.Models;

namespace UppsalaApi.Infrastructure
{
    public class DefaultAutomapperProfile : Profile
    {
        public DefaultAutomapperProfile()
        {
            CreateMap<RoomEntity, RoomResource>()
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate / 100.0m))
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(RoomsController.GetRoomByIdAsync), new { roomId = src.Id })));
                      
        }
    }
}
