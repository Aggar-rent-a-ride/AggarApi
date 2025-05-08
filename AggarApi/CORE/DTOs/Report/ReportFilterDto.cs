using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATA.Constants.Enums;
using DATA.Models.Enums;

namespace CORE.DTOs.Report
{
    public class ReportFilterDto
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TargetType? TargetType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportStatus? Status { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportDateFilter? Date { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderBy? SortingDirection { get; set; }
    }
}
