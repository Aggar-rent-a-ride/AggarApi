using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs.AppUser;
using CORE.DTOs.Message;
using CORE.DTOs.Review;
using CORE.DTOs.Vehicle;

namespace CORE.DTOs.Report
{
    public class GetReportDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public SummerizedUserDto Reporter { get; set; }
        public string TargetType { get; set; }
        public SummerizedUserDto? TargetAppUser { get; set; }
        public GetVehicleSummaryDto? TargetVehicle { get; set; }
        public GetReviewDto? TargetCustomerReview { get; set; }
        public GetReviewDto? TargetRenterReview { get; set; }
        public GetContentMessageDto? TargetContentMessage { get; set; }
        public GetFileMessageDto? TargetFileMessage { get; set; }
    }
}
