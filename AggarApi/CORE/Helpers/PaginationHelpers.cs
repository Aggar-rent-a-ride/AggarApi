using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public static class PaginationHelpers
    {
        public static int CalculateTotalPages(double countData, double pageSize)
        {
            if(pageSize == 0)
                return 1;

            return (int)Math.Ceiling(countData / pageSize);
        }
        public static string? ValidatePaging(int pageNo, int pageSize)
        {
            if (pageNo <= 0 || pageSize <= 0)
                return "Invalid page number or page size";
            return null;
        }
    }
}
