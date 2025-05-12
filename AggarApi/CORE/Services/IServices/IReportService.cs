using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs;
using CORE.DTOs.Report;

namespace CORE.Services.IServices
{
    public interface IReportService
    {
        Task<ResponseDto<object>> CreateReportAsync(CreateReportDto reportDto, int reporterId);
        Task<ResponseDto<GetReportDto>> GetReportByIdAsync(int reportId);
        Task<ResponseDto<object>> UpdateReportsStatusAsync(UpdateReportsStatusDto dto);
        Task<ResponseDto<PagedResultDto<IEnumerable<GetReportDto>>>> FilterReportsAsync(ReportFilterDto dto, int maxPageSize = 30);
    }
}
