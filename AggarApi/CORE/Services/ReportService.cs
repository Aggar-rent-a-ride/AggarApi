using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.DTOs;
using CORE.DTOs.Report;
using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;

namespace CORE.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReportService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        private async Task<string?> CheckTargetExists(int? targetId, TargetType type)
        {
            if (targetId == null && type != TargetType.None)
                return "Target id is required";
            else if(type == TargetType.None && targetId != null)
                return "Target type is required";
            else if (type == TargetType.None)
                return null;
            else if (type == TargetType.User)
            {
                if (await _unitOfWork.AppUsers.CheckAnyAsync(t => t.Id == targetId, null) == false)
                    return "User not found";
            }
            else if (type == TargetType.Vehicle)
            {
                if (await _unitOfWork.Vehicles.CheckAnyAsync(t => t.Id == targetId, null) == false)
                    return "Vehicle not found";
            }
            else if (type == TargetType.Message)
            {
                if (await _unitOfWork.Chat.CheckAnyAsync(t => t.Id == targetId, null) == false)
                    return "Message not found";
            }
            else if (type == TargetType.RenterReview)
            {
                if (await _unitOfWork.RenterReviews.CheckAnyAsync(t => t.Id == targetId, null) == false)
                    return "Renter review not found";
            }
            else if (type == TargetType.CustomerReview)
            {
                if (await _unitOfWork.CustomerReviews.CheckAnyAsync(t => t.Id == targetId, null) == false)
                    return "Customer review not found";
            }
            return null;
        }
        public async Task<ResponseDto<object>> CreateReportAsync(CreateReportDto reportDto, int reporterId)
        {
            if(await CheckTargetExists(reportDto.TargetId, reportDto.TargetType) is string error)
                return new ResponseDto<object>
                {
                    StatusCode = 400,
                    Message = error,
                };

            var report = _mapper.Map<Report>(reportDto);
            report.ReporterId = reporterId;
            await _unitOfWork.Reports.AddOrUpdateAsync(report);
            var changes = await _unitOfWork.CommitAsync();
            
            if(changes == 0)
                return new ResponseDto<object>
                {
                    StatusCode = 500,
                    Message = "Failed to create report",
                };
            
            return new ResponseDto<object>
            {
                StatusCode = 200,
                Message = "Report created successfully",
            };
        }
    }
}
