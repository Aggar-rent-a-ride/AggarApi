using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CORE.DTOs.AppUser.Enums;

namespace CORE.DTOs.AppUser
{
    public class PunishUserDto
    {
        public int UserId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PunishmentType Type { get; set; }
        public int? BanDurationInDays { get; set; }
    }
}
