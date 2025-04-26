using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATA.Models;
using DATA.Models.Enums;

namespace CORE.DTOs.Report
{
    public class CreateReportDto
    {
        public int? TargetId { get; set; }
        public string? Description { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TargetType TargetType { get; set; }
    }
}
