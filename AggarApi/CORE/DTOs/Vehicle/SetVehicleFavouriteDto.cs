﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Vehicle
{
    public class SetVehicleFavouriteDto
    {
        public int VehicleId { get; set; }
        public bool IsFavourite { get; set; }
    }
}
