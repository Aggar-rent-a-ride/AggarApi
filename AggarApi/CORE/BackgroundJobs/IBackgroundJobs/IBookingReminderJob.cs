using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface IBookingReminderJob
    {
        //days before booking is the number of days before the booking start date to send notification in
        void ScheduleBookingReminderNotification(DateTime bookingStartDate, int userId, int daysBeforeBooking);
    }
}
