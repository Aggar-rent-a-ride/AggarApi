using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.ValidationAttributes
{
    public class PastDateAttribute : ValidationAttribute
    {
        public PastDateAttribute() : base("The date cannot be in the future.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null) return false;


            if (value is DateTime dateValue)
            {
                return dateValue < DateTime.UtcNow;
            }
            else if (value is DateOnly dateOnlyValue)
            {
                return dateOnlyValue < DateOnly.FromDateTime(DateTime.UtcNow);
            }

            return false;
        }
    }
}
