using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Models
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; } //hashed
        [NotMapped]
        public string RawToken { get; set; } 
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => IsExpired == false && RevokedOn == null;
    }
}
