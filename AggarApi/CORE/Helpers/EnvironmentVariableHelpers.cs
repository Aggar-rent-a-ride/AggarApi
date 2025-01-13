using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public static class EnvironmentVariableHelpers
    {
        public static string EmailAddress => Environment.GetEnvironmentVariable("EMAIL_ADDRESS");
        public static string EmailPassword => Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    }
}
