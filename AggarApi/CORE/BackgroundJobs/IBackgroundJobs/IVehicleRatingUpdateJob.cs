﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.BackgroundJobs.IBackgroundJobs
{
    public interface IVehicleRatingUpdateJob
    {
        void ScheduleVehicleRatingUpdate(int vehicleId);
    }
}
