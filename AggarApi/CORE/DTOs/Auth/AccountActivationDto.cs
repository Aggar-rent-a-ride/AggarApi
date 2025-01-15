using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Auth
{
    public class AccountActivationDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string ActivationCode { get; set; }
    }
}
