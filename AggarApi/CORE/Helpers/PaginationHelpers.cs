using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.DTOs;

namespace CORE.Helpers
{
    public static class PaginationHelpers
    {
        public static PagedResultDto<T> CreatePagedResult<T>(T items, int pageNo, int pageSize, double totalCount)
        {
            return new PagedResultDto<T>
            {
                Data = items,
                PageNumber = pageNo,
                PageSize = pageSize,
                TotalPages = CalculateTotalPages(totalCount, pageSize),
            };
        }
        public static int CalculateTotalPages(double countData, double pageSize)
        {
            if(pageSize == 0)
                return 1;

            return (int)Math.Ceiling(countData / pageSize);
        }
        public static string? ValidatePaging(int pageNo, int pageSize, int maxPageSize)
        {
            if (pageNo <= 0)
                return "Page number must be greater than 0";
            if (pageSize <= 0)
                return "Page size must be greater than 0";
            if (pageSize > maxPageSize)
                return $"Page size must be less than or equal to {maxPageSize}";
            return null;
        }
    }
}
