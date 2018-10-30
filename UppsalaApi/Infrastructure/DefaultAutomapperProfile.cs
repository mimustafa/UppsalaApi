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


            CreateMap<OpeningEntity, OpeningResource>()
                    .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate / 100m))
                    .ForMember(dest => dest.StartAt, opt => opt.MapFrom(src => src.StartAt.UtcDateTime))
                    .ForMember(dest => dest.EndAt, opt => opt.MapFrom(src => src.EndAt.UtcDateTime))
                    .ForMember(dest => dest.Room, opt => opt.MapFrom(src =>
                        Link.To(nameof(Controllers.RoomsController.GetRoomByIdAsync), new { roomId = src.RoomId })));

            CreateMap<BookingEntity, BookingResoure>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total / 100m))
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(
                        nameof(Controllers.BookingsController.GetBookingByIdAsync),
                        new { bookingId = src.Id })))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src =>
                    Link.To(
                        nameof(Controllers.RoomsController.GetRoomByIdAsync),
                        new { roomId = src.Room.Id })));


        }
    }
}
