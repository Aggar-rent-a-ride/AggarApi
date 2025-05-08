using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.DTOs.Report;
using DATA.Models;

namespace CORE.AutoMapperProfiles
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            CreateMap<CreateReportDto, Report>();
            CreateMap<Report, GetReportDto>()
                .ForMember(dest => dest.Reporter, opt => opt.Ignore())
                .ForMember(dest => dest.TargetAppUser, opt => opt.Ignore())
                .ForMember(dest => dest.TargetCustomerReview, opt => opt.Ignore())
                .ForMember(dest => dest.TargetRenterReview, opt => opt.Ignore())
                .ForMember(dest => dest.TargetFileMessage, opt => opt.Ignore())
                .ForMember(dest => dest.TargetContentMessage, opt => opt.Ignore())
                .ForMember(dest => dest.TargetVehicle, opt => opt.Ignore());
        }
    }
}
