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
using CORE.DTOs.Rental;
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
                if (report.TargetAppUserId == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target user not found",
                    };

                var appUser = await _unitOfWork.AppUsers.GetAsync(report.TargetAppUserId.Value);

                if(appUser == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target user not found",
                    };
                result.TargetAppUser = _mapper.Map<SummerizedUserDto>(appUser);
            }
            else if (report.TargetType == TargetType.CustomerReview)
            {
                if (report.TargetCustomerReviewId == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target customerReview not found",
                    };

                var customerReview = await _unitOfWork.CustomerReviews.GetAsync(report.TargetCustomerReviewId.Value);

                if (customerReview == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target customerReview not found",
                    };
                result.TargetCustomerReview = _mapper.Map<GetReviewDto>(customerReview);
            }
            else if (report.TargetType == TargetType.RenterReview)
            {
                if (report.TargetRenterReviewId == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target renterReview not found",
                    };

                var renterReview = await _unitOfWork.RenterReviews.GetAsync(report.TargetRenterReviewId.Value);

                if (renterReview == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target renterReview not found",
                    };
                result.TargetRenterReview = _mapper.Map<GetReviewDto>(renterReview);
            }
            else if (report.TargetType == TargetType.Message)
            {
                if (report.TargetMessageId == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target message not found",
                    };

                var message = await _unitOfWork.Chat.GetAsync(report.TargetMessageId.Value);

                if (message == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target message not found",
                    };
                if (message.MessageType == MessageType.ContentMessage)
                    result.TargetContentMessage = _mapper.Map<GetContentMessageDto>(message as ContentMessage);
                else if (message.MessageType == MessageType.FileMessage)
                    result.TargetFileMessage = _mapper.Map<GetFileMessageDto>(message as FileMessage);
            }
            else if (report.TargetType == TargetType.Vehicle)
            {
                if (report.TargetVehicleId == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target vehicle not found",
                    };

                var vehicle = await _unitOfWork.Vehicles.GetAsync(report.TargetVehicleId.Value);

                if (vehicle == null)
                    return new ResponseDto<GetReportDto>
                    {
                        StatusCode = StatusCodes.BadRequest,
                        Message = "Target vehicle not found",
                    };
                result.TargetVehicle = _mapper.Map<GetVehicleSummaryDto>(vehicle);
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
            var includes = new string[] { ReportIncludes.Reporter };

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

        public async Task<ResponseDto<PagedResultDto<IEnumerable<GetReportDto>>>> FilterReportsAsync(ReportFilterDto dto, int maxPageSize = 30)
        {
            if (PaginationHelpers.ValidatePaging(dto.PageNo, dto.PageSize, maxPageSize) is string paginationError)
            {
                _logger.LogWarning("Invalid pagination parameters: {PaginationError}", paginationError);
                return new ResponseDto<PagedResultDto<IEnumerable<GetReportDto>>>
                {
                    StatusCode = StatusCodes.BadRequest,
                    Message = paginationError
                };
            }
            var includes = new string[] { ReportIncludes.Reporter };
            var reports = await _unitOfWork.Reports.FilterReportsAsync(dto.PageNo, dto.PageSize, dto.TargetType, dto.Status, dto.Date, dto.SortingDirection, includes);

            var groups = reports.GroupBy(r => r.TargetType);

            HashSet<int> targetAppUsersIds = new HashSet<int>();
            HashSet<int> targetVehiclesIds = new HashSet<int>();
            HashSet<int> targetCustomerReviewsIds = new HashSet<int>();
            HashSet<int> targetRenterReviewsIds = new HashSet<int>();
            HashSet<int> targetMessagesIds = new HashSet<int>();

            foreach (var group in groups)
            {
                var targetType = group.Key;
                switch (targetType)
                {
                    case TargetType.AppUser:
                        targetAppUsersIds.UnionWith(group.Select(r => r.TargetAppUserId).Where(id => id.HasValue).Select(id => id.Value));
                        break;
                    case TargetType.Vehicle:
                        targetVehiclesIds.UnionWith(group.Select(r => r.TargetVehicleId).Where(id => id.HasValue).Select(id => id.Value));
                        break;
                    case TargetType.CustomerReview:
                        targetCustomerReviewsIds.UnionWith(group.Select(r => r.TargetCustomerReviewId).Where(id => id.HasValue).Select(id => id.Value));
                        break;
                    case TargetType.RenterReview:
                        targetRenterReviewsIds.UnionWith(group.Select(r => r.TargetRenterReviewId).Where(id => id.HasValue).Select(id => id.Value));
                        break;
                    case TargetType.Message:
                        targetMessagesIds.UnionWith(group.Select(r => r.TargetMessageId).Where(id => id.HasValue).Select(id => id.Value));
                        break;
                }
            }

            IEnumerable<AppUser> appUsers = null;
            IEnumerable<Vehicle> vehicles = null;
            IEnumerable<CustomerReview> customerReviews = null;
            IEnumerable<RenterReview> renterReviews = null;
            IEnumerable<Message> messages = null;

            if (targetAppUsersIds.Count > 0)
                appUsers = await _unitOfWork.AppUsers.FindAsync(u => targetAppUsersIds.Contains(u.Id), dto.PageNo, dto.PageSize);
            if (targetVehiclesIds.Count > 0)
                vehicles = await _unitOfWork.Vehicles.FindAsync(v => targetVehiclesIds.Contains(v.Id), dto.PageNo, dto.PageSize);
            if (targetCustomerReviewsIds.Count > 0)
                customerReviews = await _unitOfWork.CustomerReviews.FindAsync(cr => targetCustomerReviewsIds.Contains(cr.Id), dto.PageNo, dto.PageSize);
            if (targetRenterReviewsIds.Count > 0)
                renterReviews = await _unitOfWork.RenterReviews.FindAsync(rr => targetRenterReviewsIds.Contains(rr.Id), dto.PageNo, dto.PageSize);
            if (targetMessagesIds.Count > 0)
                messages = await _unitOfWork.Chat.FindAsync(m => targetMessagesIds.Contains(m.Id), dto.PageNo, dto.PageSize);

            var result = new List<GetReportDto>();
            var count = await _unitOfWork.Reports.FilterReportsCountAsync(dto.TargetType, dto.Status, dto.Date);

            foreach (var report in reports)
            {
                var reportDto = _mapper.Map<GetReportDto>(report);
                if(report.Reporter != null)
                    reportDto.Reporter = _mapper.Map<SummerizedUserDto>(report.Reporter);

                if (report.TargetType == TargetType.AppUser)
                {
                    var user = appUsers?.FirstOrDefault(a=>a.Id == report.TargetAppUserId);
                    if(user != null)
                        reportDto.TargetAppUser = _mapper.Map<SummerizedUserDto>(user);
                }
                else if (report.TargetType == TargetType.Vehicle)
                {
                    var vehicle = vehicles?.FirstOrDefault(v=>v.Id == report.TargetVehicleId);
                    if (vehicle != null)
                        reportDto.TargetVehicle = _mapper.Map<GetVehicleSummaryDto>(vehicle);
                }
                else if (report.TargetType == TargetType.CustomerReview)
                {
                    var customerReview = customerReviews?.FirstOrDefault(cr => cr.Id == report.TargetCustomerReviewId);
                    if (customerReview != null)
                        reportDto.TargetCustomerReview = _mapper.Map<GetReviewDto>(customerReview);
                }
                else if (report.TargetType == TargetType.RenterReview)
                {
                    var renterReview = renterReviews?.FirstOrDefault(rr => rr.Id == report.TargetRenterReviewId);
                    if (renterReview != null)
                        reportDto.TargetRenterReview = _mapper.Map<GetReviewDto>(renterReview);
                }
                else if (report.TargetType == TargetType.Message)
                {
                    var message = messages?.FirstOrDefault(m => m.Id == report.TargetMessageId);
                    if (message != null)
                    {
                        if (message.MessageType == MessageType.ContentMessage)
                            reportDto.TargetContentMessage = _mapper.Map<GetContentMessageDto>(message as ContentMessage);
                        else if (message.MessageType == MessageType.FileMessage)
                            reportDto.TargetFileMessage = _mapper.Map<GetFileMessageDto>(message as FileMessage);
                    }
                }
                result.Add(reportDto);
            }

            return new ResponseDto<PagedResultDto<IEnumerable<GetReportDto>>>
            {
                StatusCode = StatusCodes.OK,
                Message = "Reports retrieved successfully",
                Data = PaginationHelpers.CreatePagedResult(result.AsEnumerable(), dto.PageNo, dto.PageSize, count),
            };
        }
    }
}
