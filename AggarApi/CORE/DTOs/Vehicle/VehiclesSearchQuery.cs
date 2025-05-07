using DATA.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class VehiclesSearchQuery
    {
        public int pageNo { get; set; }
        public int pageSize { get; set; }
        public bool byNearest { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? searchKey { get; set; }
        public int? brandId { get; set; }
        public int? typeId { get; set; }
        public VehicleTransmission? transmission { get; set; }
        public double? rate { get; set; }
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
        public int? year { get; set; }
    }
}
