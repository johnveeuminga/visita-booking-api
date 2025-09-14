using AutoMapper;
using visita_booking_api.Models.Entities;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Mapping
{
    public class RoomMappingProfile : Profile
    {
        public RoomMappingProfile()
        {
            CreateMap<Room, RoomDetailsDTO>()
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder)))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Amenities))
                .ForMember(dest => dest.PricingRules, opt => opt.MapFrom(src => src.PricingRules.Where(pr => pr.IsActive)))
                .ForMember(dest => dest.MainPhotoUrl, opt => opt.MapFrom(src => src.MainPhotoUrl))
                .ForMember(dest => dest.Accommodation, opt => opt.MapFrom(src => src.Accommodation));

            CreateMap<Room, RoomListItemDTO>()
                .ForMember(dest => dest.MainPhotoUrl, opt => opt.MapFrom(src => src.MainPhotoUrl))
                .ForMember(dest => dest.PhotoCount, opt => opt.MapFrom(src => src.Photos.Count(p => p.IsActive)))
                .ForMember(dest => dest.AmenityCount, opt => opt.MapFrom(src => src.Amenities.Count))
                .ForMember(dest => dest.MainAmenities, opt => opt.MapFrom(src => src.Amenities.Take(5).Select(a => a.Name).ToList()))
                .ForMember(dest => dest.Accommodation, opt => opt.MapFrom(src => src.Accommodation));

            CreateMap<Room, RoomSearchResultDTO>()
                .ForMember(dest => dest.MainPhotoUrl, opt => opt.MapFrom(src => src.MainPhotoUrl))
                .ForMember(dest => dest.PhotoUrls, opt => opt.MapFrom(src => src.Photos.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).Take(3).Select(p => p.CdnUrl ?? p.S3Url).ToList()))
                .ForMember(dest => dest.TopAmenities, opt => opt.MapFrom(src => src.Amenities.Take(5)))
                .ForMember(dest => dest.TotalAmenities, opt => opt.MapFrom(src => src.Amenities.Count))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.CalculatedPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.AvailabilityStatus, opt => opt.Ignore())
                .ForMember(dest => dest.RelevanceScore, opt => opt.Ignore())
                .ForMember(dest => dest.PopularityScore, opt => opt.Ignore())
                .ForMember(dest => dest.DailyPrices, opt => opt.Ignore());

            CreateMap<RoomCreateDTO, Room>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CacheVersion, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.RoomAmenities, opt => opt.Ignore())
                .ForMember(dest => dest.PricingRules, opt => opt.Ignore())
                .ForMember(dest => dest.AvailabilityOverrides, opt => opt.Ignore())
                .ForMember(dest => dest.Amenities, opt => opt.Ignore());

            CreateMap<RoomUpdateDTO, Room>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CacheVersion, opt => opt.Ignore())
                .ForMember(dest => dest.AccommodationId, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.RoomAmenities, opt => opt.Ignore())
                .ForMember(dest => dest.PricingRules, opt => opt.Ignore())
                .ForMember(dest => dest.AvailabilityOverrides, opt => opt.Ignore())
                .ForMember(dest => dest.Amenities, opt => opt.Ignore());
        }
    }

    public class RoomPhotoMappingProfile : Profile
    {
        public RoomPhotoMappingProfile()
        {
            CreateMap<RoomPhoto, RoomPhotoDTO>()
                .ForMember(dest => dest.FileUrl, opt => opt.MapFrom(src => src.CdnUrl ?? src.S3Url));
        }
    }

    public class AmenityMappingProfile : Profile
    {
        public AmenityMappingProfile()
        {
            CreateMap<Amenity, AmenityDTO>();
            CreateMap<AmenityCreateDTO, Amenity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.RoomAmenities, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAmenity, opt => opt.Ignore())
                .ForMember(dest => dest.ChildAmenities, opt => opt.Ignore());
        }
    }

    public class RoomPricingRuleMappingProfile : Profile
    {
        public RoomPricingRuleMappingProfile()
        {
            CreateMap<RoomPricingRule, RoomPricingRuleDTO>();
            CreateMap<RoomPricingRuleCreateDTO, RoomPricingRule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore());
        }
    }

    public class RoomAvailabilityMappingProfile : Profile
    {
        public RoomAvailabilityMappingProfile()
        {
            CreateMap<RoomAvailabilityOverride, RoomAvailabilityOverrideDTO>();
            CreateMap<RoomAvailabilityOverrideCreateDTO, RoomAvailabilityOverride>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore());
        }
    }
}