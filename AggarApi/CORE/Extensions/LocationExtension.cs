using DATA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Extensions
{
    public static class LocationExtension
    {
        private const double EarthRadiusKm = 6371.0;

        public static double CalculateDistance(this Location thisLocation, Location anotherLocation)
        {
            double distanceLat = (anotherLocation.Latitude - thisLocation.Latitude) * Math.PI / 180.0;
            double distanceLon = (anotherLocation.Longitude - thisLocation.Longitude) * Math.PI / 180.0;

            thisLocation.Latitude = thisLocation.Latitude * Math.PI / 180.0;
            anotherLocation.Latitude = anotherLocation.Latitude * Math.PI / 180.0;

            double a = Math.Sin(distanceLat / 2) * Math.Sin(distanceLat / 2) +
                       Math.Cos(thisLocation.Latitude) * Math.Cos(anotherLocation.Latitude) *
                       Math.Sin(distanceLon / 2) * Math.Sin(distanceLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }
    }
}
