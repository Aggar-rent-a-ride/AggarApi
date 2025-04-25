using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class CreateConnectedAccount
    {
        [Required] 
        public string Email { get; set; }
        [Required] 
        public string FirstName { get; set; }
        [Required] 
        public string LastName { get; set; }
        [Required] 
        public DateTime DateOfBirth { get; set; }
        [Required] 
        public string Phone { get; set; }
        [Required] 
        public string AddressLine1 { get; set; }
        [Required] 
        public string City { get; set; }
        [Required] 
        public string PostalCode { get; set; }
        [Required] 
        public string Country { get; set; }

        [Required] 
        public string RoutingNumber { get; set; }
        [Required] 
        public string AccountNumber { get; set; }
    }
}
