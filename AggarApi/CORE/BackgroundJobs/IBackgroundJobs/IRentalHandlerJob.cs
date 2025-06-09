using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface IRentalHandlerJob
    {
        Task ScheduleCancelNotConfirmedRentalAfterStartDateAsync(int rentalId, DateTime cancelDate);
    }
}
