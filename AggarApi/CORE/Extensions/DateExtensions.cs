using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Extensions
{
    public static class DateExtensions
    {
        public static int TotalDays(this DateTime endDate, DateTime startDate)
        {
            int totalDays = (endDate - startDate).Days;
            if ((endDate - startDate).TotalHours > totalDays * 24)
                totalDays++;

            return totalDays;
        }
    }
}
