using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATA.Models.Enums;

namespace CORE.DTOs.Report
{
    public class UpdateReportsStatusDto
    {
        public HashSet<int> ReportsIds { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportStatus Status { get; set; }
    }
}
