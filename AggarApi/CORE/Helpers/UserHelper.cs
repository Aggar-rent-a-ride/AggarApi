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
        private static readonly ILogger _logger;
        public static int GetUserId(ClaimsPrincipal User)
        {
            string? userClaim = User?.FindFirst(c => c.Type == "uid")?.Value;

            if (int.TryParse(userClaim, out int userId))
                return userId;
            else
            {
                _logger.LogError("Invalid or missing user ID claim");
                return -1;
            }
        }
    }
}
