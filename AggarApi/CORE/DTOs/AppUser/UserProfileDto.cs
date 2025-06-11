using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.AppUser
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string userName { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }
        public string Role { get; set; }
    }
}
