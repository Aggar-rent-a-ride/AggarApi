using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Payment
{
    public class CreateConnectedAccountDto
    {
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string BankAccountRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
    }
}
