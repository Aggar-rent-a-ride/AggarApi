using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public static class UserHelper
    {
        public static int GetUserId(ClaimsPrincipal User)
        {
            string? userClaim = User?.FindFirst(c => c.Type == "uid")?.Value;

            int userId = -1;
            int.TryParse(userClaim, out userId);
            
            return userId;
        }
    }
}
