using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CORE.Constants;
using CORE.DTOs;
using CORE.DTOs.AppUser;
using CORE.DTOs.Message;
using CORE.DTOs.Report;
using CORE.DTOs.Review;
using CORE.DTOs.Vehicle;
using CORE.Helpers;
using CORE.Services.IServices;
using DATA.Constants;
using DATA.Constants.Includes;
using DATA.DataAccess.Context;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using DATA.Models.Enums;
using Microsoft.Extensions.Logging;

namespace CORE.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;
        public ReportService(IMapper mapper, IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        private async Task<string?> CheckTargetExists(int? targetId, TargetType type)
        {
            if (targetId == null && type != TargetType.None)
                return "Target id is required";
            else if(type == TargetType.None && targetId != null)
                return "Target type is required";
            else if (type == TargetType.None)
                return null;
            else if (type == TargetType.AppUser)
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

        private void SetReportTarget(Report report, CreateReportDto reportDto)
        {
            report.TargetType = reportDto.TargetType;

            switch (reportDto.TargetType)
            {
                case TargetType.CustomerReview:
                    report.TargetCustomerReviewId = reportDto.TargetId;
                    break;

                case TargetType.RenterReview:
                    report.TargetRenterReviewId = reportDto.TargetId;
                    break;

                case TargetType.AppUser:
                    report.TargetAppUserId = reportDto.TargetId;
                    break;

                case TargetType.Vehicle:
                    report.TargetVehicleId = reportDto.TargetId;
                    break;

                case TargetType.Message:
                    report.TargetMessageId = reportDto.TargetId;
                    break;

                default:
                    break;
            };
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
            SetReportTarget(report, reportDto); // check here
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
                StatusCode = 201,
                Message = "Report created successfully",
            };
        }
        private async Task<ResponseDto<GetReportDto>> ProccessReportRetreival(Report report)
        {
            var result = _mapper.Map<GetReportDto>(report);

            if (report.Reporter == null)
                return new ResponseDto<GetReportDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Reporter not found",
                };

            result.Reporter = _mapper.Map<SummerizedUserDto>(report.Reporter);
            
            if (report.TargetType == TargetType.AppUser)
            {
                if(report.TargetAppUser == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target user not found",
                    };
                result.TargetAppUser = _mapper.Map<SummerizedUserDto>(report.TargetAppUser);
            }
            else if (report.TargetType == TargetType.CustomerReview)
            {
                if (report.TargetCustomerReview == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target customerReview not found",
                    };
                result.TargetCustomerReview = _mapper.Map<GetReviewDto>(report.TargetCustomerReview);
            }
            else if (report.TargetType == TargetType.RenterReview)
            {
                if (report.TargetRenterReview == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target renterReview not found",
                    };
                result.TargetRenterReview = _mapper.Map<GetReviewDto>(report.TargetRenterReview);
            }
            else if (report.TargetType == TargetType.Message)
            {
                if (report.TargetMessage == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target message not found",
                    };
                if (report.TargetMessage.MessageType == MessageType.ContentMessage)
                    result.TargetContentMessage = _mapper.Map<GetContentMessageDto>(report.TargetMessage as ContentMessage);
                else if (report.TargetMessage.MessageType == MessageType.FileMessage)
                    result.TargetFileMessage = _mapper.Map<GetFileMessageDto>(report.TargetMessage as FileMessage);
            }
            else if (report.TargetType == TargetType.Vehicle)
            {
                if (report.TargetVehicle == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target vehicle not found",
                    };
                result.TargetVehicle = _mapper.Map<GetVehicleSummaryDto>(report.TargetVehicle);
            }
            return new ResponseDto<GetReportDto>
            {
                StatusCode = StatusCodes.OK,
                Message = "Report retrieved successfully",
                Data = result,
            };
        }
        public async Task<ResponseDto<GetReportDto>> GetReportByIdAsync(int reportId)
        {
            var includes = new string[]
            {
                ReportIncludes.Reporter,
                ReportIncludes.TargetRenterReview,
                ReportIncludes.TargetCustomerReview,
                ReportIncludes.TargetAppUser,
                ReportIncludes.TargetVehicle,
                ReportIncludes.TargetMessage,
            };

            var report = await _unitOfWork.Reports.FindAsync(r => r.Id == reportId, includes);
            if(report == null)
            {
                _logger.LogWarning("Report with id {Id} not found", reportId);
                return new ResponseDto<GetReportDto>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Report not found",
                };
            }

            return await ProccessReportRetreival(report);
        }

        public async Task<ResponseDto<object>> UpdateReportsStatusAsync(UpdateReportsStatusDto dto)
        {
            var reports = await _unitOfWork.Reports.FindAsync(r => dto.ReportsIds.Contains(r.Id) && r.Status != dto.Status, 1, int.MaxValue);
            
            if(reports.Count() == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "No reports found to update",
                };

            foreach (var report in reports)
                report.Status = dto.Status;
            var changes = await _unitOfWork.CommitAsync();
            if (changes == 0)
                return new ResponseDto<object>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = "Failed to change report status",
                };
            return new ResponseDto<object>
            {
                StatusCode = StatusCodes.OK,
                Message = "Report status changed successfully",
            };
        }
    }
}
