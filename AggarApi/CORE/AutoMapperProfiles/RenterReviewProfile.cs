using AutoMapper;
using CORE.DTOs.Review;
using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.AutoMapperProfiles
{
    public class RenterReviewProfile : Profile
    {
        public RenterReviewProfile()
        {
            CreateMap<RenterReview, SummarizedReviewDto>()
                .ForMember(dest=>dest.ReviewId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => Math.Round((src.Care + src.Behavior + src.Punctuality) / 3, 1)))
                .ForMember(dest => dest.Reviewer, opt => opt.MapFrom(src => src.Renter))
                .ReverseMap();

            CreateMap<Renter, ReviewerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath))
                .ReverseMap();
        }
    }
}
