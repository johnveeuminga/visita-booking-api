using AutoMapper;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Mapping
{
    public class AccommodationMappingProfile : Profile
    {
        public AccommodationMappingProfile()
        {
            CreateMap<Accommodation, AccommodationResponseDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.FullName : "Unknown"))
                .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Email : "Unknown"))
                .ForMember(dest => dest.ActiveRoomCount, opt => opt.MapFrom(src => src.Rooms.Count(r => r.IsActive)));

            CreateMap<Accommodation, AccommodationSummaryDto>()
                .ForMember(dest => dest.ActiveRoomCount, opt => opt.MapFrom(src => src.Rooms.Count(r => r.IsActive)));

            CreateMap<CreateAccommodationRequestDto, Accommodation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Rooms, opt => opt.Ignore());

            CreateMap<UpdateAccommodationRequestDto, Accommodation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Rooms, opt => opt.Ignore());
        }
    }
}